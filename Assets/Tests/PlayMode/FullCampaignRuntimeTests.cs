using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.Marble;
using SaileachStudios.Mirlini.Core;
public class FullCampaignRuntimeTests : SandboxPlayTest {
    [UnityTest] public IEnumerator FullCampaignConfiguresFiftyAttemptsAndStopsAtItsEnd() {
        var campaign=AssetDatabase.LoadAssetAtPath<LevelCampaign>("Assets/Levels/Production Campaign.asset");
        var manager=Find<LevelManager>();var serialized=new SerializedObject(manager);
        serialized.FindProperty("campaign").objectReferenceValue=campaign;serialized.ApplyModifiedPropertiesWithoutUndo();
        var walls=serialized.FindProperty("walls");var features=Find<LevelFeaturesBehavior>();
        var ball=Find<MarbleBehaviour>();var goal=Find<HoleBehavior>();LevelAttempt previous=null;
        string[] snapshots=campaign.Levels.Select(EditorJsonUtility.ToJson).ToArray();
        for(int index=0;index<50;index++) {
            var data=campaign.Levels[index];Assert.IsTrue(manager.TrySetupLevel(index),"Setup "+(index+1));
            Assert.AreEqual(index,manager.CurrentLevelIndex);Assert.AreEqual(BallState.Playing,ball.CurrentState);
            Assert.AreNotSame(previous,features.Attempt);previous=features.Attempt;
            Assert.AreEqual(!data.Unlock.Enabled,goal.IsCorrect);
            Assert.AreEqual(data.Unlock.Enabled,features.Ring!=null && features.Ring.gameObject.activeSelf);
            Assert.AreEqual(data.Dark!=null,features.Spotlight!=null && features.Spotlight.enabled);
            Assert.Less(Vector3.Distance(manager.transform.position+data.MarbleStartPosition,ball.transform.position),.00001f);
            for(int edge=0;edge<BoardGrid.EdgeCount;edge++) {
                var wall=(GameObject)walls.GetArrayElementAtIndex(edge).objectReferenceValue;
                Assert.AreEqual(data.Walls[edge]!=WallState.Empty,wall.activeSelf,$"Level {index+1}, edge {edge}");
                if(data.Walls[edge]==WallState.Empty) continue;
                Assert.IsTrue(wall.GetComponent<Collider>().enabled);Assert.IsFalse(wall.GetComponent<Collider>().isTrigger);
                Assert.AreEqual(data.Walls[edge]==WallState.Solid,wall.GetComponent<Renderer>().enabled);
            }
            yield return new WaitForFixedUpdate();yield return null;
            if(data.Dark!=null) {
                Assert.AreEqual(ball.transform.position.x,features.Spotlight.transform.position.x,.001f);
                Assert.AreEqual(ball.transform.position.z,features.Spotlight.transform.position.z,.001f);
            }
            CaptureForReview(index+1,walls);
        }
        Assert.IsFalse(manager.LoadNextLevel());Assert.AreEqual(49,manager.CurrentLevelIndex);
        GameManagerBehavior.Instance.Events.LevelCompleted();yield return new WaitForSeconds(.2f);
        Assert.AreEqual(49,manager.CurrentLevelIndex);
        CollectionAssert.AreEqual(snapshots,campaign.Levels.Select(EditorJsonUtility.ToJson).ToArray());
    }
    // Optional evidence capture from the actual Sandbox camera, never required in headless CI.
    // Pass -mirliniReviewDirectory <absolute directory> with graphics enabled.
    private static void CaptureForReview(int number,SerializedProperty walls) {
        var args=Environment.GetCommandLineArgs();int flag=Array.IndexOf(args,"-mirliniReviewDirectory");
        if(flag<0) return;
        Assert.Less(flag+1,args.Length);Assert.AreNotEqual(UnityEngine.Rendering.GraphicsDeviceType.Null,SystemInfo.graphicsDeviceType);
        string folder=args[flag+1];Directory.CreateDirectory(folder);
        var camera=UnityEngine.Object.FindAnyObjectByType<Camera>();Assert.IsNotNull(camera);
        SaveCamera(camera,Path.Combine(folder,$"Level-{number:00}.png"));
        // A separately labelled diagnostic view makes hidden wall topology reviewable.
        var hidden=new System.Collections.Generic.List<Renderer>();
        for(int edge=0;edge<walls.arraySize;edge++) {
            var wall=(GameObject)walls.GetArrayElementAtIndex(edge).objectReferenceValue;
            if(!wall.activeSelf) continue;
            var renderer=wall.GetComponent<Renderer>();if(renderer.enabled) continue;
            hidden.Add(renderer);renderer.enabled=true;
        }
        try {if(hidden.Count>0) SaveCamera(camera,Path.Combine(folder,$"Level-{number:00}-layout-diagnostic.png"));}
        finally {foreach(var renderer in hidden) renderer.enabled=false;}
    }
    private static void SaveCamera(Camera camera,string path) {
        var previous=camera.targetTexture;var active=RenderTexture.active;
        var target=new RenderTexture(960,960,24);var texture=new Texture2D(960,960,TextureFormat.RGB24,false);
        try {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            texture.ReadPixels(new Rect(0,0,960,960),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
        } finally {
            camera.targetTexture=previous;RenderTexture.active=active;target.Release();
            UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(texture);
        }
    }
}
