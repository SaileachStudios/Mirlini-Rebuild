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
            controller = new HoleController(GameManagerBehavior.Instance.Events);
            GameManagerBehavior.Instance.Events.OnHoleStatusChanged += OnStatusChanged;
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

        private void OnDestroy() {
            if (GameManagerBehavior.Instance != null) {
                GameManagerBehavior.Instance.Events.OnHoleStatusChanged -= OnStatusChanged;
            }
        }
    }
}
