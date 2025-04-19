using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem { 
    public interface IInputWrapper {
        //Keyboard Input
        public float GetAxisValue(string axisName);

        //Touch Input
        Vector2 screenSize { get; }
        int TouchCount { get; }
        Vector2 GetTouchPosition(int index);

        //Gyro Input
        public bool GyroEnabled { get; set; }
        public Vector3 GyroGravity { get; set; }

    }
}