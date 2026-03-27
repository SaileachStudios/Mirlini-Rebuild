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
        private bool isSubscribedToEvents = false;

        private void OnEnable() {
            TrySubscribeToEvents();
        }

        void Start() {
            TrySubscribeToEvents();
        }

        private void OnDisable() {
            UnsubscribeFromEvents();
        }

        public void SetIsCorrectHole(bool isCorrect) {
            if (!TrySubscribeToEvents()) {
                return;
            }

            controller.SetAsCorrectHole(isCorrect);
            OnStatusChanged(isCorrect);
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

            if (!TrySubscribeToEvents()) {
                return;
            }

            controller.MarbleDropped(transform.position);
        }

        private bool TrySubscribeToEvents() {
            if (!EnsureController()) {
                return false;
            }

            if (isSubscribedToEvents) {
                return true;
            }

            GameManagerBehavior.Instance.Events.OnHoleStatusChanged += OnStatusChanged;
            isSubscribedToEvents = true;
            return true;
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

        private void UnsubscribeFromEvents() {
            if (!isSubscribedToEvents || GameManagerBehavior.Instance == null) {
                return;
            }

            GameManagerBehavior.Instance.Events.OnHoleStatusChanged -= OnStatusChanged;
            isSubscribedToEvents = false;
        }
    }
}
