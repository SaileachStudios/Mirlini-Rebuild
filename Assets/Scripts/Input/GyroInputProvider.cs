using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class GyroInputProvider : IInputProvider
    {
        private IInputWrapper inputWrapper;

        public GyroInputProvider(IInputWrapper wrapper) {
            inputWrapper = wrapper;
        }

        public bool GyroEnabled {
            get {
                return inputWrapper.GyroEnabled;
            }
        }

        public Vector2 GetInput() {
            Vector3 gravity = inputWrapper.GyroGravity * GetSpeedModifier();
            return new Vector2 (gravity.y, -gravity.x);
        }

        public float GetSunRotationModifier() {
            return 0;
        }

        public float GetSpeedModifier() {
            return 2f;
        }

    }
}