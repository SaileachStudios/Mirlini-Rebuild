using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.Board;

public class HoleControllerTests
{
    private bool holeIsCorrect = false;
    private bool holeWasChanged = false;
    private HoleController controller;
    
    [SetUp]
    public void Setup() {
        controller = new HoleController();
        controller.onMarbleDropped += MarbleListener;
        controller.onHoleStatusChanged += HoleChangedListener;
    }

    private void MarbleListener(bool isCorrect, Vector3 holeLocation) { 
        holeIsCorrect = isCorrect;
    }
    private void HoleChangedListener(bool isCorrect) { 
        holeWasChanged = isCorrect;
    }

    [Test]
    public void Test_OnMarbleDroppedCalled_WithFalse() {
        holeIsCorrect = true;
        controller.SetAsCorrectHole(false);
        controller.MarbleDropped(Vector3.zero);
        Assert.AreEqual(false, holeIsCorrect);
    }

    [Test]
    public void Test_OnMarbleDroppedCalled_WithTrue() {
        holeIsCorrect = false;
        controller.SetAsCorrectHole(true);
        controller.MarbleDropped(Vector3.zero);
        Assert.AreEqual(true, holeIsCorrect);
    }

    [Test]
    public void Test_OnCorrectHoleChanged() {
        holeWasChanged = false;
        controller.SetAsCorrectHole(true);

        Assert.AreEqual(true, holeWasChanged);
    }

    [TearDown]
    public void TearDown() {
        controller.onMarbleDropped -= MarbleListener;
        controller.onHoleStatusChanged -= HoleChangedListener;
    }
}
