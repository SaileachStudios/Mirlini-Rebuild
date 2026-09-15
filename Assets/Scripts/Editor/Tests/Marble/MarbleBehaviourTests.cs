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
        var ball=Find<MarbleBehaviour>();Set(ball,"shrinkDuration",0);
        Assert.IsTrue(ball.TryEnterHole(true,new Vector3(1,0,1)));
        Assert.IsFalse(ball.TryEnterHole(false,new Vector3(9,0,9)));
        Assert.AreEqual(BallState.Falling,ball.CurrentState);
        Assert.IsTrue(GameManagerBehavior.Instance.IsPaused);
        yield return null;yield return null;
        Assert.AreEqual(BallState.LevelComplete,ball.CurrentState);
        Assert.AreEqual(.5f,ball.CollisionRadius,.00001f);
        Assert.AreEqual(new Vector3(1,0,1),ball.transform.position);
    }
    [UnityTest]
    public IEnumerator FailureGrowthPreservesExternalPauseAndAttempt() {
        var ball=Find<MarbleBehaviour>();Set(ball,"shrinkDuration",0);
        Vector3 start=ball.transform.position;
        Assert.IsTrue(ball.TryEnterHole(false,Vector3.zero));
        GameManagerBehavior.Instance.SetPaused(true);
        yield return null;yield return null;
        Assert.AreEqual(BallState.Playing,ball.CurrentState);
        Assert.AreEqual(start,ball.transform.position);
        Assert.IsTrue(GameManagerBehavior.Instance.IsPaused);
        Assert.IsFalse(Find<LevelManager>().TryCallForHelp());
        GameManagerBehavior.Instance.SetPaused(false);
        Assert.IsFalse(GameManagerBehavior.Instance.IsPaused);
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
