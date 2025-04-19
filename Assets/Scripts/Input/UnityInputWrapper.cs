using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class UnityInputWrapper : IInputWrapper
    {
        public int TouchCount => Input.touchCount;

        public Vector2 GetTouchPosition(int index) {
            return Input.GetTouch(index).position;
        }

        public float GetAxisValue(string name) {
            return Input.GetAxis(name);
        }
    }
}