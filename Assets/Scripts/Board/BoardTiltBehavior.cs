using SaileachStudios.Mirlini.Core;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class BoardTiltBehavior : MonoBehaviour
    {
        private BoardTiltController controller;
        private GameManagerBehavior manager;
        private GameEvents subscribedEvents;
        private void Awake() { var feel=GameplayFeel.Load();controller = new BoardTiltController(feel.TiltDegrees, feel.TiltResponse); }
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
