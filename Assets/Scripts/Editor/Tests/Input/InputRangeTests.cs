using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;

public class InputRangeTests
{
    [Test]
    public void SubUnitAnalogStrengthSurvives() {
        Assert.AreEqual(new Vector2(.2f,.3f),InputRange.Clamp(new Vector2(.2f,.3f)));
        Assert.AreEqual(Vector2.zero,InputRange.Clamp(Vector2.zero));
    }
    [Test]
    public void OnlyOversizedVectorsAreClamped() {
        var result=InputRange.Clamp(new Vector2(3,4));
        Assert.AreEqual(.6f,result.x,.00001f);Assert.AreEqual(.8f,result.y,.00001f);
    }
    [Test]
    public void TouchAndGyroModifiersReachConsumer() {
        var wrapper=new MockInputWrapper { TouchPosition=new Vector2(600,600),GyroGravity=new Vector3(0,.1f,0) };
        Assert.AreEqual(.1f,InputRange.Clamp(new TouchInputProvider(wrapper,.2f).GetInput()).x,.00001f);
        Assert.AreEqual(.2f,InputRange.Clamp(new GyroInputProvider(wrapper,2).GetInput()).x,.00001f);
    }
    [Test]
    public void KeyboardStillHasFullStrengthAndClampedDiagonals() {
        var wrapper=new MockInputWrapper();wrapper.AxisValues["Vertical"]=1;wrapper.AxisValues["Horizontal"]=0;
        var keyboard=new KeyboardInputProvider(wrapper,100);
        Assert.AreEqual(Vector2.right,InputRange.Clamp(keyboard.GetInput()));
        wrapper.AxisValues["Horizontal"]=1;
        Assert.AreEqual(1,InputRange.Clamp(keyboard.GetInput()).magnitude,.00001f);
    }
    [Test]
    public void NonFiniteInputCannotReachPhysics() { Assert.AreEqual(Vector2.zero,InputRange.Clamp(new Vector2(float.NaN,0))); }
}
