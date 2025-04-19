using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class KeyboardInputProvider : IInputProvider
    {
        private readonly float sunRotationModifier;
        private readonly IInputWrapper inputWrapper;

        public KeyboardInputProvider(IInputWrapper wrapper, float sunRotMod = -60f) {
            inputWrapper = wrapper;
            sunRotationModifier = sunRotMod;
        }

        public Vector2 GetInput() {
            return new Vector2(
                inputWrapper.GetAxisValue("Vertical"),
                -1 * inputWrapper.GetAxisValue("Horizontal")
            ) * GetSpeedModifier();
        }

        public float GetSunRotationModifier() {
            return sunRotationModifier;
        }

        public float GetSpeedModifier() {
            return 1f;
        }
    }
}