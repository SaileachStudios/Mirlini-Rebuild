using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.Core;

public sealed class GameplayFeelReview : EditorWindow {
    private static readonly int[] Numbers={1,2,46,7,11,16,26,33,48,50};
    private static readonly string[] Labels={"1 — Open movement","2 — Ordinary maze","46 — Dense precision","7 — Unlock",
        "11 — Dark","16 — Reveal","26 — Unlock + Dark","33 — Unlock + Reveal","48 — Goal visibility","50 — Final maze"};
    [SerializeField] private int selected;
    [SerializeField] private bool pending;
    [MenuItem("Mirlini/Phase 4 playtest")]
    public static void Open() { GetWindow<GameplayFeelReview>("Mirlini playtest"); }
    private void OnGUI() {
        EditorGUILayout.HelpBox("Human review candidate. Check speed, corrections, stopping, wall contact, readability and comfort. No timing calibration yet.",MessageType.Info);
        selected=EditorGUILayout.Popup("Representative",selected,Labels);
        if(GUILayout.Button(EditorApplication.isPlaying?"Start / restart selected attempt":"Play selected level in Sandbox")) {
            if(EditorApplication.isPlaying) SelectAttempt();
            else if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene("Assets/Scenes/Sandbox.unity");
                var manager=Object.FindAnyObjectByType<LevelManager>();var so=new SerializedObject(manager);
                so.FindProperty("campaign").objectReferenceValue=AssetDatabase.LoadAssetAtPath<LevelCampaign>("Assets/Levels/Production Campaign.asset");
                so.ApplyModifiedPropertiesWithoutUndo();pending=true;EditorApplication.isPlaying=true;
            }
        }
        if(GUILayout.Button("Select shared feel profile")) Selection.activeObject=GameplayFeel.Load();
        EditorGUILayout.HelpBox("Profile values load on entering Play Mode. Restart Play Mode after tuning. TiltDegrees = 0 disables visual tilt. Stop Play Mode to restore the authored Sandbox selection.",MessageType.None);
    }
    private void OnInspectorUpdate() {
        if(!pending || !EditorApplication.isPlaying) return;
        var manager=Object.FindAnyObjectByType<LevelManager>();
        if(manager!=null && manager.CurrentLevelIndex>=0) {pending=false;SelectAttempt();}
    }
    private void SelectAttempt() {
        var manager=Object.FindAnyObjectByType<LevelManager>();
        var ball=Object.FindAnyObjectByType<SaileachStudios.Mirlini.Marble.MarbleBehaviour>();
        if(manager==null || ball==null || !ball.CanStartPlaying) {Debug.LogWarning("Wait for hole recovery before changing attempts.");return;}
        var so=new SerializedObject(manager);so.FindProperty("campaign").objectReferenceValue=AssetDatabase.LoadAssetAtPath<LevelCampaign>("Assets/Levels/Production Campaign.asset");
        so.ApplyModifiedPropertiesWithoutUndo();manager.TrySetupLevel(Numbers[selected]-1);
    }
    // One-time asset creation for this phase, also callable by exact-editor batch verification.
    public static void CreateAssets() {
        if(!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets","Resources");
        if(AssetDatabase.LoadAssetAtPath<GameplayFeel>("Assets/Resources/Gameplay Feel.asset")!=null) return;
        var material=new Material(Shader.Find("Sprites/Default"));material.name="World Feedback";
        AssetDatabase.CreateAsset(material,"Assets/Materials/World Feedback.mat");
        var feel=ScriptableObject.CreateInstance<GameplayFeel>();feel.WorldFeedbackMaterial=material;
        AssetDatabase.CreateAsset(feel,"Assets/Resources/Gameplay Feel.asset");
        var marble=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Marble.mat");
        marble.SetFloat("_Metallic",.25f);marble.SetFloat("_Glossiness",.5f);
        marble.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;
        marble.EnableKeyword("_EMISSION");marble.SetColor("_EmissionColor",new Color(.06f,.07f,.08f));
        EditorUtility.SetDirty(marble);AssetDatabase.SaveAssets();
    }
}
