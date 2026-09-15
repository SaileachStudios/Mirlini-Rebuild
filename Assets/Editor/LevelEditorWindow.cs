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
        EditorGUI.BeginChangeCheck();
        string name = EditorGUILayout.TextField("Display name", current.LevelName);
        if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(current,"Rename level"); current.LevelName=name; EditorUtility.SetDirty(current); }
        EditorGUILayout.LabelField("Star baseline", "Needs calibration (legacy time is unused)");
        EditorGUILayout.LabelField("Mechanics", current.Type + " — schema editing follows Phase 1");
        snapPositions = EditorGUILayout.Toggle("Snap to half logical unit", snapPositions);
        EditPosition("Marble start (logical X/Z)", true);
        EditPosition("Goal (logical X/Z)", false);
        EditorGUILayout.HelpBox("Numeric coordinates are board-local logical units. Disable snapping for exact numeric placement. Walls always occupy a valid edge. Top is +Z; right is +X.", MessageType.Info);
        if (current.wallInfo == null || current.wallInfo.Length != BoardGrid.EdgeCount) {
            EditorGUILayout.HelpBox($"Expected {BoardGrid.EdgeCount} wall entries. Repair preserves entries that fit.", MessageType.Error);
            if (GUILayout.Button("Repair wall array")) {
                Undo.RecordObject(current,"Repair wall array");
                System.Array.Resize(ref current.wallInfo,BoardGrid.EdgeCount);
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
        level.LevelName="New level";
        level.MarbleStartPosition=new Vector3(-BoardGrid.HalfExtent+BoardGrid.CellSize/2,0,-BoardGrid.HalfExtent+BoardGrid.CellSize/2);
        level.HolePosition=-level.MarbleStartPosition;
        AssetDatabase.CreateAsset(level,AssetDatabase.GenerateUniqueAssetPath(Folder+"/New Level.asset"));
        Undo.RegisterCreatedObjectUndo(level,"Create level");
        current=level; Selection.activeObject=level;
    }
    private void EditPosition(string label,bool start) {
        Vector3 value=start?current.MarbleStartPosition:current.HolePosition;
        BoardPoint logical=BoardGrid.ToLogical(new BoardPoint(value.x,value.z));
        EditorGUI.BeginChangeCheck();
        Vector2 edited=EditorGUILayout.Vector2Field(label,new Vector2(logical.X,logical.Z));
        if (!EditorGUI.EndChangeCheck()) return;
        var point=new BoardPoint(edited.x,edited.y);
        if (snapPositions) point=BoardGrid.SnapPlacement(point);
        BoardPoint world=BoardGrid.ToWorld(point);
        Undo.RecordObject(current,"Place "+(start?"marble":"goal"));
        Vector3 result=new Vector3(world.X,BoardGrid.PlayY,world.Z);
        if (start) current.MarbleStartPosition=result; else current.HolePosition=result;
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
            GUI.backgroundColor=current.wallInfo[i]?Color.black:Color.white;
            if(GUI.Button(button,GUIContent.none)) {
                Undo.RecordObject(current,"Toggle wall edge");
                current.wallInfo[i]=!current.wallInfo[i]; EditorUtility.SetDirty(current);
            }
            GUI.backgroundColor=previous;
        }
        DrawMarker(rect,current.MarbleStartPosition,"S",Color.green);
        DrawMarker(rect,current.HolePosition,"G",Color.cyan);
    }
    private static Vector2 ToGUI(Rect rect,BoardPoint world) => new Vector2(rect.x+(world.X/BoardGrid.HalfExtent+1)*rect.width/2,rect.y+(1-world.Z/BoardGrid.HalfExtent)*rect.height/2);
    private static void DrawMarker(Rect rect,Vector3 world,string label,Color color) {
        Vector2 p=ToGUI(rect,new BoardPoint(world.x,world.z));
        Rect marker=new Rect(p.x-7,p.y-7,14,14);
        EditorGUI.DrawRect(marker,color); GUI.Label(marker,label);
    }
}
