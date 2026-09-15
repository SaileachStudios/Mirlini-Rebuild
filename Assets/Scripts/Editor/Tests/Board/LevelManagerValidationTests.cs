using System.Collections;
using NUnit.Framework;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.Marble;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class LevelManagerValidationTests : SandboxPlayTest
{
    [UnityTest]
    public IEnumerator InvalidIndexCannotMutateLevel() {
        var level=Find<LevelManager>();var ball=Find<MarbleBehaviour>();Vector3 before=ball.transform.position;
        LogAssert.Expect(LogType.Error,"Invalid level index or missing level list.");
        Assert.IsFalse(level.TrySetupLevel(999));Assert.AreEqual(0,level.CurrentLevelIndex);Assert.AreEqual(before,ball.transform.position);
        yield break;
    }
    [UnityTest]
    public IEnumerator MissingWallReferenceFailsBeforeChangingMarbleOrIndex() {
        var level=Find<LevelManager>();var ball=Find<MarbleBehaviour>();Vector3 before=ball.transform.position;
        var serialized=new SerializedObject(level);serialized.FindProperty("walls").GetArrayElementAtIndex(0).objectReferenceValue=null;serialized.ApplyModifiedPropertiesWithoutUndo();
        LogAssert.Expect(LogType.Error,"Wall references must be assigned and unique.");
        Assert.IsFalse(level.TrySetupLevel(1));Assert.AreEqual(0,level.CurrentLevelIndex);Assert.AreEqual(before,ball.transform.position);
        yield break;
    }
    [UnityTest]
    public IEnumerator MissingGoalFailsWithoutPartialSetup() {
        var level=Find<LevelManager>();var ball=Find<MarbleBehaviour>();Vector3 before=ball.transform.position;
        var serialized=new SerializedObject(level);serialized.FindProperty("hole").objectReferenceValue=null;serialized.ApplyModifiedPropertiesWithoutUndo();
        LogAssert.Expect(LogType.Error,"Marble and goal references must be distinct and assigned.");
        Assert.IsFalse(level.TrySetupLevel(1));Assert.AreEqual(before,ball.transform.position);
        yield break;
    }
}
