using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;

public class TouchInputProviderTests
{
    [Test]
    public void GetInput_ReturnsCorrectVector_withoutTouch() {
        var mockInput = new MockInputWrapper { TouchPosition = new Vector2(600, 400), TouchCount = 0 };
        var screenSize = new Vector2(1200, 800);

        var testTouchHandler = new TouchInputProvider(mockInput, 0.2f, screenSize);

        var result = testTouchHandler.GetInput();
        Assert.AreEqual(0f, result.x);
        Assert.AreEqual(0f, result.y);
    }

    [Test]
    public void GetInput_ReturnsCorrectVector_withTouchCenter() {
        var mockInput = new MockInputWrapper { TouchPosition = new Vector2(600, 400), TouchCount = 1 };
        var screenSize = new Vector2(1200, 800);

        var testTouchHandler = new TouchInputProvider(mockInput, 0.2f, screenSize);

        var result = testTouchHandler.GetInput();
        Assert.AreEqual(0f, result.x);
        Assert.AreEqual(0f, result.y);
    }

    [Test]
    public void GetInput_ReturnsCorrectVector_withTouchUpperLeft() {
        var mockInput = new MockInputWrapper { TouchPosition = new Vector2(0, 800), TouchCount = 1 };
        var screenSize = new Vector2(1200, 800);

        var testTouchHandler = new TouchInputProvider(mockInput, 0.2f, screenSize);

        var result = testTouchHandler.GetInput();
        Assert.AreEqual(0.2f, result.x);
        Assert.AreEqual(0.2f, result.y);
    }

    [Test]
    public void GetInput_ReturnsCorrectVector_withTouchLowerRight() {
        var mockInput = new MockInputWrapper { TouchPosition = new Vector2(1200, 0), TouchCount = 1 };
        var screenSize = new Vector2(1200, 800);

        var testTouchHandler = new TouchInputProvider(mockInput, 0.2f, screenSize);

        var result = testTouchHandler.GetInput();
        Assert.AreEqual(-0.2f, result.x);
        Assert.AreEqual(-0.2f, result.y);
    }
}