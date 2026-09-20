using System.Collections;
using NUnit.Framework;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.Core;
using SaileachStudios.Mirlini.Marble;
using UnityEngine;
using UnityEngine.TestTools;

public class MarbleBehaviourTests : SandboxPlayTest
{
    [UnityTest]
    public IEnumerator RealSceneStartupExplicitlyStartsMarble() {
        Assert.AreEqual(0,Find<LevelManager>().CurrentLevelIndex);
        Assert.AreEqual(BallState.Playing,Find<MarbleBehaviour>().CurrentState);
        Assert.IsFalse(GameManagerBehavior.Instance.IsPaused);
        Assert.IsFalse(Find<MarbleBehaviour>().GetComponent<Rigidbody>().isKinematic);
        yield break;
    }
    [UnityTest]
    public IEnumerator DuplicateHoleEntryCannotOverwriteAcceptedOutcome() {
        Find<LevelEndFlowController>().enabled=false;
        var ball=Find<MarbleBehaviour>();Set(ball,"shrinkDuration",.05f);
        Assert.IsTrue(ball.TryEnterHole(true,new Vector3(1,0,1)));
        Assert.IsFalse(ball.TryEnterHole(false,new Vector3(9,0,9)));
        Assert.AreEqual(BallState.Falling,ball.CurrentState);
        Assert.IsTrue(GameManagerBehavior.Instance.IsPaused);
        float deadline = Time.realtimeSinceStartup + 3f;
        while (ball.CurrentState == BallState.Falling && Time.realtimeSinceStartup < deadline) {
            Assert.IsTrue(GameManagerBehavior.Instance.IsPaused);
            Assert.IsFalse(ball.TryEnterHole(false, new Vector3(9,0,9)));
            yield return null;
        }
        Assert.AreEqual(BallState.LevelComplete,ball.CurrentState);
        Assert.AreEqual(Vector3.zero, ball.transform.localScale);
        Assert.IsTrue(ball.GetComponent<Rigidbody>().isKinematic);
        Assert.AreEqual(.5f,ball.CollisionRadius,.00001f);
        Assert.AreEqual(new Vector3(1,0,1),ball.transform.position);
    }
    [UnityTest]
    public IEnumerator FailureGrowthPreservesExternalPauseAndAttempt() {
        yield return VerifyFailureRecovery(0f);
    }
    [UnityTest]
    public IEnumerator AnimatedFailureRemainsPausedUntilFullyPlayable() {
        yield return VerifyFailureRecovery(.05f);
    }
    private IEnumerator VerifyFailureRecovery(float duration) {
        var level = Find<LevelManager>();
        var ball = Find<MarbleBehaviour>();
        var manager = GameManagerBehavior.Instance;
        var body = ball.GetComponent<Rigidbody>();
        var serialized = new UnityEditor.SerializedObject(level);
        var data = (LevelData)serialized.FindProperty("levels").GetArrayElementAtIndex(level.CurrentLevelIndex).objectReferenceValue;
        Vector3 expectedStart = level.transform.position + data.MarbleStartPosition;
        Vector3 fullScale = ball.transform.localScale;
        int attemptLevel = level.CurrentLevelIndex;
        Set(ball, "shrinkDuration", duration);
        yield return new WaitForFixedUpdate();
        // Resting contacts can reconstruct positions a few millionths away from the authored start.
        // This is 1/50,000 of the marble radius, not permission for meaningful displacement.
        const float positionTolerance = .00001f;
        Assert.LessOrEqual(Vector3.Distance(expectedStart, ball.transform.position), positionTolerance);
        Assert.IsTrue(ball.TryEnterHole(false, Vector3.zero));
        manager.SetPaused(true);
        bool sawGrowth = false;
        float deadline = Time.realtimeSinceStartup + 3f;
        while (ball.CurrentState != BallState.Playing && Time.realtimeSinceStartup < deadline) {
            sawGrowth |= ball.CurrentState == BallState.Respawning;
            Assert.IsTrue(manager.IsPaused);
            Assert.IsTrue(body.isKinematic);
            Assert.IsFalse(level.TryCallForHelp());
            yield return null;
        }
        Assert.AreEqual(BallState.Playing, ball.CurrentState, "Recovery must finish within the timeout.");
        if (duration > 0f) Assert.IsTrue(sawGrowth, "Exercise the actual growth phase.");
        Assert.LessOrEqual(Vector3.Distance(expectedStart, ball.transform.position), positionTolerance);
        Assert.AreEqual(fullScale, ball.transform.localScale);
        Assert.IsFalse(body.isKinematic);
        Assert.AreEqual(attemptLevel, level.CurrentLevelIndex);
        Assert.IsTrue(manager.IsPaused);
        Assert.IsFalse(level.TryCallForHelp());
        manager.SetPaused(false);
        Assert.IsFalse(manager.IsPaused);
    }
    [UnityTest]
    public IEnumerator HelpIsGatedAndDoesNotRestartAttempt() {
        var level=Find<LevelManager>();var ball=Find<MarbleBehaviour>();var rb=ball.GetComponent<Rigidbody>();
        var manager=GameManagerBehavior.Instance;
        manager.SetPaused(true);Assert.IsFalse(level.TryCallForHelp());manager.SetPaused(false);
        rb.position=new Vector3(BoardGrid.HalfExtent-.1f,0,0);ball.transform.position=rb.position;
        rb.linearVelocity=Vector3.right;rb.angularVelocity=Vector3.up;
        Assert.IsTrue(level.TryCallForHelp());
        Assert.LessOrEqual(ball.transform.position.x,BoardGrid.HalfExtent-ball.CollisionRadius-.049f);
        Assert.AreEqual(Vector3.zero,rb.linearVelocity);Assert.AreEqual(Vector3.zero,rb.angularVelocity);
        Assert.AreEqual(0,level.CurrentLevelIndex);Assert.AreEqual(BallState.Playing,ball.CurrentState);
        Set(ball,"shrinkDuration",.1f);ball.TryEnterHole(false,Vector3.zero);
        Assert.IsFalse(level.TryCallForHelp());
        yield return null;
    }
    [UnityTest]
    public IEnumerator SustainedBlockedInputNeverRespawnsAutomatically() {
        var ball=Find<MarbleBehaviour>();var owner=GameManagerBehavior.Instance;
        Vector3 location=new Vector3(0,0,0);ball.transform.position=location;
        for (int i=0;i<300;i++) owner.Events.InputUpdated(false,Vector2.right);
        Assert.AreEqual(BallState.Playing,ball.CurrentState);
        Assert.AreEqual(location,ball.transform.position);
        yield break;
    }
    [UnityTest]
    public IEnumerator DisabledMarbleAndTiltDetachFromPersistentEvents() {
        var ball=Find<MarbleBehaviour>();var tilt=Find<BoardTiltBehavior>();var events=GameManagerBehavior.Instance.Events;
        ball.enabled=false;tilt.enabled=false;Quaternion rotation=tilt.transform.rotation;
        events.BallDropped(true,Vector3.zero);events.InputUpdated(false,Vector2.one);
        Assert.AreEqual(BallState.Idle,ball.CurrentState);Assert.AreEqual(rotation,tilt.transform.rotation);
        yield break;
    }
}
