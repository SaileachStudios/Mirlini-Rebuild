using NUnit.Framework;
using SaileachStudios.Mirlini.InputSystem;
using UnityEngine;

public class InputProviderFactoryTests
{
    [Test]
    public void CreateInputProvider_ReturnsKeyboardInputProvider_WhenOnWindows() {
        var mockPlatformDetector = new MockPlatformDetector(Platform.windows);
        var mockInput = new MockInputWrapper();
        var factory = new InputProviderFactory(mockPlatformDetector, mockInput);

        var inputProvider = factory.Create();

        Assert.IsInstanceOf<KeyboardInputProvider>(inputProvider);
        Assert.AreEqual(100f, inputProvider.GetSpeedModifier());
    }

    [Test]
    public void CreateInputProvider_ReturnsTouchInputProvider_WhenOnMobileWithoutGyro() {
        var mockPlatformDetector = new MockPlatformDetector(Platform.mobile_with_touch);
        var mockInput = new MockInputWrapper();
        var factory = new InputProviderFactory(mockPlatformDetector, mockInput);

        var inputProvider = factory.Create();


        Assert.IsInstanceOf<TouchInputProvider>(inputProvider);
        Assert.AreEqual(0.2f, inputProvider.GetSpeedModifier());
    }

    [Test]
    public void CreateInputProvider_ReturnsGyroInputProvider_WhenOnMobileWithGyro() {
        var mockPlatformDetector = new MockPlatformDetector(Platform.mobile_with_gyro);
        var mockInput = new MockInputWrapper();
        var factory = new InputProviderFactory(mockPlatformDetector, mockInput);

        var inputProvider = factory.Create();


        Assert.IsInstanceOf<GyroInputProvider>(inputProvider);
        Assert.AreEqual(2f, inputProvider.GetSpeedModifier());
    }
}
