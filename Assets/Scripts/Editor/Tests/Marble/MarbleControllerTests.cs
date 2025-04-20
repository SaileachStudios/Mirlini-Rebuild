using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;
using SaileachStudios.Mirlini.Marble;

public class MarbleControllerTests
{
    [Test]
    public void CalculateMovementForce_WithRightwardInput_ReturnsExpectedForce() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(1f, 0f), MockSpeed = 2f };
        var controller = new MarbleController(mockInput, mockInput.MockSpeed);

        var result = controller.CalculateMovementForce();
        Assert.AreEqual(2f, result.x, "Expected X=2f recieved " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f recieved " + result.y);
        Assert.AreEqual(0f, result.z, "Expected Z=0f recieved " + result.z);
        Assert.AreEqual(true, controller.IsMoving);
    }

    [Test]
    public void CalculateMovementForce_WithLeftwardInput_ReturnsExpectedForce() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(-1f, 0f), MockSpeed = 3f };
        var controller = new MarbleController(mockInput, mockInput.MockSpeed);

        var result = controller.CalculateMovementForce();
        Assert.AreEqual(-3f, result.x, "Expected X=-3f recieved " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f recieved " + result.y);
        Assert.AreEqual(0f, result.z, "Expected Z=0f recieved " + result.z);
        Assert.AreEqual(true, controller.IsMoving);
    }

    [Test]
    public void CalculateMovementForce_WithUpwardInput_ReturnsExpectedForce() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(0f, 1f), MockSpeed = 2f };
        var controller = new MarbleController(mockInput, mockInput.MockSpeed);

        var result = controller.CalculateMovementForce();
        Assert.AreEqual(0f, result.x, "Expected X=0f recieved " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f recieved " + result.y);
        Assert.AreEqual(2f, result.z, "Expected Z=2f recieved " + result.z);
        Assert.AreEqual(true, controller.IsMoving);
    }

    [Test]
    public void CalculateMovementForce_WithDownwardInput_ReturnsExpectedForce() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(0f, -1f), MockSpeed = 3f };
        var controller = new MarbleController(mockInput, mockInput.MockSpeed);

        var result = controller.CalculateMovementForce();
        Assert.AreEqual(0f, result.x, "Expected X=0f recieved " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f recieved " + result.y);
        Assert.AreEqual(-3f, result.z, "Expected Z=-3f recieved " + result.z);
        Assert.AreEqual(true, controller.IsMoving);
    }

    [Test]
    public void CalculateMovementForce_WithNoInput_ReturnsExpectedForce() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(0f, 0f), MockSpeed = 3f };
        var controller = new MarbleController(mockInput, mockInput.MockSpeed);

        var result = controller.CalculateMovementForce();
        Assert.AreEqual(0f, result.x, "Expected X=0f recieved " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f recieved " + result.y);
        Assert.AreEqual(0f, result.z, "Expected Z=0f recieved " + result.z);
        Assert.AreEqual(false, controller.IsMoving);
    }
}
