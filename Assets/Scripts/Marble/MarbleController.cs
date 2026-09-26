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
        private float releaseResponse, maxAcceleration, deadband = VELOCITY_DEADZONE;

        public MarbleController(SaileachStudios.Mirlini.Core.GameplayFeel feel) : this(feel.Speed, feel.Response) {
            releaseResponse=feel.ReleaseResponse;maxAcceleration=feel.MaxAcceleration;deadband=feel.VelocityDeadband;
        }
        // Integrates only horizontal motion. Exact target clamping prevents timestep overshoot.
        public Vector3 Step(Vector3 velocity, Vector2 input, float deltaTime) {
            CalculateTargetVelocity(InputRange.Clamp(input));
            Vector3 horizontal=new Vector3(velocity.x,0,velocity.z);
            Vector3 change=CalculateForceToReachTarget(velocity)*Mathf.Max(0,deltaTime);
            Vector3 difference=currentTargetVelocity-horizontal;
            if(change.sqrMagnitude>difference.sqrMagnitude) change=difference;
            horizontal=Vector3.ClampMagnitude(horizontal+change,speed);
            if(input==Vector2.zero && horizontal.magnitude<deadband) horizontal=Vector3.zero;
            return new Vector3(horizontal.x,velocity.y,horizontal.z);
        }

        public MarbleController(float levelSpeed, float accelRate = 10f) {
            speed = levelSpeed;
            accelerationRate = accelRate;
            releaseResponse=accelRate;maxAcceleration=levelSpeed*MAX_FORCE_MULTIPLIER;
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
            if (velocityDifference.magnitude < deadband) {
                return Vector3.zero;
            }

            // Apply acceleration rate to control how quickly we reach target
            Vector3 force = velocityDifference * (currentTargetVelocity == Vector3.zero ? releaseResponse : accelerationRate);

            // FORCE CAPPING: Prevent explosive corrections
            float maxForce = maxAcceleration;
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
