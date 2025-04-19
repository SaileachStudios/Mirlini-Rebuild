using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class UnityInputWrapper : IInputWrapper {

        //Keyboard
        public float GetAxisValue(string name) {
            return Input.GetAxis(name);
        }

        //Touch
        public Vector2 screenSize => new Vector2(Screen.width, Screen.height);
        public int TouchCount => Input.touchCount;
        public Vector2 GetTouchPosition(int index) {
            return Input.GetTouch(index).position;
        }

        //Gyro
        public bool GyroEnabled {
            get {
                return Input.gyro.enabled;
            }
            set { return; }
        }

        public Vector3 GyroGravity {
            get {
                return Input.gyro.gravity;
            }
            set { return; }
        }
    }
}