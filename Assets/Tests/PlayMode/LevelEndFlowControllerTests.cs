using System.Collections;
using NUnit.Framework;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.Core;
using UnityEngine.TestTools;

public class LevelEndFlowControllerTests : SandboxPlayTest
{
    [UnityTest]
    public IEnumerator CompletionLoadsNextConfiguredLevelOnce() {
        var flow=Find<LevelEndFlowController>();var level=Find<LevelManager>();Set(flow,"temporaryCompletionDelay",0);
        GameManagerBehavior.Instance.Events.LevelCompleted();GameManagerBehavior.Instance.Events.LevelCompleted();
        yield return null;yield return null;
        Assert.AreEqual(1,level.CurrentLevelIndex);
    }
    [UnityTest]
    public IEnumerator DisabledFlowDoesNotReceiveCompletion() {
        Find<LevelEndFlowController>().enabled=false;
        GameManagerBehavior.Instance.Events.LevelCompleted();yield return null;
        Assert.AreEqual(0,Find<LevelManager>().CurrentLevelIndex);
    }
    [UnityTest]
    public IEnumerator ReenabledFlowReceivesOnlyOneSubscription() {
        var flow=Find<LevelEndFlowController>();Set(flow,"temporaryCompletionDelay",0);
        flow.enabled=false;flow.enabled=true;
        GameManagerBehavior.Instance.Events.LevelCompleted();yield return null;
        Assert.AreEqual(1,Find<LevelManager>().CurrentLevelIndex);
    }
}
