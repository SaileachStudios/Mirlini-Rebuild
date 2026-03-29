using System.Collections;
using System.Reflection;
using NUnit.Framework;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.Core;
using SaileachStudios.Mirlini.Marble;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

public class LevelEndFlowControllerTests
{
    private static readonly BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly BindingFlags StaticBindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

    private GameObject gameManagerObject;
    private GameObject managerObject;
    private GameObject marbleObject;
    private GameObject holeObject;
    private GameObject wallObject;
    private LevelManager levelManager;
    private LevelEndFlowController flowController;

    [UnityTest]
    public IEnumerator LevelCompleted_WhenFlowControllerEnabled_LoadsNextLevel() {
        SetupScene(CreateLevelData("Level 1", Vector3.zero, Vector3.zero), CreateLevelData("Level 2", new Vector3(10f, 0f, 0f), new Vector3(5f, 0f, 0f)));
        Vector3 initialMarblePosition = marbleObject.transform.position;

        GameManagerBehavior.Instance.Events.LevelCompleted();
        yield return null;

        Assert.AreNotEqual(initialMarblePosition, marbleObject.transform.position);
        Assert.AreEqual(new Vector3(10f, 0f, 0f), marbleObject.transform.position);
    }

    [UnityTest]
    public IEnumerator LevelCompleted_WhenFlowControllerDisabled_DoesNotLoadNextLevel() {
        SetupScene(CreateLevelData("Level 1", Vector3.zero, Vector3.zero), CreateLevelData("Level 2", new Vector3(10f, 0f, 0f), new Vector3(5f, 0f, 0f)));
        Vector3 initialMarblePosition = marbleObject.transform.position;

        InvokeUnityMessage(flowController, "OnDisable");
        GameManagerBehavior.Instance.Events.LevelCompleted();
        yield return null;

        Assert.AreEqual(initialMarblePosition, marbleObject.transform.position);
    }

    [UnityTest]
    public IEnumerator LevelCompleted_WhenRaisedTwiceQuickly_OnlyProcessesOneTransition() {
        SetupScene(
            CreateLevelData("Level 1", Vector3.zero, Vector3.zero),
            CreateLevelData("Level 2", new Vector3(10f, 0f, 0f), new Vector3(5f, 0f, 0f)),
            CreateLevelData("Level 3", new Vector3(20f, 0f, 0f), new Vector3(15f, 0f, 0f)));

        GameManagerBehavior.Instance.Events.LevelCompleted();
        GameManagerBehavior.Instance.Events.LevelCompleted();
        yield return null;

        Assert.AreEqual(new Vector3(10f, 0f, 0f), marbleObject.transform.position);
    }

    [TearDown]
    public void TearDown() {
        if (managerObject != null) {
            Object.DestroyImmediate(managerObject);
        }

        if (marbleObject != null) {
            Object.DestroyImmediate(marbleObject);
        }

        if (holeObject != null) {
            Object.DestroyImmediate(holeObject);
        }

        if (wallObject != null) {
            Object.DestroyImmediate(wallObject);
        }

        if (gameManagerObject != null) {
            Object.DestroyImmediate(gameManagerObject);
        }

        SetGameManagerInstance(null);
    }

    private void SetupScene(params LevelData[] levels) {
        gameManagerObject = new GameObject("GameManager");
        var gameManager = gameManagerObject.AddComponent<GameManagerBehavior>();
        SetGameManagerInstance(gameManager);

        marbleObject = new GameObject("Marble");
        marbleObject.AddComponent<Rigidbody>();
        var marbleBehaviour = marbleObject.AddComponent<MarbleBehaviour>();
        InvokeUnityMessage(marbleBehaviour, "Awake");
        InvokeUnityMessage(marbleBehaviour, "Start");

        holeObject = new GameObject("Hole");
        var holeBehavior = holeObject.AddComponent<HoleBehavior>();
        SetPrivateField(holeBehavior, "correctIndicator", new GameObject("CorrectIndicator"));
        SetPrivateField(holeBehavior, "incorrectIndicator", new GameObject("IncorrectIndicator"));
        InvokeUnityMessage(holeBehavior, "OnEnable");

        wallObject = new GameObject("Wall");

        managerObject = new GameObject("LevelManager");
        levelManager = managerObject.AddComponent<LevelManager>();
        var flowControllers = managerObject.GetComponents<LevelEndFlowController>();
        if (flowControllers.Length == 0) {
            flowController = managerObject.AddComponent<LevelEndFlowController>();
        }
        else {
            flowController = flowControllers[0];
            foreach (var duplicateFlowController in flowControllers.Skip(1)) {
                Object.DestroyImmediate(duplicateFlowController);
            }
        }

        SetPrivateField(levelManager, "marble", marbleObject);
        SetPrivateField(levelManager, "hole", holeObject);
        SetPrivateField(levelManager, "walls", new[] { wallObject });
        SetPrivateField(levelManager, "levels", levels);

        SetPrivateField(flowController, "levelManager", levelManager);
        SetPrivateField(flowController, "temporaryCompletionDelay", 0f);

        InvokeUnityMessage(flowController, "OnEnable");
        levelManager.SetupLevel(0);
    }

    private static LevelData CreateLevelData(string levelName, Vector3 marblePosition, Vector3 holePosition) {
        var levelData = ScriptableObject.CreateInstance<LevelData>();
        levelData.LevelName = levelName;
        levelData.MarbleStartPosition = marblePosition;
        levelData.HolePosition = holePosition;
        levelData.wallInfo = new[] { false };
        return levelData;
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
}
