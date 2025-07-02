using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.Board;

public class BoardTiltControllerTests
{
    [Test]
    public void Tilt_AccumulatesOverFrames_Horizontially_Correctly() {
        var controller = new BoardTiltController(30f, 5f); // 30 deg max tilt, speed 5

        Vector2 input = new Vector2(1f, 0f); // Tilt right ? negative Z
        controller.UpdateTilt(input, 0.02f);  // Frame 1
        Assert.AreEqual(3f, controller.GetCurrentEuler().z, 0.1f);  // ? 30 * 0.02 * 5

        controller.UpdateTilt(input, 0.02f);  // Frame 2
        Assert.AreEqual(5.7f, controller.GetCurrentEuler().z, 0.1f);

        controller.UpdateTilt(input, 0.02f);  // Frame 3
        Assert.AreEqual(8.13f, controller.GetCurrentEuler().z, 0.1f);
    }

    [Test]
    public void Tilt_AccumulatesOverFrames_Vertically_Correctly() {
        var controller = new BoardTiltController(30f, 5f); // 30 deg max tilt, speed 5

        Vector2 input = new Vector2(0f, 1f); // 
        controller.UpdateTilt(input, 0.02f);  // Frame 1
        Assert.AreEqual(-3f, controller.GetCurrentEuler().x, 0.1f);  // ? 30 * 0.02 * 5

        controller.UpdateTilt(input, 0.02f);  // Frame 2
        Assert.AreEqual(-5.7f, controller.GetCurrentEuler().x, 0.1f);

        controller.UpdateTilt(input, 0.02f);  // Frame 3
        Assert.AreEqual(-8.13f, controller.GetCurrentEuler().x, 0.1f);
    }

    [Test]
    public void Tilt_AccumulatesOverFrames_Diagonialy_Correctly() {
        var controller = new BoardTiltController(30f, 5f); // 30 deg max tilt, speed 5

        Vector2 input = new Vector2(1f, 1f); // 
        controller.UpdateTilt(input, 0.02f);  // Frame 1
        Assert.AreEqual(3f, controller.GetCurrentEuler().z, 0.1f);  // ? 30 * 0.02 * 5
        Assert.AreEqual(-3f, controller.GetCurrentEuler().x, 0.1f);  // ? 30 * 0.02 * 5

        controller.UpdateTilt(input, 0.02f);  // Frame 2
        Assert.AreEqual(5.7f, controller.GetCurrentEuler().z, 0.1f);
        Assert.AreEqual(-5.7f, controller.GetCurrentEuler().x, 0.1f);

        controller.UpdateTilt(input, 0.02f);  // Frame 3
        Assert.AreEqual(8.13f, controller.GetCurrentEuler().z, 0.1f);
        Assert.AreEqual(-8.13f, controller.GetCurrentEuler().x, 0.1f);
    }
}
