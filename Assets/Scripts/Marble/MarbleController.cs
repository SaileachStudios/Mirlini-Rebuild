using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;

namespace SaileachStudios.Mirlini.Marble
{
    public class MarbleController
    {
        private float speed = 0;
        private float velocity = 0;

        public MarbleController(float levelSpeed) {
            speed = levelSpeed;
        }

        public Vector3 CalculateMovementForce(Vector2 playerInput) {
            Vector3 result = new Vector3(playerInput.x * speed, 0f, playerInput.y * speed);
            velocity = result.magnitude;

            return result;
        }

        public bool IsMoving {
            get { return velocity != 0f; }
        }

    }
}