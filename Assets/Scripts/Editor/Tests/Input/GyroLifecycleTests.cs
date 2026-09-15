using NUnit.Framework;
using SaileachStudios.Mirlini.InputSystem;
using UnityEngine;

public class GyroLifecycleTests
{
    private class CountingWrapper : IInputWrapper
    {
        public int Writes; private bool enabled;
        public bool GyroEnabled { get=>enabled; set { enabled=value;Writes++; } }
        public Vector3 GyroGravity { get; set; }
        public Vector2 screenSize=>Vector2.one;
        public int TouchCount=>0;
        public Vector2 GetTouchPosition(int i)=>Vector2.zero;
        public float GetAxisValue(string name)=>0;
    }
    [Test]
    public void SensorIsOwnedByActivationNotReadsOrConstruction() {
        var wrapper=new CountingWrapper();var provider=new GyroInputProvider(wrapper,2);
        provider.GetInput();Assert.AreEqual(0,wrapper.Writes);
        provider.Activate();provider.Activate();provider.GetInput();
        Assert.AreEqual(1,wrapper.Writes);Assert.IsTrue(wrapper.GyroEnabled);
        provider.Deactivate();provider.Deactivate();Assert.AreEqual(2,wrapper.Writes);Assert.IsFalse(wrapper.GyroEnabled);
        provider.Activate();Assert.AreEqual(3,wrapper.Writes);Assert.IsTrue(wrapper.GyroEnabled);
    }
}
