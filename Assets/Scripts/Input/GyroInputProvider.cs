using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class GyroInputProvider : IInputProvider, IInputLifecycle
    {
        private IInputWrapper inputWrapper;
        private float speedModifer = 1f;

        public GyroInputProvider(IInputWrapper wrapper, float speedMod) {
            inputWrapper = wrapper;
            this.speedModifer = speedMod;
        }

        private bool active;
        public void Activate() {
            if (active) return;
            inputWrapper.GyroEnabled = true;
            active = true;
        }

        public void Deactivate() {
            if (!active) return;
            inputWrapper.GyroEnabled = false;
            active = false;
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
