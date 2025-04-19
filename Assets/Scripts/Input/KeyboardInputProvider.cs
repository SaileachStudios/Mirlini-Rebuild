using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class KeyboardInputProvider : IInputProvider
    {
        private readonly IInputWrapper inputWrapper;
        private float speedModifer = 1f;

        public KeyboardInputProvider(IInputWrapper wrapper, float speedMod) {
            inputWrapper = wrapper;
            speedModifer = speedMod;
        }

        public Vector2 GetInput() {
            return new Vector2(
                inputWrapper.GetAxisValue("Vertical"),
                -1 * inputWrapper.GetAxisValue("Horizontal")
            ) * GetSpeedModifier();
        }

        public float GetSpeedModifier() {
            return speedModifer;
        }
    }
}