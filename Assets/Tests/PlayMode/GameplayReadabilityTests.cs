using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.Core;
using SaileachStudios.Mirlini.Marble;
using Object=UnityEngine.Object;
public class GameplayReadabilityTests : SandboxPlayTest {
    private void Select(int number) {
        var manager=Find<LevelManager>();var so=new SerializedObject(manager);
        so.FindProperty("campaign").objectReferenceValue=AssetDatabase.LoadAssetAtPath<LevelCampaign>("Assets/Levels/Production Campaign.asset");
        so.ApplyModifiedPropertiesWithoutUndo();Assert.IsTrue(manager.TrySetupLevel(number-1));
    }
    private void Place(Vector3 position) {
        var ball=Find<MarbleBehaviour>();var body=ball.GetComponent<Rigidbody>();
        body.position=position;ball.transform.position=position;body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;Physics.SyncTransforms();
    }
    [UnityTest] public IEnumerator RingProgressResetCompletionAndReplayMatchAttempt() {
        Select(7);var features=Find<LevelFeaturesBehavior>();var ring=features.Ring;
        var visual=ring.GetComponent<UnlockRingPresentation>();var beacon=Find<HoleBehavior>().GetComponent<GoalPresentation>();
        Assert.IsFalse(beacon.IsUnlocked);Place(ring.transform.position);yield return new WaitForSeconds(.7f);
        Assert.Greater(visual.Progress01,.2f);Assert.Less(visual.Progress01,.8f);Assert.IsFalse(visual.Completed);Capture("07-progress");
        GameManagerBehavior.Instance.SetPaused(true);float progress=visual.Progress01;yield return new WaitForSeconds(.1f);Assert.AreEqual(progress,visual.Progress01);
        GameManagerBehavior.Instance.SetPaused(false);Place(new Vector3(-12.6f,0,-12.6f));yield return new WaitForFixedUpdate();yield return null;
        Assert.AreEqual(0,visual.Progress01);Capture("07-reset");
        Place(ring.transform.position);yield return new WaitForSeconds(2.15f);
        Assert.IsTrue(visual.Completed);Assert.AreEqual(1,visual.Progress01);Assert.IsTrue(beacon.IsUnlocked);Capture("07-complete");
        Select(7);yield return null;Assert.IsFalse(visual.Completed);Assert.AreEqual(0,visual.Progress01);Assert.IsFalse(beacon.IsUnlocked);
        Select(1);Assert.IsFalse(ring.gameObject.activeSelf);Assert.AreEqual(0,visual.Progress01);Assert.IsTrue(beacon.IsUnlocked);
    }
    [UnityTest] public IEnumerator RevealHighlightSettlesAndReplayRemovesItWithoutChangingCollider() {
        Select(16);var manager=Find<LevelManager>();var so=new SerializedObject(manager);
        var data=AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Levels/Production/Level 16.asset");
        int index=Array.FindIndex(data.Walls,s=>s==WallState.RevealOnCollision);
        var wall=(GameObject)so.FindProperty("walls").GetArrayElementAtIndex(index).objectReferenceValue;
        var box=wall.GetComponent<BoxCollider>();var visual=wall.GetComponent<Renderer>();var reveal=wall.GetComponent<RevealWall>();
        var size=box.size;var scale=wall.transform.localScale;
        Physics.SyncTransforms();var bounds=box.bounds;
        // Contact the thin face for either orientation; approved collision filtering remains exercised.
        Vector3 contact=bounds.size.x>bounds.size.z?new Vector3(bounds.center.x,0,bounds.max.z+.49f):new Vector3(bounds.max.x+.49f,0,bounds.center.z);
        Place(contact);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
        Assert.IsTrue(visual.enabled);Assert.IsTrue(reveal.IsHighlighting);Capture("16-discovery");
        yield return new WaitForSeconds(.4f);Assert.IsFalse(reveal.IsHighlighting);Assert.IsTrue(visual.enabled);Capture("16-settled");
        Assert.AreEqual(size,box.size);Assert.AreEqual(scale,wall.transform.localScale);Assert.IsTrue(box.enabled);Assert.IsFalse(box.isTrigger);
        Assert.IsTrue(manager.TryCallForHelp());Assert.IsTrue(Find<LevelFeaturesBehavior>().Attempt.IsRevealed(index));
        Select(16);Assert.IsFalse(visual.enabled);Assert.IsFalse(reveal.IsHighlighting);Assert.IsTrue(box.enabled);
    }
    [UnityTest] public IEnumerator SharedMovementAcceleratesStopsAndKeepsAnalogInRealPhysics() {
        Select(1);var game=GameManagerBehavior.Instance;game.enabled=false; // deterministic test input, not keyboard polling
        var body=Find<MarbleBehaviour>().GetComponent<Rigidbody>();
        float[] speeds=new float[3], stops=new float[3], reversals=new float[3];
        // Recorded Phase 3 controller, on the same flat physical rig, for a controlled comparison.
        var baseline=new MarbleController(8,12);
        for(int sample=0;sample<3;sample++) {
            Place(new Vector3(-12.6f,0,-12.6f));
            Action<Vector2> drive=input=> {
                if(sample==0) {
                    baseline.CalculateTargetVelocity(input);
                    Vector3 v=body.linearVelocity+baseline.CalculateForceToReachTarget(body.linearVelocity)*Time.fixedDeltaTime;
                    Vector3 h=Vector3.ClampMagnitude(new Vector3(v.x,0,v.z),15);
                    body.linearVelocity=new Vector3(h.x,body.linearVelocity.y,h.z);
                } else game.Events.InputUpdated(false,input);
            };
            Vector2 held=new Vector2(sample==1?.25f:1,0);
            for(int step=0;step<35;step++) {drive(held);yield return new WaitForFixedUpdate();}
            speeds[sample]=body.linearVelocity.x;float x=body.position.x;
            for(int step=0;step<30;step++) {drive(Vector2.zero);yield return new WaitForFixedUpdate();}
            Assert.Less(new Vector2(body.linearVelocity.x,body.linearVelocity.z).magnitude,.05f);
            stops[sample]=body.position.x-x;
            for(int step=0;step<35;step++) {drive(held);yield return new WaitForFixedUpdate();}
            for(int step=0;step<25;step++) {
                drive(-held);yield return new WaitForFixedUpdate();
                if(body.linearVelocity.x<=0) {reversals[sample]=(step+1)*Time.fixedDeltaTime;break;}
            }
        }
        Assert.Greater(speeds[2],speeds[0]);Assert.That(speeds[1]/speeds[2],Is.InRange(.20f,.30f));
        Assert.That(stops[2],Is.InRange(0f,.8f));Assert.Less(stops[2],stops[0]);
        for(int i=0;i<3;i++) Debug.Log($"PHASE4_PHYSICS sample={i} speed={speeds[i]:F4} releaseDistance={stops[i]:F4} reversalSeconds={reversals[i]:F3}");
        game.enabled=true;
    }
    private static void Capture(string name) {
        var args=Environment.GetCommandLineArgs();int flag=Array.IndexOf(args,"-mirliniReviewDirectory");if(flag<0)return;
        var folder=args[flag+1];Directory.CreateDirectory(folder);var camera=Find<Camera>();
        var previous=camera.targetTexture;var active=RenderTexture.active;var target=new RenderTexture(960,960,24);var texture=new Texture2D(960,960,TextureFormat.RGB24,false);
        try {camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,960,960),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());}
        finally {camera.targetTexture=previous;RenderTexture.active=active;target.Release();Object.Destroy(target);Object.Destroy(texture);}
    }
}
