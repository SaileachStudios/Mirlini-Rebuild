using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem { 
    public interface IInputWrapper {
        int TouchCount { get; }
        Vector2 GetTouchPosition(int index);
        public float GetAxisValue(string axisName);
    }
}