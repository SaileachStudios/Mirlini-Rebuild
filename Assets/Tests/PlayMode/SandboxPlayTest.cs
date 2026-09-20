using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Actual Unity lifecycle; no reflective Awake/Start/OnEnable calls.
public abstract class SandboxPlayTest
{
    [UnitySetUp]
    public IEnumerator OpenSandbox() {
        if (SaileachStudios.Mirlini.Core.GameManagerBehavior.Instance != null) {
            Object.Destroy(SaileachStudios.Mirlini.Core.GameManagerBehavior.Instance.gameObject);
            yield return null;
        }
        EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Sandbox.unity",new LoadSceneParameters(LoadSceneMode.Single));
        yield return null;
    }
    [UnityTearDown]
    public IEnumerator LeavePlayMode() {
        if (SaileachStudios.Mirlini.Core.GameManagerBehavior.Instance != null)
            Object.Destroy(SaileachStudios.Mirlini.Core.GameManagerBehavior.Instance.gameObject);
        yield return null;
    }
    protected static T Find<T>() where T:Object => Object.FindAnyObjectByType<T>();
    protected static void Set(Object target,string field,float value) {
        var serialized=new SerializedObject(target);serialized.FindProperty(field).floatValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
