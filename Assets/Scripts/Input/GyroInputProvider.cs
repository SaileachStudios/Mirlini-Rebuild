using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class GyroInputProvider : IInputProvider
    {
        private IInputWrapper inputWrapper;
        private float speedModifer = 1f;

        public GyroInputProvider(IInputWrapper wrapper, float speedMod) {
            inputWrapper = wrapper;
            this.speedModifer = speedMod;
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

        public float GetSpeedModifier() {
            return speedModifer;
        }

    }
}