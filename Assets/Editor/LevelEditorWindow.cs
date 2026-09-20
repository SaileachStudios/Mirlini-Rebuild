using System.Linq;
using UnityEngine;
using UnityEditor;
using SaileachStudios.Mirlini.Board;

public class LevelEditorWindow : EditorWindow
{
    private LevelData current;
    private Vector2 scroll;
    private bool snapPositions = true;
    private const string Folder = "Assets/Levels";

    [MenuItem("Mirlini/Level Editor")]
    public static void ShowWindow() { GetWindow<LevelEditorWindow>("Mirlini Level Editor"); }
    private void OnGUI() {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        current = (LevelData)EditorGUILayout.ObjectField("Level", current, typeof(LevelData), false);
        if (GUILayout.Button("Create level")) CreateLevel();
        if (current == null) { EditorGUILayout.EndScrollView(); return; }
        if (current.Unlock == null || current.Instructions == null) {
            EditorGUILayout.HelpBox("Missing feature or instruction data. Repair explicitly before editing.",MessageType.Error);
            if(GUILayout.Button("Repair missing configuration")) {
                Undo.RecordObject(current,"Repair level configuration");
                if(current.Unlock==null) current.Unlock=new UnlockFeature();
                if(current.Instructions==null) current.Instructions=new LevelInstructions();
                EditorUtility.SetDirty(current);
            }
            EditorGUILayout.EndScrollView();return;
        }
        EditorGUI.BeginChangeCheck();
        string name = EditorGUILayout.TextField("Display name", current.LevelName);
        if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(current,"Rename level"); current.LevelName=name; EditorUtility.SetDirty(current); }
        EditorGUILayout.LabelField("Permanent ID", current.LevelId ?? "Missing");
        if (string.IsNullOrWhiteSpace(current.LevelId) && GUILayout.Button("Assign permanent ID")) {
            Undo.RecordObject(current,"Assign level identity");current.LevelId=System.Guid.NewGuid().ToString("N");EditorUtility.SetDirty(current);
        }
        if (GUILayout.Button("Validate level library identities")) {
            var library=AssetDatabase.FindAssets("t:LevelData").Select(g=>AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(g)));
            if(LevelDataValidation.TryValidateCampaign(library,out string libraryError)) Debug.Log("Level library validation passed.");
            else Debug.LogError(libraryError);
        }
        var serialized = new SerializedObject(current);
        serialized.Update();
        EditorGUILayout.PropertyField(serialized.FindProperty("Instructions"),true);
        EditorGUILayout.PropertyField(serialized.FindProperty("Unlock").FindPropertyRelative("Enabled"),new GUIContent("Unlock"));
        EditorGUILayout.PropertyField(serialized.FindProperty("Dark"),new GUIContent("Dark profile (optional)"));
        serialized.ApplyModifiedProperties();
        EditorGUILayout.LabelField("Star baseline", "Needs calibration");
        snapPositions = EditorGUILayout.Toggle("Snap to half logical unit", snapPositions);
        EditPosition("Marble start (logical X/Z)", 0);
        EditPosition("Goal (logical X/Z)", 1);
        if(current.Unlock.Enabled) EditPosition("Unlock ring (logical X/Z)",2);
        EditorGUILayout.LabelField("Edges: click to cycle Empty → Solid → Reveal (magenta)");
        EditorGUILayout.HelpBox("Numeric coordinates are board-local logical units. Disable snapping for exact numeric placement. Walls always occupy a valid edge. Top is +Z; right is +X.", MessageType.Info);
        if (current.Walls == null || current.Walls.Length != BoardGrid.EdgeCount) {
            EditorGUILayout.HelpBox($"Expected {BoardGrid.EdgeCount} wall entries. Repair preserves entries that fit.", MessageType.Error);
            if (GUILayout.Button("Repair wall array")) {
                Undo.RecordObject(current,"Repair wall array");
                System.Array.Resize(ref current.Walls,BoardGrid.EdgeCount);
                EditorUtility.SetDirty(current);
            }
        } else {
            if (!LevelDataValidation.TryValidate(current,out string error)) EditorGUILayout.HelpBox(error,MessageType.Warning);
            DrawBoard();
        }
        EditorGUILayout.EndScrollView();
    }
    private void CreateLevel() {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets","Levels");
        var level=ScriptableObject.CreateInstance<LevelData>();
        level.LevelName="New level";level.LevelId=System.Guid.NewGuid().ToString("N");
        level.MarbleStartPosition=new Vector3(-BoardGrid.HalfExtent+BoardGrid.CellSize/2,0,-BoardGrid.HalfExtent+BoardGrid.CellSize/2);
        level.HolePosition=-level.MarbleStartPosition;
        AssetDatabase.CreateAsset(level,AssetDatabase.GenerateUniqueAssetPath(Folder+"/New Level.asset"));
        Undo.RegisterCreatedObjectUndo(level,"Create level");
        current=level; Selection.activeObject=level;
    }
    private void EditPosition(string label,int target) {
        Vector3 value=target==0?current.MarbleStartPosition:target==1?current.HolePosition:current.Unlock.RingPosition;
        BoardPoint logical=BoardGrid.ToLogical(new BoardPoint(value.x,value.z));
        EditorGUI.BeginChangeCheck();
        Vector2 edited=EditorGUILayout.Vector2Field(label,new Vector2(logical.X,logical.Z));
        if (!EditorGUI.EndChangeCheck()) return;
        var point=new BoardPoint(edited.x,edited.y);
        if (snapPositions) point=BoardGrid.SnapPlacement(point);
        BoardPoint world=BoardGrid.ToWorld(point);
        Undo.RecordObject(current,"Place level point");
        Vector3 result=new Vector3(world.X,BoardGrid.PlayY,world.Z);
        if(target==0) current.MarbleStartPosition=result;else if(target==1) current.HolePosition=result;else current.Unlock.RingPosition=result;
        EditorUtility.SetDirty(current);
    }
    private void DrawBoard() {
        float size=Mathf.Max(240,Mathf.Min(600,position.width-40));
        Rect rect=GUILayoutUtility.GetRect(size,size,GUILayout.ExpandWidth(false));
        EditorGUI.DrawRect(rect,new Color(.85f,.85f,.82f));
        for(int i=0;i<BoardGrid.EdgeCount;i++) {
            BoardEdge edge=BoardGrid.GetEdge(i);
            BoardPoint world=BoardGrid.ToWorld(edge.Center);
            Vector2 center=ToGUI(rect,world);
            float length=BoardGrid.CellSize/(2*BoardGrid.HalfExtent)*rect.width;
            Rect button=new Rect(center.x-(edge.AlongZ?3:length/2),center.y-(edge.AlongZ?length/2:3),edge.AlongZ?6:length,edge.AlongZ?length:6);
            Color previous=GUI.backgroundColor;
            GUI.backgroundColor=current.Walls[i]==WallState.Solid?Color.black:current.Walls[i]==WallState.RevealOnCollision?Color.magenta:Color.white;
            if(GUI.Button(button,GUIContent.none)) {
                Undo.RecordObject(current,"Toggle wall edge");
                current.Walls[i]=(WallState)(((int)current.Walls[i]+1)%3); EditorUtility.SetDirty(current);
            }
            GUI.backgroundColor=previous;
        }
        DrawMarker(rect,current.MarbleStartPosition,"S",Color.green);
        DrawMarker(rect,current.HolePosition,"G",Color.cyan);
        if(current.Unlock.Enabled) DrawMarker(rect,current.Unlock.RingPosition,"U",Color.yellow);
    }
    private static Vector2 ToGUI(Rect rect,BoardPoint world) => new Vector2(rect.x+(world.X/BoardGrid.HalfExtent+1)*rect.width/2,rect.y+(1-world.Z/BoardGrid.HalfExtent)*rect.height/2);
    private static void DrawMarker(Rect rect,Vector3 world,string label,Color color) {
        if(!BoardGrid.IsFinite(world.x) || !BoardGrid.IsFinite(world.z)) return;
        Vector2 p=ToGUI(rect,new BoardPoint(world.x,world.z));
        Rect marker=new Rect(p.x-7,p.y-7,14,14);
        EditorGUI.DrawRect(marker,color); GUI.Label(marker,label);
    }
}
