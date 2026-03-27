using NUnit.Framework;
using SaileachStudios.Mirlini.Core;
using SaileachStudios.Mirlini.Marble;
using System.Reflection;
using UnityEngine;

public class MarbleBehaviourTests
{
    private static readonly BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly BindingFlags StaticBindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

    private GameObject gameManagerObject;
    private GameObject marbleObject;
    private MarbleBehaviour marbleBehaviour;

    [SetUp]
    public void SetUp() {
        gameManagerObject = new GameObject("GameManager");
        var gameManager = gameManagerObject.AddComponent<GameManagerBehavior>();
        SetGameManagerInstance(gameManager);

        marbleObject = new GameObject("Marble");
        marbleObject.AddComponent<Rigidbody>();
        marbleBehaviour = marbleObject.AddComponent<MarbleBehaviour>();

        InvokeUnityMessage(marbleBehaviour, "Awake");
        InvokeUnityMessage(marbleBehaviour, "Start");
    }

    [Test]
    public void MarbleBehaviour_StartsInIdleState() {
        Assert.AreEqual(BallState.Idle, marbleBehaviour.CurrentState);
    }

    [Test]
    public void StartPlaying_TransitionsMarbleToPlaying() {
        bool transitioned = marbleBehaviour.StartPlaying();

        Assert.AreEqual(true, transitioned);
        Assert.AreEqual(BallState.Playing, marbleBehaviour.CurrentState);
    }

    [Test]
    public void OnMarbleDropped_WhenPlaying_TransitionsMarbleToFalling() {
        marbleBehaviour.StartPlaying();

        marbleBehaviour.OnMarbleDropped(true, Vector3.one);

        Assert.AreEqual(BallState.Falling, marbleBehaviour.CurrentState);
    }

    [Test]
    public void OnMarbleDropped_WhenCorrectHole_CompletesLevelAfterShrink() {
        SetPrivateField(marbleBehaviour, "shrinkDuration", 0f);
        marbleBehaviour.StartPlaying();

        marbleBehaviour.OnMarbleDropped(true, Vector3.one);

        Assert.AreEqual(BallState.LevelComplete, marbleBehaviour.CurrentState);
    }

    [Test]
    public void OnMarbleDropped_WhenIncorrectHole_RespawnsAndReturnsToPlaying() {
        marbleObject.transform.position = new Vector3(2f, 0f, 3f);
        SetPrivateField(marbleBehaviour, "shrinkDuration", 0f);
        marbleBehaviour.StartPlaying();

        marbleObject.transform.position = new Vector3(10f, 0f, 12f);
        marbleBehaviour.OnMarbleDropped(false, Vector3.one);

        Assert.AreEqual(BallState.Playing, marbleBehaviour.CurrentState);
        Assert.AreEqual(new Vector3(2f, 0f, 3f), marbleObject.transform.position);
        Assert.AreEqual(Vector3.one, marbleObject.transform.localScale);
    }

    [Test]
    public void OnPlayerInput_WhenMarbleIsStuck_RespawnsBackToLastStartPosition() {
        marbleObject.transform.position = new Vector3(2f, 0f, 3f);
        SetPrivateField(marbleBehaviour, "shrinkDuration", 0f);
        SetPrivateField(marbleBehaviour, "stuckDetectionDuration", Time.fixedDeltaTime);
        SetPrivateField(marbleBehaviour, "stuckMovementThreshold", 0.5f);
        marbleBehaviour.StartPlaying();

        marbleObject.transform.position = new Vector3(9f, 0f, 9f);
        SetPrivateField(marbleBehaviour, "lastProgressPosition", marbleObject.transform.position);

        InvokePlayerInput(false, Vector2.right);

        Assert.AreEqual(BallState.Playing, marbleBehaviour.CurrentState);
        Assert.AreEqual(new Vector3(2f, 0f, 3f), marbleObject.transform.position);
        Assert.AreEqual(Vector3.one, marbleObject.transform.localScale);
    }

    [Test]
    public void OnPlayerInput_WithoutMovementInput_DoesNotTriggerStuckRespawn() {
        marbleObject.transform.position = new Vector3(2f, 0f, 3f);
        SetPrivateField(marbleBehaviour, "shrinkDuration", 0f);
        SetPrivateField(marbleBehaviour, "stuckDetectionDuration", Time.fixedDeltaTime);
        SetPrivateField(marbleBehaviour, "stuckMovementThreshold", 0.5f);
        marbleBehaviour.StartPlaying();

        marbleObject.transform.position = new Vector3(9f, 0f, 9f);

        InvokePlayerInput(false, Vector2.zero);
        InvokePlayerInput(false, Vector2.zero);
        InvokePlayerInput(false, Vector2.zero);

        Assert.AreEqual(BallState.Playing, marbleBehaviour.CurrentState);
        Assert.AreEqual(new Vector3(9f, 0f, 9f), marbleObject.transform.position);
    }

    [TearDown]
    public void TearDown() {
        if (marbleObject != null) {
            Object.DestroyImmediate(marbleObject);
        }

        if (gameManagerObject != null) {
            Object.DestroyImmediate(gameManagerObject);
        }

        SetGameManagerInstance(null);
    }

    private static void InvokeUnityMessage(object target, string methodName) {
        MethodInfo method = target.GetType().GetMethod(methodName, InstanceBindingFlags);
        method?.Invoke(target, null);
    }

    private static void SetGameManagerInstance(GameManagerBehavior instance) {
        FieldInfo backingField = typeof(GameManagerBehavior).GetField("<Instance>k__BackingField", StaticBindingFlags);
        backingField?.SetValue(null, instance);
    }

    private static void SetPrivateField(object target, string fieldName, object value) {
        FieldInfo field = target.GetType().GetField(fieldName, InstanceBindingFlags);
        field?.SetValue(target, value);
    }

    private void InvokePlayerInput(bool isPaused, Vector2 input) {
        MethodInfo method = marbleBehaviour.GetType().GetMethod("OnPlayerInput", InstanceBindingFlags);
        method?.Invoke(marbleBehaviour, new object[] { isPaused, input });
    }
}
