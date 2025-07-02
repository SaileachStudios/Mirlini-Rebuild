using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class BoardTiltController
    {
        private readonly float maxTiltAngle;
        private readonly float tiltSpeed;

        private Vector3 currentRotation;

        public BoardTiltController(float maxTiltAngle, float tiltSpeed) {
            this.maxTiltAngle = maxTiltAngle;
            this.tiltSpeed = tiltSpeed;
            this.currentRotation = Vector3.zero;
        }

        public Quaternion UpdateTilt(Vector2 input, float deltaTime) {
            var targetEuler = new Vector3(-input.y, 0f, input.x) * maxTiltAngle;
            currentRotation = Vector3.Lerp(currentRotation, targetEuler, tiltSpeed * deltaTime);
            return Quaternion.Euler(currentRotation);
        }

        public Vector3 GetCurrentEuler() => currentRotation;
    }
}