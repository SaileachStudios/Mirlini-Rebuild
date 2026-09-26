using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.Core;
using SaileachStudios.Mirlini.Marble;
using SaileachStudios.Mirlini.InputSystem;
public class GameplayFeelTests {
    [Test] public void ProductionProfileAndFeedbackMaterialAreValid() {
        var feel=GameplayFeel.Load();Assert.IsTrue(feel.IsValid);Assert.IsNotNull(feel.WorldFeedbackMaterial);
        Assert.IsNotNull(feel.WorldFeedbackMaterial.shader);
        var marble=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Marble.mat");
        Assert.IsTrue(marble.IsKeywordEnabled("_EMISSION"),"Marble visibility assistance must survive material serialization.");
    }
    [Test] public void SharedInputKeepsProportionalAnalogAndCapsDiagonal() {
        var c=new MarbleController(GameplayFeel.Load());
        Vector3 low=c.Step(Vector3.zero,new Vector2(.25f,0),.02f),full=c.Step(Vector3.zero,Vector2.right,.02f);
        Assert.AreEqual(full.x*.25f,low.x,.0001f);
        Vector3 v=Vector3.zero;for(int i=0;i<100;i++) v=c.Step(v,Vector2.one,.02f);
        Assert.LessOrEqual(v.magnitude,GameplayFeel.Load().Speed+.001f);
    }
    [Test] public void ReleaseStopsWithoutCrossingZeroAndPreservesVerticalVelocity() {
        var c=new MarbleController(GameplayFeel.Load());Vector3 v=new Vector3(9.5f,-2,0);
        for(int i=0;i<100;i++) {v=c.Step(v,Vector2.zero,.02f);Assert.GreaterOrEqual(v.x,0);Assert.AreEqual(-2,v.y);}
        Assert.AreEqual(0,v.x);
    }
    [Test] public void LargeStepDoesNotOvershootTargetAndReversalIsBounded() {
        var feel=GameplayFeel.Load();var c=new MarbleController(feel);
        Assert.AreEqual(feel.Speed,c.Step(Vector3.zero,Vector2.right,1).x,.001f);
        var before=new Vector3(feel.Speed,0,0);var after=c.Step(before,Vector2.left,.02f);
        Assert.Less(after.x,before.x);Assert.LessOrEqual((after-before).magnitude,feel.MaxAcceleration*.02f+.001f);
    }
    [Test] public void ProfileRejectsNonFiniteOrInvalidMovement() {
        var f=ScriptableObject.CreateInstance<GameplayFeel>();
        try {f.Speed=float.NaN;Assert.IsFalse(f.IsValid);f.Speed=1;f.ReleaseResponse=0;Assert.IsFalse(f.IsValid);}
        finally {Object.DestroyImmediate(f);}
    }
}
