using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.Marble;
using SaileachStudios.Mirlini.Core;
public class ProductionFeatureTests : SandboxPlayTest {
    private LevelData SelectLevel(int number) {
        var data=AssetDatabase.LoadAssetAtPath<LevelData>($"Assets/Levels/Production/Level {number:00}.asset");
        var manager=Find<LevelManager>();var serialized=new SerializedObject(manager);
        serialized.FindProperty("campaign").objectReferenceValue=null;
        var levels=serialized.FindProperty("levels");levels.arraySize=1;levels.GetArrayElementAtIndex(0).objectReferenceValue=data;
        serialized.ApplyModifiedPropertiesWithoutUndo();Assert.IsTrue(manager.TrySetupLevel(0));return data;
    }
    private void PlaceBall(Vector3 position) {
        var ball=Find<MarbleBehaviour>();var body=ball.GetComponent<Rigidbody>();
        body.position=position;ball.transform.position=position;body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;Physics.SyncTransforms();
    }
    private IEnumerator FailAndRecover() {
        var ball=Find<MarbleBehaviour>();Set(ball,"shrinkDuration",0);
        Assert.IsTrue(ball.TryEnterHole(false,Vector3.zero));yield return null;yield return null;
        Assert.AreEqual(BallState.Playing,ball.CurrentState);
    }
    [UnityTest] public IEnumerator AllRepresentativesBuildCorrectWallStatesWithoutMutatingAssets() {
        foreach(int number in new[]{1,6,10,15,21,27,29,40,45,46,50}) {
            var data=SelectLevel(number);string before=EditorJsonUtility.ToJson(data);
            var serialized=new SerializedObject(Find<LevelManager>());var walls=serialized.FindProperty("walls");
            for(int i=0;i<BoardGrid.EdgeCount;i++) {
                var wall=(GameObject)walls.GetArrayElementAtIndex(i).objectReferenceValue;
                Assert.AreEqual(data.Walls[i]!=WallState.Empty,wall.activeSelf);
                if(data.Walls[i]==WallState.Empty) continue;
                Assert.IsTrue(wall.GetComponent<Collider>().enabled);Assert.IsFalse(wall.GetComponent<Collider>().isTrigger);
                Assert.AreEqual(data.Walls[i]==WallState.Solid,wall.GetComponent<Renderer>().enabled);
            }
            Assert.AreEqual(!data.Unlock.Enabled,Find<HoleBehavior>().IsCorrect);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(before,EditorJsonUtility.ToJson(data));
        }
    }
    [UnityTest] public IEnumerator RevealFiltersCollisionAndSurvivesRecoveryAndHelpUntilReplay() {
        var data=SelectLevel(15);var features=Find<LevelFeaturesBehavior>();var attempt=features.Attempt;
        int edge=System.Array.FindIndex(data.Walls,x=>x==WallState.RevealOnCollision);
        var serialized=new SerializedObject(Find<LevelManager>());
        var wall=(GameObject)serialized.FindProperty("walls").GetArrayElementAtIndex(edge).objectReferenceValue;
        var bounds=wall.GetComponent<Collider>().bounds;
        Vector3 contact=new Vector3(bounds.center.x,0,bounds.max.z+.49f);
        var fake=GameObject.CreatePrimitive(PrimitiveType.Sphere);fake.transform.position=contact;fake.AddComponent<Rigidbody>();
        yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
        Assert.IsFalse(attempt.IsRevealed(edge));Object.Destroy(fake);yield return null;
        PlaceBall(contact);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
        Assert.IsTrue(attempt.IsRevealed(edge));Assert.IsTrue(wall.GetComponent<Renderer>().enabled);
        Assert.AreEqual(1,Enumerable.Range(0,BoardGrid.EdgeCount).Count(attempt.IsRevealed));
        Assert.IsTrue(Find<LevelManager>().TryCallForHelp());Assert.AreSame(attempt,features.Attempt);Assert.IsTrue(attempt.IsRevealed(edge));
        yield return FailAndRecover();Assert.AreSame(attempt,features.Attempt);Assert.IsTrue(wall.GetComponent<Renderer>().enabled);
        Assert.IsTrue(Find<LevelManager>().TrySetupLevel(0));Assert.AreNotSame(attempt,features.Attempt);Assert.IsFalse(wall.GetComponent<Renderer>().enabled);
    }
    [UnityTest] public IEnumerator LockedGoalTriggerRecoversWithinTheSameAttempt() {
        var data=SelectLevel(6);var features=Find<LevelFeaturesBehavior>();var attempt=features.Attempt;
        var ball=Find<MarbleBehaviour>();Set(ball,"shrinkDuration",.1f);
        PlaceBall(Find<HoleBehavior>().transform.position);
        yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
        Assert.AreEqual(BallState.Falling,ball.CurrentState);
        Assert.IsTrue(GameManagerBehavior.Instance.IsPaused);
        yield return new WaitForSeconds(.3f);
        Assert.AreEqual(BallState.Playing,ball.CurrentState);Assert.AreSame(attempt,features.Attempt);
        Assert.IsFalse(attempt.GoalUnlocked);Assert.IsFalse(Find<HoleBehavior>().IsCorrect);
        Assert.Less(Vector3.Distance(data.MarbleStartPosition,ball.transform.position),.001f);
    }
    [UnityTest] public IEnumerator RingFiltersNonMarbleAndResetsEarlyExit() {
        SelectLevel(6);var features=Find<LevelFeaturesBehavior>();var attempt=features.Attempt;var ring=features.Ring;
        var fake=GameObject.CreatePrimitive(PrimitiveType.Sphere);fake.transform.position=ring.transform.position;fake.AddComponent<Rigidbody>();
        yield return new WaitForSeconds(.15f);Assert.AreEqual(0,attempt.UnlockProgress);Object.Destroy(fake);yield return null;
        PlaceBall(ring.transform.position);yield return new WaitForSeconds(.3f);
        Assert.Greater(attempt.UnlockProgress,0);Assert.IsFalse(attempt.GoalUnlocked);
        PlaceBall(new Vector3(-10,0,-10));yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
        Assert.AreEqual(0,attempt.UnlockProgress);Assert.IsFalse(Find<HoleBehavior>().IsCorrect);
    }
    [UnityTest] public IEnumerator UnlockAndDarkPersistUntilNewAttemptAndLightingRestores() {
        var originalLight=Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(x=>x.type==LightType.Directional);
        bool wasEnabled=originalLight.enabled;
        SelectLevel(21);var features=Find<LevelFeaturesBehavior>();var attempt=features.Attempt;
        Assert.IsFalse(originalLight.enabled);Assert.IsTrue(features.Spotlight.enabled);Assert.IsFalse(Find<HoleBehavior>().IsCorrect);
        PlaceBall(features.Ring.transform.position);yield return new WaitForSeconds(1f);Assert.IsFalse(attempt.GoalUnlocked);
        GameManagerBehavior.Instance.SetPaused(true);float progress=attempt.UnlockProgress;yield return new WaitForSeconds(.15f);Assert.AreEqual(progress,attempt.UnlockProgress);
        GameManagerBehavior.Instance.SetPaused(false);yield return new WaitForSeconds(1.15f);
        Assert.IsTrue(attempt.GoalUnlocked);Assert.IsTrue(Find<HoleBehavior>().IsCorrect);
        Assert.IsTrue(Find<LevelManager>().TryCallForHelp());Assert.AreSame(attempt,features.Attempt);
        yield return FailAndRecover();Assert.AreSame(attempt,features.Attempt);Assert.IsTrue(attempt.GoalUnlocked);
        yield return null;Assert.AreEqual(Find<MarbleBehaviour>().transform.position.x,features.Spotlight.transform.position.x,.001f);
        Assert.IsTrue(Find<LevelManager>().TrySetupLevel(0));Assert.IsFalse(features.Attempt.GoalUnlocked);
        SelectLevel(1);Assert.AreEqual(wasEnabled,originalLight.enabled);Assert.IsFalse(features.Spotlight.enabled);Assert.IsFalse(features.Ring.gameObject.activeSelf);
    }
    [UnityTest] public IEnumerator UnlockAndRevealConfigureIndependently() {
        foreach(int number in new[]{27,45}) {
            var data=SelectLevel(number);var features=Find<LevelFeaturesBehavior>();
            Assert.IsTrue(features.Ring.gameObject.activeSelf);Assert.IsFalse(features.Attempt.GoalUnlocked);
            Assert.IsFalse(Find<HoleBehavior>().IsCorrect);Assert.AreEqual(94,data.Walls.Count(x=>x==WallState.RevealOnCollision));
            PlaceBall(features.Ring.transform.position);yield return new WaitForSeconds(2.15f);
            Assert.IsTrue(features.Attempt.GoalUnlocked);Assert.IsTrue(Find<HoleBehavior>().IsCorrect);
        }
    }
}
