using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaileachStudios.Mirlini.InputSystem;

namespace SaileachStudios.Mirlini.Marble
{
    public class MarbleController
    {
        private float speed = 0;
        private float accelerationRate = 1f;
        private Vector3 currentTargetVelocity = Vector3.zero;


        // Deadzone to prevent oscillation from tiny velocities
        private const float VELOCITY_DEADZONE = 0.05f;

        // Maximum force per frame to prevent explosive corrections
        private const float MAX_FORCE_MULTIPLIER = 15f;

        public MarbleController(float levelSpeed, float accelRate = 10f) {
            speed = levelSpeed;
            accelerationRate = accelRate;
        }

        public Vector3 CalculateTargetVelocity(Vector2 playerInput) {
            currentTargetVelocity = new Vector3(playerInput.x * speed, 0f, playerInput.y * speed);
            return currentTargetVelocity;
        }

        public Vector3 CalculateForceToReachTarget(Vector3 currentVelocity) {
            // Calculate the difference between where we want to go and where we're going
            Vector3 currentHorizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            Vector3 velocityDifference = currentTargetVelocity - currentHorizontalVelocity;

            // DEADZONE: Ignore tiny velocity differences to prevent oscillation
            if (velocityDifference.magnitude < VELOCITY_DEADZONE) {
                return Vector3.zero;
            }

            // Apply acceleration rate to control how quickly we reach target
            Vector3 force = velocityDifference * accelerationRate;

            // FORCE CAPPING: Prevent explosive corrections
            float maxForce = speed * MAX_FORCE_MULTIPLIER;
            if (force.magnitude > maxForce) {
                force = force.normalized * maxForce;
            }

            return force;
        }

        public bool IsMoving {
            get {
                return currentTargetVelocity.magnitude > 0.01f;
            }
        }

        public float AccelerationRate {
            get { return accelerationRate; }
            set { accelerationRate = value; }
        }

    }
}
