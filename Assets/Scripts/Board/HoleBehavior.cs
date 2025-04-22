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

        void Awake() {
            controller = new HoleController();
            controller.onHoleStatusChanged += OnStatusChanged;
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
            controller.MarbleDropped();
        }

        public HoleController GetHoleController() {
            return controller;
        }
    }
}