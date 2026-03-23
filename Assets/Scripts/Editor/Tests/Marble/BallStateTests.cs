using NUnit.Framework;
using SaileachStudios.Mirlini.Marble;
using Unity.VisualScripting;
using UnityEngine;

public class BallStateTests
{
    // ===== STATE ENUM TESTS =====

    [Test]
    public void BallState_HasIdleState()
    {
        BallState state = BallState.Idle;
        Assert.AreEqual(BallState.Idle, state);
    }

    [Test]
    public void BallState_HasPlayingState()
    {
        BallState state = BallState.Playing;
        Assert.AreEqual(BallState.Playing, state);
    }

    [Test]
    public void BallState_HasFallingState()
    {
        BallState state = BallState.Falling;
        Assert.AreEqual(BallState.Falling, state);
    }

    [Test]
    public void BallState_HasRespawningState()
    {
        BallState state = BallState.Respawning;
        Assert.AreEqual(BallState.Respawning, state);
    }

    [Test]
    public void BallState_HasStuckState()
    {
        BallState state = BallState.Stuck;
        Assert.AreEqual(BallState.Stuck, state);
    }

    [Test]
    public void BallState_HasLevelCompleteState()
    {
        BallState state = BallState.LevelComplete;
        Assert.AreEqual(BallState.LevelComplete, state);
    }

    // ===== STATE MACHINE TESTS =====

    [Test]
    public void BallStateMachine_StartsInIdleState()
    {
        var stateMachine = new BallStateMachine();
        Assert.AreEqual(BallState.Idle, stateMachine.CurrentState);
    }

    [Test]
    public void BallStateMachine_CanTransitionFromIdleToPlaying()
    {
        var stateMachine = new BallStateMachine();
        bool transitioned = stateMachine.TransitionTo(BallState.Playing);

        Assert.AreEqual(true, transitioned, "Should allow Idle ? Playing");
        Assert.AreEqual(BallState.Playing, stateMachine.CurrentState);
    }

    [Test]
    public void BallStateMachine_CanTransitionFromPlayingToFalling()
    {
        var stateMachine = new BallStateMachine();
        stateMachine.TransitionTo(BallState.Playing);
        bool transitioned = stateMachine.TransitionTo(BallState.Falling);

        Assert.AreEqual(true, transitioned, "Should allow Playing ? Falling");
        Assert.AreEqual(BallState.Falling, stateMachine.CurrentState);
    }

    [Test]
    public void BallStateMachine_CanTransitionFromFallingToRespawning()
    {
        var stateMachine = new BallStateMachine();
        stateMachine.TransitionTo(BallState.Playing);
        stateMachine.TransitionTo(BallState.Falling);
        bool transitioned = stateMachine.TransitionTo(BallState.Respawning);

        Assert.AreEqual(true, transitioned, "Should allow Falling ? Respawning");
        Assert.AreEqual(BallState.Respawning, stateMachine.CurrentState);
    }

    [Test]
    public void BallStateMachine_CanTransitionFromRespawningToPlaying()
    {
        var stateMachine = new BallStateMachine();
        stateMachine.TransitionTo(BallState.Playing);
        stateMachine.TransitionTo(BallState.Falling);
        stateMachine.TransitionTo(BallState.Respawning);
        bool transitioned = stateMachine.TransitionTo(BallState.Playing);

        Assert.AreEqual(true, transitioned, "Should allow Respawning ? Playing");
        Assert.AreEqual(BallState.Playing, stateMachine.CurrentState);
    }

    [Test]
    public void BallStateMachine_CanTransitionFromPlayingToStuck()
    {
        var stateMachine = new BallStateMachine();
        stateMachine.TransitionTo(BallState.Playing);
        bool transitioned = stateMachine.TransitionTo(BallState.Stuck);

        Assert.AreEqual(true, transitioned, "Should allow Playing ? Stuck");
        Assert.AreEqual(BallState.Stuck, stateMachine.CurrentState);
    }

    [Test]
    public void BallStateMachine_CanTransitionFromStuckToRespawning()
    {
        var stateMachine = new BallStateMachine();
        stateMachine.TransitionTo(BallState.Playing);
        stateMachine.TransitionTo(BallState.Stuck);
        bool transitioned = stateMachine.TransitionTo(BallState.Respawning);

        Assert.AreEqual(true, transitioned, "Should allow Stuck ? Respawning");
        Assert.AreEqual(BallState.Respawning, stateMachine.CurrentState);
    }

    [Test]
    public void BallStateMachine_CanTransitionFromFallingToLevelComplete()
    {
        var stateMachine = new BallStateMachine();
        stateMachine.TransitionTo(BallState.Playing);
        stateMachine.TransitionTo(BallState.Falling);
        bool transitioned = stateMachine.TransitionTo(BallState.LevelComplete);

        Assert.AreEqual(true, transitioned, "Should allow Falling ? LevelComplete");
        Assert.AreEqual(BallState.LevelComplete, stateMachine.CurrentState);
    }

    [Test]
    public void BallStateMachine_CannotTransitionFromIdleToFalling()
    {
        var stateMachine = new BallStateMachine();
        bool transitioned = stateMachine.TransitionTo(BallState.Falling);

        Assert.AreEqual(false, transitioned, "Should NOT allow Idle ? Falling");
        Assert.AreEqual(BallState.Idle, stateMachine.CurrentState, "Should stay in Idle");
    }

    [Test]
    public void BallStateMachine_CannotTransitionFromLevelCompleteToPlaying()
    {
        var stateMachine = new BallStateMachine();
        stateMachine.TransitionTo(BallState.Playing);
        stateMachine.TransitionTo(BallState.Falling);
        stateMachine.TransitionTo(BallState.LevelComplete);
        bool transitioned = stateMachine.TransitionTo(BallState.Playing);

        Assert.AreEqual(false, transitioned, "Should NOT allow LevelComplete ? Playing");
        Assert.AreEqual(BallState.LevelComplete, stateMachine.CurrentState);
    }

    // ===== STATE CHANGE EVENT TESTS =====

    [Test]
    public void BallStateMachine_FiresEventOnStateChange()
    {
        var stateMachine = new BallStateMachine();
        BallState? capturedOldState = null;
        BallState? capturedNewState = null;

        stateMachine.OnStateChanged += (oldState, newState) => {
            capturedOldState = oldState;
            capturedNewState = newState;
        };

        stateMachine.TransitionTo(BallState.Playing);

        Assert.AreEqual(BallState.Idle, capturedOldState);
        Assert.AreEqual(BallState.Playing, capturedNewState);
    }

    [Test]
    public void BallStateMachine_DoesNotFireEventOnInvalidTransition()
    {
        var stateMachine = new BallStateMachine();
        bool eventFired = false;

        stateMachine.OnStateChanged += (oldState, newState) => {
            eventFired = true;
        };

        stateMachine.TransitionTo(BallState.Falling); // Invalid from Idle

        Assert.AreEqual(false, eventFired, "Event should not fire on invalid transition");
    }
}