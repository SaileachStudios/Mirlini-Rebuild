using UnityEngine;
using UnityEditor;
using SaileachStudios.Mirlini.Board;
using System.IO;

public class LevelEditorWindow : EditorWindow
{
    private LevelData currentLevelData;
    private const string FOLDER_PATH = "Assets/Levels";
    private const float STARTING_INDEX = 13.68f;
    private const float AREA_SPERATION_AMOUNT = 1.824f;
    private float wallThin = 10f;
    private float wallLong = 20f;
    private float areaSize = 20f;
    private int wallIndex = 0;
    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Mirlini/Level Editor")]
    public static void ShowWindow() {
        GetWindow<LevelEditorWindow>("Mirlini Level Editor");
    }

    private void CreateAndSaveLevelData() {
        currentLevelData = ScriptableObject.CreateInstance<LevelData>();

        // Ensure the Levels folder exists
        if (!AssetDatabase.IsValidFolder(FOLDER_PATH)) {
            AssetDatabase.CreateFolder("Assets", "Levels");
        }

        // Create a unique file name
        string assetName = "LevelData_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".asset";
        string assetPath = $"{FOLDER_PATH}/{assetName}";

        // Create the asset file
        AssetDatabase.CreateAsset(currentLevelData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = currentLevelData;
    }

    private string GetAssetFilename(LevelData asset, bool includeExtension = false) {
        string path = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(path)) return null;

        return includeExtension ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);
    }

    private void RenameLevelAsset(LevelData levelData, string newName) {
        string assetPath = AssetDatabase.GetAssetPath(levelData);
        if (!string.IsNullOrEmpty(assetPath)) {
            if (AssetDatabase.LoadAssetAtPath<LevelData>($"{FOLDER_PATH}/{newName}") == null){
                string error = AssetDatabase.RenameAsset(assetPath, newName);
                if (!string.IsNullOrEmpty(error)) {
                    Debug.LogError("Rename failed: " + error);
                }
                else {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
        else {
            Debug.LogWarning("Could not find asset path for the provided object.");
        }
    }

    private void OnGUI() {
        wallIndex = 0;
        GUI.backgroundColor = Color.gray;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);

        if (GUILayout.Button("Add New Level", GUILayout.Width(100))) {
            CreateAndSaveLevelData();
        }

        GUILayout.BeginHorizontal();
        DisplayInfoPanel();
        if (currentLevelData != null) {
            GUILayout.Space(20);
            DisplayMazePanel();
        }
        GUILayout.EndHorizontal();

        if (GUI.changed) {
            EditorUtility.SetDirty(currentLevelData);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DisplayInfoPanel() {
        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
        GUILayout.Label("Information Section");
        currentLevelData = (LevelData)EditorGUILayout.ObjectField("Level data", currentLevelData, typeof(LevelData), false);
        if (currentLevelData != null) {

            currentLevelData.LevelName = EditorGUILayout.TextField("Level Name", currentLevelData.LevelName);
            if (currentLevelData.LevelName != string.Empty && GetAssetFilename(currentLevelData) != currentLevelData.LevelName) {
                RenameLevelAsset(currentLevelData, currentLevelData.LevelName);
            }
            currentLevelData.Type = (LevelType)EditorGUILayout.EnumPopup("Level Type", currentLevelData.Type);
            currentLevelData.IdealCompletionTime = EditorGUILayout.FloatField("Ideal Completion Time", currentLevelData.IdealCompletionTime);
        }
        GUILayout.EndVertical();
    }

    private void DisplayMazePanel() {
        for (float zIndex = STARTING_INDEX; zIndex > -STARTING_INDEX; zIndex -= AREA_SPERATION_AMOUNT) {
            DisplayAreaColumn(zIndex);
            DisplayWallColumn();
        }
        DisplayAreaColumn(-STARTING_INDEX);
    }

    private void DisplayAreaColumn(float zIndex) {

        GUILayout.BeginVertical();
        for (float xIndex = STARTING_INDEX; xIndex > -STARTING_INDEX; xIndex -= AREA_SPERATION_AMOUNT) {
            CreateAreaButton(new Vector3(xIndex, 0f, zIndex));
            CreateHorizontalWall(wallIndex++);
        }
        CreateAreaButton(new Vector3(-STARTING_INDEX, 0f, zIndex));
        GUILayout.EndVertical();
    }

    private void DisplayWallColumn() {
        GUILayout.BeginVertical();
        for (int index = 0; index < 15; index++) {
            CreateVerticalWall(wallIndex++);
            GUILayout.Button("", GUILayout.Height(wallThin), GUILayout.Width(wallThin));
        }
        CreateVerticalWall(wallIndex++);
        GUILayout.EndVertical();
    }

    private void CreateAreaButton(Vector3 areaLocation) {
        var label = " ";
        GUI.backgroundColor = Color.white;
        if( areaLocation == currentLevelData.MarbleStartPosition) {
            GUI.backgroundColor = Color.green;
            label = "S";
        }
        if (areaLocation == currentLevelData.HolePosition) {
            GUI.backgroundColor = Color.blue;
            label = "F";
        }
 
        if (GUILayout.Button(label, GUILayout.Height(areaSize), GUILayout.Width(areaSize))) {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Set as Start"), false, () => {
                currentLevelData.MarbleStartPosition = areaLocation;
                if (currentLevelData.HolePosition == areaLocation) {
                    currentLevelData.HolePosition = Vector3.zero;
                }
            });
            menu.AddItem(new GUIContent("Set as Hole"), false, () => {
                currentLevelData.HolePosition = areaLocation;
                if (currentLevelData.MarbleStartPosition == areaLocation) {
                    currentLevelData.MarbleStartPosition = Vector3.zero;
                }
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Clear"), false, () => {
                if (currentLevelData.HolePosition == areaLocation) {
                    currentLevelData.HolePosition = Vector3.zero;
                }
                if (currentLevelData.MarbleStartPosition == areaLocation) {
                    currentLevelData.MarbleStartPosition = Vector3.zero;
                }
            });

            // Show at the last rect used by GUILayout (i.e. the button)
            Rect buttonRect = GUILayoutUtility.GetLastRect();
            Vector2 buttonScreenPos = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.y));
            buttonRect.position = buttonScreenPos;

            menu.DropDown(buttonRect);
        }
    }

    private void CreateVerticalWall(int index) {
        if (currentLevelData.wallInfo[index]) {
            GUI.backgroundColor = Color.black;
        }
        else {
            GUI.backgroundColor = Color.white;
        }
        if (GUILayout.Button("", GUILayout.Height(wallLong), GUILayout.Width(wallThin))) {
            currentLevelData.wallInfo[index] = !currentLevelData.wallInfo[index];
        }
    }

    private void CreateHorizontalWall(int index) {
        if (currentLevelData.wallInfo[index]) {
            GUI.backgroundColor = Color.black;
        }
        else {
            GUI.backgroundColor = Color.white;
        }
        if (GUILayout.Button("", GUILayout.Height(wallThin), GUILayout.Width(wallLong))) {
            currentLevelData.wallInfo[index] = !currentLevelData.wallInfo[index];
        }
    }
}
