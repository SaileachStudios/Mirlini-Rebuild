using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;

public class MockInputProvider : IInputProvider
{
    public Vector2 MockInput { get; set; } = new Vector2(0f, 0f);
    public float MockSpeed { get; set; } = 1f;

    public Vector2 GetInput() {
        return MockInput;
    }

    public float GetSpeedModifier() {
        return MockSpeed;
    }
}
