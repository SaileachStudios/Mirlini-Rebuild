
using SaileachStudios.Mirlini.InputSystem;
using System.Collections.Generic;
using UnityEngine;

public class MockInputWrapper : IInputWrapper
{
    public int TouchCount { get; set; } = 1;
    public Vector2 TouchPosition { get; set; } = new Vector2(300, 500);
    public Dictionary<string, float> AxisValues { get; set; } = new Dictionary<string, float> { { "Vertical", 1f }, { "Horizontal", 0.5f } };

    public Vector2 GetTouchPosition(int index) {
        return TouchPosition;
    }

    public float GetAxisValue(string axisName) {
        Debug.Log("GetAxisValue(" + axisName + ")");
        return AxisValues.ContainsKey(axisName) ? AxisValues[axisName] : 0f;
    }
}
