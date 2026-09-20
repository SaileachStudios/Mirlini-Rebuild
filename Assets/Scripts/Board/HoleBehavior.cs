using SaileachStudios.Mirlini.Core;
using SaileachStudios.Mirlini.Marble;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class HoleBehavior : MonoBehaviour
    {
        [SerializeField] private GameObject correctIndicator;
        [SerializeField] private GameObject incorrectIndicator;
        public bool IsCorrect { get; private set; }
        private HoleController controller;
        private GameManagerBehavior manager;
        public bool IsConfigured => correctIndicator != null && incorrectIndicator != null && correctIndicator != incorrectIndicator;
        private void OnEnable() { Bind(GameManagerBehavior.Instance); }
        private void OnDisable() { controller = null; manager = null; }
        public void Bind(GameManagerBehavior owner) {
            if (owner == manager && controller != null) return;
            manager = owner;
            controller = owner == null ? null : new HoleController(owner.Events);
        }
        public void SetIsCorrectHole(bool correct) {
            if (!IsConfigured || controller == null) return;
            IsCorrect = correct;
            controller.SetAsCorrectHole(correct);
            correctIndicator.SetActive(correct);
            incorrectIndicator.SetActive(!correct);
        }
        private void OnTriggerEnter(Collider other) {
            var marble = other.GetComponent<MarbleBehaviour>();
            if (marble == null || !marble.CanCallForHelp || controller == null) return;
            controller.MarbleDropped(transform.position);
        }
    }
}
