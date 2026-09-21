using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SaileachStudios.Mirlini.Board;

[Serializable] public sealed class AuditedWall { public string sourceId; public Vector3 center,size; public bool reveal; }
[Serializable] public sealed class AuditedLevel {
    public int number,cornerPosts; public string sourceGuid,sourceHash,displayName;
    public Vector3 start,goal,ring; public bool unlock,dark; public float historicalTime;
    public LevelInstructions instructions; public AuditedWall[] walls;
}
[Serializable] public sealed class AuditedLevels { public int version; public AuditedLevel[] levels; }
public static class ProductionLevelImporter {
    public const string Intermediate="Docs/Phase2/representatives.json";
    public const string FullIntermediate="Docs/Phase3/full-campaign.json";
    public const string FullCampaignPath="Assets/Levels/Production Campaign.asset";
    public const string Output="Assets/Levels/Production";
    public const string ProfilePath="Assets/Levels/Shared Dark.asset";
    public const string CampaignPath="Assets/Levels/Representative Campaign.asset";
    public static LevelData Normalize(AuditedLevel source,DarkLightingProfile profile) {
        if(source==null || string.IsNullOrEmpty(source.sourceGuid) || source.sourceGuid.Length!=32 || source.walls==null || source.instructions==null)
            throw new ArgumentException("Incomplete audited source.");
        var level=ScriptableObject.CreateInstance<LevelData>();
        try {
            level.LevelId="mirlini-"+source.sourceGuid;level.LevelName=source.displayName;
            level.name="Level "+source.number;level.Instructions=JsonUtility.FromJson<LevelInstructions>(JsonUtility.ToJson(source.instructions));
            level.MarbleStartPosition=Point(source.start);level.HolePosition=Point(source.goal);
            level.Unlock.Enabled=source.unlock;level.Unlock.RingPosition=source.unlock?Point(source.ring):Vector3.zero;
            level.Dark=source.dark?profile:null;
            int duplicates=0,snapped=0;
            foreach(var wall in source.walls) {
                bool alongZ=wall.size.z>wall.size.x;
                float rawLength=alongZ?wall.size.z:wall.size.x;int length=Mathf.RoundToInt(rawLength);
                if(length<1 || Mathf.Abs(rawLength-length)>.001f) throw new ArgumentException("Non-unit source span: "+wall.sourceId);
                for(int j=0;j<length;j++) {
                    float offset=j-(length-1)*.5f;
                    var logical=new BoardPoint(wall.center.x+(alongZ?0:offset),wall.center.z+(alongZ?offset:0));
                    int edge;
                    if(!BoardGrid.TryGetEdgeIndex(logical,alongZ,out edge)) {
                        // Only the explicitly audited Level 40 exception is accepted.
                        if(source.number!=40 || wall.sourceId!="1841613894/3315767750148257856" ||
                            !BoardGrid.TrySnapEdge(logical,alongZ,out edge)) throw new ArgumentException("Unknown off-grid wall: "+wall.sourceId);
                        BoardPoint target=BoardGrid.GetEdge(edge).Center;
                        if(Mathf.Abs(target.X-logical.X)>.051f || Mathf.Abs(target.Z-logical.Z)>.051f) throw new ArgumentException("Offset exceeds known correction.");
                        snapped++;
                    }
                    WallState state=wall.reveal?WallState.RevealOnCollision:WallState.Solid;
                    if(level.Walls[edge]!=WallState.Empty) {
                        if(level.Walls[edge]!=state) throw new ArgumentException("Conflicting source states on edge "+edge);
                        duplicates++;
                    }
                    level.Walls[edge]=state;
                }
            }
            level.MigrationSourceGuid=source.sourceGuid;level.MigrationSourceHash=source.sourceHash;
            level.MigrationLevelNumber=source.number;level.HistoricalIdealTime=source.historicalTime;
            level.MigrationNotes=$"Deduplicated edge occurrences: {duplicates}; snapped segments: {snapped}; omitted historical posts: {source.cornerPosts}.";
            if(source.dark && profile==null) throw new ArgumentException("Dark profile required.");
            if(!LevelDataValidation.TryValidate(level,out string error)) throw new ArgumentException($"Level {source.number}: {error}");
            if(!LevelTraversal.CanReachObjectives(level)) throw new ArgumentException($"Level {source.number}: start cannot reach goal/ring with marble clearance.");
            return level;
        } catch { UnityEngine.Object.DestroyImmediate(level);throw; }
    }
    private static Vector3 Point(Vector3 source) {
        if(!BoardGrid.IsFinite(source.x) || !BoardGrid.IsFinite(source.y) || !BoardGrid.IsFinite(source.z)) throw new ArgumentException("Non-finite source point.");
        BoardPoint p=BoardGrid.ToWorld(BoardGrid.SnapPlacement(new BoardPoint(source.x,source.z)));
        return new Vector3(p.X,BoardGrid.PlayY,p.Z);
    }
    public static AuditedLevels ReadSource(string path=Intermediate) {
        var result=JsonUtility.FromJson<AuditedLevels>(File.ReadAllText(path));
        if(result==null || result.version!=1 || result.levels==null || result.levels.Length==0) throw new ArgumentException("Invalid intermediate version or level list.");
        return result;
    }
    [MenuItem("Mirlini/Migration/Preview representative import")]
    public static void Preview() { Import(false); }
    [MenuItem("Mirlini/Migration/Import representative levels")]
    public static void Apply() { Import(true); }
    [MenuItem("Mirlini/Migration/Preview full production import")]
    public static void PreviewFull() { Import(false,true); }
    [MenuItem("Mirlini/Migration/Import full production campaign")]
    public static void ApplyFull() { Import(true,true); }
    public static void ValidateFullBatch(IReadOnlyList<LevelData> levels) {
        if(levels.Count!=50 || !levels.Select(x=>x.MigrationLevelNumber).SequenceEqual(Enumerable.Range(1,50)))
            throw new ArgumentException("Full campaign must contain exactly Level 1 through Level 50 in order.");
        int[] expected={17,9,8,5,7,4,0,0};
        var actual=new int[8];
        foreach(var level in levels) actual[(level.Unlock.Enabled?1:0)|(level.Dark!=null?2:0)|(level.Walls.Contains(WallState.RevealOnCollision)?4:0)]++;
        if(!actual.SequenceEqual(expected)) throw new ArgumentException("Full campaign mechanics differ from the verified audit distribution.");
    }
    public static void Import(bool write,bool full=false) {
        var profile=AssetDatabase.LoadAssetAtPath<DarkLightingProfile>(ProfilePath);
        if(profile==null) throw new InvalidOperationException("Create the shared Dark profile before importing.");
        var normalized=new List<LevelData>();
        try {
            var errors=new List<Exception>();
            foreach(var source in ReadSource(full?FullIntermediate:Intermediate).levels.OrderBy(x=>x.number)) {
                try { normalized.Add(Normalize(source,profile)); }
                catch(ArgumentException sourceError) { errors.Add(new ArgumentException($"Level {source.number}: {sourceError.Message}",sourceError)); }
            }
            if(errors.Count>0) throw new AggregateException("Batch rejected before asset writes.",errors);
            if(full) ValidateFullBatch(normalized);
            if(!LevelDataValidation.TryValidateCampaign(normalized,out string error)) throw new ArgumentException(error);
            // Validate the entire batch and existing identities before any write.
            foreach(var level in normalized) {
                var existing=AssetDatabase.LoadAssetAtPath<LevelData>(PathFor(level));
                if(existing!=null && existing.LevelId!=level.LevelId) throw new ArgumentException("Existing asset has a different identity: "+PathFor(level));
                foreach(string guid in AssetDatabase.FindAssets("t:LevelData")) {
                    string path=AssetDatabase.GUIDToAssetPath(guid);
                    var other=AssetDatabase.LoadAssetAtPath<LevelData>(path);
                    if(path!=PathFor(level) && other.LevelId==level.LevelId) throw new ArgumentException("Identity already exists at "+path);
                }
            }
            string report="| Level | Stable ID | Solid | Reveal | Start | Goal | Ring | Features | Normalization | Validation | Traversal |\n|---|---|---:|---:|---|---|---|---|---|---|---|\n";
            var assets=new List<LevelData>();
            foreach(var level in normalized) {
                string path=PathFor(level);var existing=AssetDatabase.LoadAssetAtPath<LevelData>(path);
                bool changed=existing==null || JsonUtility.ToJson(existing)!=JsonUtility.ToJson(level);
                Debug.Log($"{(write?"Import":"Preview")} {path}: {(existing==null?"new":changed?"changed":"unchanged")}");
                report+=$"| {level.MigrationLevelNumber} | {level.LevelId} | {level.Walls.Count(x=>x==WallState.Solid)} | {level.Walls.Count(x=>x==WallState.RevealOnCollision)} | {level.MarbleStartPosition} | {level.HolePosition} | {(level.Unlock.Enabled?level.Unlock.RingPosition.ToString():"—")} | {FeatureName(level)} | {level.MigrationNotes} | Pass | Pass |\n";
                if(!write) continue;
                if(existing==null) {existing=ScriptableObject.CreateInstance<LevelData>();EditorUtility.CopySerialized(level,existing);AssetDatabase.CreateAsset(existing,path);}
                else if(changed) {EditorUtility.CopySerialized(level,existing);EditorUtility.SetDirty(existing);}
                assets.Add(existing);
            }
            if(write) {
                string campaignPath=full?FullCampaignPath:CampaignPath;
                var campaign=AssetDatabase.LoadAssetAtPath<LevelCampaign>(campaignPath);
                if(campaign==null) {campaign=ScriptableObject.CreateInstance<LevelCampaign>();AssetDatabase.CreateAsset(campaign,campaignPath);}
                if(!campaign.Levels.SequenceEqual(assets)) {campaign.Levels=assets.ToArray();EditorUtility.SetDirty(campaign);}
                AssetDatabase.SaveAssets();File.WriteAllText(full?"Docs/Phase3/Migration.md":"Docs/Phase2/Migration.md",report);
            }
        } finally { foreach(var level in normalized) UnityEngine.Object.DestroyImmediate(level); }
    }
    private static string FeatureName(LevelData level) {
        var names=new List<string>();
        if(level.Unlock.Enabled) names.Add("Unlock");
        if(level.Dark!=null) names.Add("Dark");
        if(level.Walls.Contains(WallState.RevealOnCollision)) names.Add("Reveal");
        return names.Count==0?"Normal":string.Join(" + ",names);
    }
    private static string PathFor(LevelData level) => Output+"/Level "+level.MigrationLevelNumber.ToString("00")+".asset";
    // One-time scene authoring; never repairs components during gameplay.
    public static void PreparePhase2() {
        Directory.CreateDirectory(Output);AssetDatabase.Refresh();
        if(AssetDatabase.LoadAssetAtPath<DarkLightingProfile>(ProfilePath)==null)
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DarkLightingProfile>(),ProfilePath);
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Sandbox.unity");
        var manager=UnityEngine.Object.FindAnyObjectByType<LevelManager>();
        if(manager.GetComponent<LevelFeaturesBehavior>()==null) manager.gameObject.AddComponent<LevelFeaturesBehavior>();
        EditorSceneManager.SaveScene(scene);
        Apply();
    }
    [MenuItem("Mirlini/Play representative campaign in Sandbox")]
    public static void PlayRepresentatives() { PlayCampaign(CampaignPath); }
    [MenuItem("Mirlini/Play full production campaign in Sandbox")]
    public static void PlayFullCampaign() { PlayCampaign(FullCampaignPath); }
    private static void PlayCampaign(string campaignPath) {
        if(EditorApplication.isPlaying) {Debug.LogWarning("Exit Play Mode before selecting a campaign.");return;}
        var campaign=AssetDatabase.LoadAssetAtPath<LevelCampaign>(campaignPath);
        if(campaign==null || !LevelDataValidation.TryValidateCampaign(campaign.Levels,out _)) {
            Debug.LogError("Select a valid imported campaign before playing.");return;
        }
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Sandbox.unity");
        var manager=UnityEngine.Object.FindAnyObjectByType<LevelManager>();
        Undo.RecordObject(manager,"Select representative campaign");
        var serialized=new SerializedObject(manager);
        serialized.FindProperty("campaign").objectReferenceValue=campaign;
        serialized.ApplyModifiedProperties();EditorApplication.isPlaying=true;
    }
}
