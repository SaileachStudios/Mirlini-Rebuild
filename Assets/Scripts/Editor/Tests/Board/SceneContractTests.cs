using System.Linq;
using NUnit.Framework;
using SaileachStudios.Mirlini.Board;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneContractTests
{
    [Test]
    public void SandboxHasAuthoredFlowAndSharedGeometry() {
        var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Sandbox.unity");
        try {
            var level=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<LevelManager>(true)).Single();
            Assert.AreEqual(1,level.GetComponents<LevelFeaturesBehavior>().Length);
            Assert.IsTrue(level.GetComponent<LevelFeaturesBehavior>().enabled);
            var flows=level.GetComponents<LevelEndFlowController>();Assert.AreEqual(1,flows.Length);Assert.IsTrue(flows[0].enabled);
            Assert.AreSame(level,new SerializedObject(flows[0]).FindProperty("levelManager").objectReferenceValue);
            Assert.IsTrue(level.TryValidateGeometry(out string error),error);
            var data=new SerializedObject(level);var walls=data.FindProperty("walls");Assert.AreEqual(BoardGrid.EdgeCount,walls.arraySize);
            for(int i=0;i<walls.arraySize;i++) {
                var wall=(GameObject)walls.GetArrayElementAtIndex(i).objectReferenceValue;
                BoardEdge edge=BoardGrid.GetEdge(i);
                Vector3 expected=BoardGeometryPlacement.Position(BoardGrid.ToWorld(edge.Center),level.transform.position,BoardGrid.WallCenterY);
                Assert.Less(Vector3.Distance(expected,wall.transform.position),.0001f,$"Edge {i}");
                // Preview-scene physics bounds may not be populated; verify authored scale/orientation instead.
                Assert.Less(Vector3.Distance(new Vector3(BoardGrid.WallLength,BoardGrid.WallHeight,BoardGrid.WallThickness),wall.transform.localScale),.0001f);
                Assert.Less(Quaternion.Angle(edge.AlongZ?Quaternion.Euler(0,90,0):Quaternion.identity,wall.transform.rotation),.001f);
            }
        } finally { EditorSceneManager.ClosePreviewScene(scene); }
    }
    [Test]
    public void PrototypeAssetsHaveValidDataAndNoStarBaseline() {
        foreach(string guid in AssetDatabase.FindAssets("t:LevelData",new[]{"Assets/Levels"})) {
            var level=AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guid));
            Assert.IsTrue(LevelDataValidation.TryValidate(level,out string error,.5f),error);
            Assert.AreEqual(StarCalibrationState.NeedsCalibration,level.StarCalibration);
        }
    }
    [Test]
    public void InvalidWallArrayAndCoincidentPointsAreRejected() {
        var data=ScriptableObject.CreateInstance<LevelData>();
        try {
            data.LevelId="test";data.Walls=new WallState[1];Assert.IsFalse(LevelDataValidation.TryValidate(data,out _));
            data.Walls=new WallState[BoardGrid.EdgeCount];Assert.IsFalse(LevelDataValidation.TryValidate(data,out _));
            data.HolePosition=Vector3.one;Assert.IsFalse(LevelDataValidation.TryValidate(data,out _));
        } finally { Object.DestroyImmediate(data); }
    }
}
