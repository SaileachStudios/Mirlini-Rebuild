using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;
using SaileachStudios.Mirlini.Marble;

public class MarbleControllerTests
{
    // ===== TARGET VELOCITY CALCULATION TESTS =====

    [Test]
    public void CalculateTargetVelocity_WithRightwardInput_ReturnsExpectedVelocity() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(1f, 0f), MockSpeed = 2f };
        var controller = new MarbleController(mockInput.MockSpeed);
        var result = controller.CalculateTargetVelocity(mockInput.MockInput);

        Assert.AreEqual(2f, result.x, "Expected X=2f received " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f received " + result.y);
        Assert.AreEqual(0f, result.z, "Expected Z=0f received " + result.z);
        Assert.AreEqual(true, controller.IsMoving);
    }

    [Test]
    public void CalculateTargetVelocity_WithLeftwardInput_ReturnsExpectedVelocity() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(-1f, 0f), MockSpeed = 3f };
        var controller = new MarbleController(mockInput.MockSpeed);
        var result = controller.CalculateTargetVelocity(mockInput.MockInput);

        Assert.AreEqual(-3f, result.x, "Expected X=-3f received " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f received " + result.y);
        Assert.AreEqual(0f, result.z, "Expected Z=0f received " + result.z);
        Assert.AreEqual(true, controller.IsMoving);
    }

    [Test]
    public void CalculateTargetVelocity_WithUpwardInput_ReturnsExpectedVelocity() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(0f, 1f), MockSpeed = 2f };
        var controller = new MarbleController(mockInput.MockSpeed);
        var result = controller.CalculateTargetVelocity(mockInput.MockInput);

        Assert.AreEqual(0f, result.x, "Expected X=0f received " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f received " + result.y);
        Assert.AreEqual(2f, result.z, "Expected Z=2f received " + result.z);
        Assert.AreEqual(true, controller.IsMoving);
    }

    [Test]
    public void CalculateTargetVelocity_WithDownwardInput_ReturnsExpectedVelocity() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(0f, -1f), MockSpeed = 3f };
        var controller = new MarbleController(mockInput.MockSpeed);
        var result = controller.CalculateTargetVelocity(mockInput.MockInput);

        Assert.AreEqual(0f, result.x, "Expected X=0f received " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f received " + result.y);
        Assert.AreEqual(-3f, result.z, "Expected Z=-3f received " + result.z);
        Assert.AreEqual(true, controller.IsMoving);
    }

    [Test]
    public void CalculateTargetVelocity_WithNoInput_ReturnsExpectedVelocity() {
        var mockInput = new MockInputProvider { MockInput = new Vector2(0f, 0f), MockSpeed = 3f };
        var controller = new MarbleController(mockInput.MockSpeed);
        var result = controller.CalculateTargetVelocity(mockInput.MockInput);

        Assert.AreEqual(0f, result.x, "Expected X=0f received " + result.x);
        Assert.AreEqual(0f, result.y, "Expected Y=0f received " + result.y);
        Assert.AreEqual(0f, result.z, "Expected Z=0f received " + result.z);
        Assert.AreEqual(false, controller.IsMoving);
    }

    // ===== FORCE CALCULATION TESTS =====

    [Test]
    public void CalculateForceToReachTarget_WhenStationary_ReturnsFullForce() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(1f, 0f)); // Target: (5, 0, 0)

        Vector3 currentVelocity = Vector3.zero;
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Raw force: 5 * 10 = 50, cap at 5 * 15 = 75, so uncapped
        Assert.AreEqual(50f, force.x, 0.01f, "Expected force X=50f received " + force.x);
        Assert.AreEqual(0f, force.y, 0.01f, "Expected force Y=0f received " + force.y);
        Assert.AreEqual(0f, force.z, 0.01f, "Expected force Z=0f received " + force.z);
    }

    [Test]
    public void CalculateForceToReachTarget_WhenAtTargetSpeed_ReturnsZeroForce() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(1f, 0f)); // Target: (5, 0, 0)

        Vector3 currentVelocity = new Vector3(5f, 0f, 0f); // Already at target
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Force should be zero since we're at target (within deadzone)
        Assert.AreEqual(0f, force.x, 0.01f, "Expected force X=0f received " + force.x);
        Assert.AreEqual(0f, force.y, 0.01f, "Expected force Y=0f received " + force.y);
        Assert.AreEqual(0f, force.z, 0.01f, "Expected force Z=0f received " + force.z);
    }

    [Test]
    public void CalculateForceToReachTarget_WhenMovingTooFast_ReturnsNegativeForce() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(1f, 0f)); // Target: (5, 0, 0)

        Vector3 currentVelocity = new Vector3(8f, 0f, 0f); // Moving too fast
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Force: (5 - 8) * 10 = -30, within cap of 75
        Assert.AreEqual(-30f, force.x, 0.01f, "Expected force X=-30f received " + force.x);
        Assert.AreEqual(0f, force.y, 0.01f, "Expected force Y=0f received " + force.y);
        Assert.AreEqual(0f, force.z, 0.01f, "Expected force Z=0f received " + force.z);
    }

    [Test]
    public void CalculateForceToReachTarget_IgnoresVerticalVelocity() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(1f, 0f)); // Target: (5, 0, 0)

        Vector3 currentVelocity = new Vector3(0f, -10f, 0f); // Falling but not moving horizontally
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Should apply full horizontal force: 5 * 10 = 50
        Assert.AreEqual(50f, force.x, 0.01f, "Expected force X=50f received " + force.x);
        Assert.AreEqual(0f, force.y, 0.01f, "Expected force Y=0f received " + force.y);
        Assert.AreEqual(0f, force.z, 0.01f, "Expected force Z=0f received " + force.z);
    }

    [Test]
    public void CalculateForceToReachTarget_WithDiagonalMovement_CalculatesCorrectly() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(1f, 1f)); // Target: (5, 0, 5)

        Vector3 currentVelocity = new Vector3(2f, 0f, 2f); // Partially there
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Force = (5-2, 0, 5-2) * 10 = (30, 0, 30), within cap
        Assert.AreEqual(30f, force.x, 0.01f, "Expected force X=30f received " + force.x);
        Assert.AreEqual(0f, force.y, 0.01f, "Expected force Y=0f received " + force.y);
        Assert.AreEqual(30f, force.z, 0.01f, "Expected force Z=30f received " + force.z);
    }

    // ===== DEADZONE TESTS =====

    [Test]
    public void CalculateForceToReachTarget_WithVeryTinyVelocityDifference_ReturnsZeroForce() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(0f, 0f)); // Target: stop

        Vector3 currentVelocity = new Vector3(0.01f, 0f, 0.01f); // Extremely tiny drift
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Within deadzone (0.05), should return zero
        Assert.AreEqual(0f, force.magnitude, 0.01f, "Should ignore tiny velocity within deadzone");
    }

    [Test]
    public void CalculateForceToReachTarget_WithSmallButSignificantDifference_AppliesForce() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(0f, 0f)); // Target: stop

        Vector3 currentVelocity = new Vector3(0.5f, 0f, 0f); // Small but above deadzone
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Should apply corrective force (0.5 is above 0.05 deadzone)
        Assert.AreNotEqual(0f, force.magnitude, "Should correct velocity above deadzone");
        Assert.Less(force.x, 0f, "Force should be negative to slow down");
    }

    // ===== FORCE CAPPING TESTS =====

    [Test]
    public void CalculateForceToReachTarget_CapsExtremeForce() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(1f, 0f)); // Target: (5, 0, 0)

        Vector3 currentVelocity = new Vector3(-20f, 0f, 0f); // Going opposite direction fast
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Raw force: (5 - (-20)) * 10 = 250, should cap at 5 * 15 = 75
        float maxExpectedForce = 75f;
        Assert.LessOrEqual(force.magnitude, maxExpectedForce + 0.1f,
            "Force should be capped at speed * MAX_FORCE_MULTIPLIER");
        Assert.AreEqual(75f, force.magnitude, 0.1f, "Force should be exactly at cap");
    }

    [Test]
    public void CalculateForceToReachTarget_DoesNotCapSmallForces() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(1f, 0f)); // Target: (5, 0, 0)

        Vector3 currentVelocity = new Vector3(4.5f, 0f, 0f); // Close to target
        Vector3 force = controller.CalculateForceToReachTarget(currentVelocity);

        // Force: (5 - 4.5) * 10 = 5, well below cap of 75
        Assert.AreEqual(5f, force.x, 0.01f, "Small forces should not be capped");
    }

    // ===== ACCELERATION RATE TESTS =====

    [Test]
    public void AccelerationRate_CanBeModified() {
        var controller = new MarbleController(5f, 10f);

        Assert.AreEqual(10f, controller.AccelerationRate);

        controller.AccelerationRate = 20f;

        Assert.AreEqual(20f, controller.AccelerationRate);
    }

    [Test]
    public void AccelerationRate_AffectsForceCalculation_WhenUncapped() {
        var controller = new MarbleController(5f, 5f); // Lower accel rate
        controller.CalculateTargetVelocity(new Vector2(1f, 0f));

        Vector3 currentVelocity = Vector3.zero;
        Vector3 forceWithRate5 = controller.CalculateForceToReachTarget(currentVelocity);

        controller.AccelerationRate = 10f;
        Vector3 forceWithRate10 = controller.CalculateForceToReachTarget(currentVelocity);

        // Both should be uncapped (25 and 50, both under 75 cap)
        Assert.AreEqual(forceWithRate5.x * 2f, forceWithRate10.x, 0.01f,
            "Doubling acceleration rate should double force when uncapped");
    }

    // ===== IsMoving TESTS =====

    [Test]
    public void IsMoving_ReturnsFalse_WhenTargetVelocityIsNearZero() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(0.001f, 0.001f)); // Very small input

        Assert.AreEqual(false, controller.IsMoving, "Should not be considered moving with tiny input");
    }

    [Test]
    public void IsMoving_ReturnsTrue_WhenTargetVelocityIsSignificant() {
        var controller = new MarbleController(5f, 10f);
        controller.CalculateTargetVelocity(new Vector2(0.5f, 0f)); // Noticeable input

        Assert.AreEqual(true, controller.IsMoving, "Should be considered moving with significant input");
    }

}
