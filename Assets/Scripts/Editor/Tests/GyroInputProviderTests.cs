using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;

public class GyroInputProviderTests
{
    [Test]
    public void GryoInput_Disabled() {
        var mockInput = new MockInputWrapper { GyroEnabled = false };
        var gyroInput = new GyroInputProvider(mockInput);

        Assert.AreEqual(false, gyroInput.GyroEnabled);
    }

    [Test]
    public void GryoInput_Enabled() {
        var mockInput = new MockInputWrapper { GyroEnabled = true };
        var gyroInput = new GyroInputProvider(mockInput);

        Assert.AreEqual(true, gyroInput.GyroEnabled);
    }

    [Test]
    public void GetInput_ReturnsCorrectValues_FromMockGravity() {
        var mockInput = new MockInputWrapper { GyroGravity = new Vector3(0f, 0.25f, 0f) };  // Simulating device tilt
        var gyroInput = new GyroInputProvider(mockInput);

        Vector2 result = gyroInput.GetInput();

        Assert.AreEqual(0.5f, result.x, 0.01f, $"Expected X to be 0.5 but was {result.x}");
        Assert.AreEqual(0f, result.y, 0.01f, $"Expected Y to be 0.0 but was {result.y}");
    }

    [Test]
    public void GetInput_UsesNegativeXGravityForYAxis() {
        var mockInput = new MockInputWrapper { GyroGravity = new Vector3(-0.2f, 0f, 0f) };  // Simulating device tilt
        var gyroInput = new GyroInputProvider(mockInput);

        Vector2 result = gyroInput.GetInput();

        Assert.AreEqual(0f, result.x, 0.01f);
        Assert.AreEqual(0.4f, result.y, 0.01f, $"Expected Y to be 0.4 but got {result.y}");
    }

    [Test]
    public void GetInput_ReturnsCombinedInput_FromGravity() {
        var mockInput = new MockInputWrapper { GyroGravity = new Vector3(0.3f, 0.4f, 0f) };  // Simulating device tilt
        var gyroInput = new GyroInputProvider(mockInput);

        Vector2 result = gyroInput.GetInput();

        Assert.AreEqual(0.8f, result.x, 0.01f, $"Expected X = 0.8, got {result.x}");
        Assert.AreEqual(-0.6f, result.y, 0.01f, $"Expected Y = -0.6, got {result.y}");
    }

    [Test]
    public void GetInput_RespectsGyroSpeedModifier() {
        var mockInput = new MockInputWrapper { GyroGravity = new Vector3(0.1f, 0.2f, 0f) };  // Simulating device tilt
        var gyroInput = new GyroInputProvider(mockInput);

        Vector2 result = gyroInput.GetInput();

        Assert.AreEqual(0.4f, result.x, 0.01f, $"Expected X = 0.4, got {result.x}");
        Assert.AreEqual(-0.2f, result.y, 0.01f, $"Expected Y = -0.2, got {result.y}");
    }

    [Test]
    public void GetInput_ReturnsCorrectVector_WithNegativeGyroValues() {
        var mockGyro = new MockInputWrapper { GyroGravity = new Vector2(-0.3f, 0.5f) };  // Simulating device tilt
        var gyroInput = new GyroInputProvider(mockGyro);

        Vector2 result = gyroInput.GetInput();

        Assert.AreEqual(1.0f, result.x, 0.001f, $"Expected X: 1.0f, but got {result.x}");
        Assert.AreEqual(0.6f, result.y, 0.001f, $"Expected Y: 0.6f, but got {result.y}");
    }
}
