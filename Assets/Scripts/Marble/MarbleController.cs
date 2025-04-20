using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;

namespace SaileachStudios.Mirlini.Marble
{
    public class MarbleController
    {
        private IInputProvider inputProvider = null;
        private float speed = 0;
        private float velocity = 0;

        public MarbleController(IInputProvider provider, float levelSpeed) {
            inputProvider = provider;
            speed = levelSpeed;
        }

        public Vector3 CalculateMovementForce() {
            Vector2 playerInput = inputProvider.GetInput().normalized;
            Vector3 result = new Vector3(playerInput.x * speed, 0f, playerInput.y * speed);
            velocity = result.magnitude;

            return result;
        }

        public bool IsMoving {
            get { return velocity != 0f; }
        }
    }
}