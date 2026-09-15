using SaileachStudios.Mirlini.Core;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class BoardTiltBehavior : MonoBehaviour
    {
        [SerializeField] private float maxTiltAngle = 10f;
        [SerializeField] private float tiltSpeed = 5f;
        private BoardTiltController controller;
        private GameManagerBehavior manager;
        private GameEvents subscribedEvents;
        private void Awake() { controller = new BoardTiltController(maxTiltAngle, tiltSpeed); }
        private void OnEnable() {
            manager = GameManagerBehavior.Instance;
            if (manager == null) return;
            subscribedEvents = manager.Events;
            subscribedEvents.OnFixedUpdate += UpdateInput;
        }
        private void OnDisable() {
            if (subscribedEvents != null) subscribedEvents.OnFixedUpdate -= UpdateInput;
            subscribedEvents = null; manager = null;
        }
        private void UpdateInput(bool paused, Vector2 input) {
            if (manager == null || manager.IsGyroEnabled() || paused) return;
            transform.rotation = controller.UpdateTilt(input, Time.fixedDeltaTime);
        }
    }
}
