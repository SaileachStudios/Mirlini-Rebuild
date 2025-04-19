using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.InputSystem
{
    public class TouchInputProvider : IInputProvider
    {
        private readonly IInputWrapper inputWrapper;
        private Vector2 screenCenter = Vector2.zero;

        public TouchInputProvider(IInputWrapper wrapper, Vector2 screenSize) {
            inputWrapper = wrapper;
            screenCenter = new Vector2(screenSize.x / 2f, screenSize.y / 2f);
        }

        public Vector2 GetInput() {
            if(inputWrapper.TouchCount > 0) {
                Vector2 touchPos = inputWrapper.GetTouchPosition(0);
                float x = (touchPos.y / screenCenter.y) - 1f;
                float y = -1f * ((touchPos.x / screenCenter.x) - 1f);
                return new Vector2(x, y);
            }
            return Vector2.zero;
        }

        public float GetSunRotationModifier() {
            throw new System.NotImplementedException();
        }
    }
}