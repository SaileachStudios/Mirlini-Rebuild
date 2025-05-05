using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.Board;

public class HoleControllerTests
{
    private HoleController controller;
    private MockGameEvents gameEvents;
    [SetUp]
    public void Setup() {
        gameEvents = new MockGameEvents();
        controller = new HoleController(gameEvents);
    }

    [Test]
    public void Test_OnMarbleDroppedCalled_WithFalse() {
        controller.SetAsCorrectHole(false);
        controller.MarbleDropped(Vector3.up);
        Assert.AreEqual(true, gameEvents.BallDroppedCalled);
        Assert.AreEqual(false, gameEvents.BallDroppedCalledWithStatus);
        Assert.AreEqual(Vector3.up, gameEvents.BallDroppedCalledWithLocation);
    }

    [Test]
    public void Test_OnMarbleDroppedCalled_WithTrue() {
        controller.SetAsCorrectHole(true);
        controller.MarbleDropped(Vector3.up);
        Assert.AreEqual(true, gameEvents.BallDroppedCalled);
        Assert.AreEqual(true, gameEvents.BallDroppedCalledWithStatus);
        Assert.AreEqual(Vector3.up, gameEvents.BallDroppedCalledWithLocation);
    }

    [Test]
    public void Test_HoleStatusChangedToTrue() {
        controller.SetAsCorrectHole(true);

        Assert.AreEqual(true, gameEvents.HoleStatusWasChanged);
        Assert.AreEqual(true, gameEvents.HoleStatusChangedTo);
    }

    [Test]
    public void Test_HoleStatusChangedToFalse() {
        controller.SetAsCorrectHole(false);

        Assert.AreEqual(true, gameEvents.HoleStatusWasChanged, "ChangeHoleStatus not called");
        Assert.AreEqual(false, gameEvents.HoleStatusChangedTo, "ChangeHoleStatus called with wrong value");
    }

    [TearDown]
    public void TearDown() {
        controller = null;
        gameEvents = null;
    }
}
