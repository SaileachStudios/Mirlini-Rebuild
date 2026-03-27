using SaileachStudios.Mirlini.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class HoleBehavior : MonoBehaviour
    {
        [SerializeField] private GameObject correctIndicator;
        [SerializeField] private GameObject incorrectIndicator;

        private HoleController controller;

        void Start() {
            EnsureController();
            GameManagerBehavior.Instance.Events.OnHoleStatusChanged += OnStatusChanged;
        }

        public void SetIsCorrectHole(bool isCorrect) {
            if (!EnsureController()) {
                return;
            }

            controller.SetAsCorrectHole(isCorrect);
        }

        void OnStatusChanged(bool isCorrect) {
            if (isCorrect) {
                correctIndicator.SetActive(true);
                incorrectIndicator.SetActive(false);
            }
            else {
                correctIndicator.SetActive(false);
                incorrectIndicator.SetActive(true);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.GetComponent<SaileachStudios.Mirlini.Marble.MarbleBehaviour>() == null) {
                return;
            }

            controller.MarbleDropped(transform.position);
        }

        private bool EnsureController() {
            if (controller != null) {
                return true;
            }

            if (GameManagerBehavior.Instance == null) {
                return false;
            }

            controller = new HoleController(GameManagerBehavior.Instance.Events);
            return true;
        }

        private void OnDestroy() {
            if (GameManagerBehavior.Instance != null) {
                GameManagerBehavior.Instance.Events.OnHoleStatusChanged -= OnStatusChanged;
            }
        }
    }
}
