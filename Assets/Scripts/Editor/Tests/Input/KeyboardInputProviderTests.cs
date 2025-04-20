using NUnit.Framework;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;
using System.Collections.Generic;

public class KeyboardInputProviderTests
{
    [Test]
    public void GetInput_DoesNotThrow() {
        var mockInput = new MockInputWrapper { AxisValues = new Dictionary<string, float> { { "Vertical", 1f }, { "Horizontal", 0.5f } } };
        var inputProvider = new KeyboardInputProvider(mockInput, 100f);
        // Should not throw even if input axes are unset (in test context)
        Vector2 input = inputProvider.GetInput();
        Assert.IsInstanceOf<Vector2>(input);
    }

    [Test]
    public void GetInput_ReturnsCorrectValue() {
        var mockInput = new MockInputWrapper { AxisValues = new Dictionary<string, float> { { "Vertical", 1f }, { "Horizontal", 0.5f } } };
        var inputProvider = new KeyboardInputProvider(mockInput, 100f);
        // Should not throw even if input axes are unset (in test context)
        Vector2 result = inputProvider.GetInput();
        Assert.AreEqual(100f, result.x, $"Expected result.x to be 100f but was {result.x}");
        Assert.AreEqual(-50f, result.y, $"Expected result.y to be -50f but was {result.y}");
    }
}