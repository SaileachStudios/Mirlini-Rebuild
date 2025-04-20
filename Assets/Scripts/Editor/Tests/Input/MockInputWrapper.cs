
using SaileachStudios.Mirlini.InputSystem;
using System.Collections.Generic;
using UnityEngine;

public class MockInputWrapper : IInputWrapper
{
    //Keyboard
    public Dictionary<string, float> AxisValues { get; set; } = new Dictionary<string, float> { { "Vertical", 1f }, { "Horizontal", 0.5f } };
    public float GetAxisValue(string axisName) {
        return AxisValues.ContainsKey(axisName) ? AxisValues[axisName] : 0f;
    }

    //Touch
    public Vector2 screenSize { get; set; } = new Vector2(1200, 800);
    public int TouchCount { get; set; } = 1;
    public Vector2 TouchPosition { get; set; } = new Vector2(300, 500);
    public Vector2 GetTouchPosition(int index) {
        return TouchPosition;
    }

    //Gyro
    public bool GyroEnabled { get; set; } = false;
    public Vector3 GyroGravity { get; set; } = Vector3.zero;

}
