using SaileachStudios.Mirlini.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class BoardTiltBehavior : MonoBehaviour
    {
        [SerializeField] private float maxTiltAngle = 10f;
        [SerializeField] private float tiltSpeed = 5f;

        private BoardTiltController controller;

        private void Start() {
            controller = new BoardTiltController(maxTiltAngle, tiltSpeed);
            GameManagerBehavior.Instance.Events.OnFixedUpdate += UpdateInput;
        }

        private void UpdateInput(bool isPaused, Vector2 playerInput) {
            if (GameManagerBehavior.Instance.IsGyroEnabled()) return;

            if (!isPaused) {
                Quaternion targetRotation = controller.UpdateTilt(playerInput, Time.fixedDeltaTime);
                transform.rotation = targetRotation;
            }
        }
    }
}