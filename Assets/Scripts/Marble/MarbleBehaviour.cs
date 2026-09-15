using System.Collections;
using UnityEngine;
using SaileachStudios.Mirlini.Core;

namespace SaileachStudios.Mirlini.Marble
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class MarbleBehaviour : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float accelerationRate = 5f;
        [SerializeField] private float shrinkDuration = 1f;
        [SerializeField] private float maxSpeed = 10f;
        private Rigidbody rb;
        private MarbleController controller;
        private BallStateMachine stateMachine;
        private GameManagerBehavior manager;
        private GameEvents subscribedEvents;
        private Vector3 initialScale;
        private Coroutine resolution;
        private bool pendingIsCorrect;
        private Vector3 pendingHolePosition;
        public BallState CurrentState => stateMachine?.CurrentState ?? BallState.Idle;
        public bool CanStartPlaying => isActiveAndEnabled && (CurrentState == BallState.Idle || CurrentState == BallState.Playing || CurrentState == BallState.LevelComplete);
        public bool CanCallForHelp => isActiveAndEnabled && CurrentState == BallState.Playing && manager != null && !manager.IsPaused && resolution == null;
        public float CollisionRadius {
            get {
                var sphere = GetComponent<SphereCollider>();
                // Setup may validate the next level while the visual marble is shrunk to zero.
                // Always use the collider at its fully playable scale.
                Vector3 parentScale = transform.parent == null ? Vector3.one : transform.parent.lossyScale;
                Vector3 scale = Vector3.Scale(initialScale, parentScale);
                return sphere.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            }
        }
        private Vector3 respawnPosition;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
            initialScale = transform.localScale;
            rb.isKinematic = true; // inert until the level has explicitly supplied its start
            controller = new MarbleController(speed, accelerationRate);
            stateMachine = new BallStateMachine();
        }
        private void OnEnable() { Bind(GameManagerBehavior.Instance); }
        private void OnDisable() {
            Unsubscribe();
            if (resolution != null) StopCoroutine(resolution);
            resolution = null;
            if (rb != null) rb.isKinematic = true;
            // A disabled interrupted attempt is not resumed implicitly. Level setup starts a new one.
            stateMachine = new BallStateMachine();
            manager = null;
        }
        public void Bind(GameManagerBehavior owner) {
            if (owner == manager && subscribedEvents != null) return;
            Unsubscribe(); manager = owner;
            if (owner == null || !isActiveAndEnabled) return;
            subscribedEvents = owner.Events;
            subscribedEvents.OnMarbleDropped += OnMarbleDropped;
            subscribedEvents.OnFixedUpdate += OnPlayerInput;
        }
        private void Unsubscribe() {
            if (subscribedEvents == null) return;
            subscribedEvents.OnMarbleDropped -= OnMarbleDropped;
            subscribedEvents.OnFixedUpdate -= OnPlayerInput;
            subscribedEvents = null;
        }
        public bool StartPlaying() {
            if (!CanStartPlaying || rb == null || stateMachine == null || manager == null) return false;
            respawnPosition = transform.position;
            transform.localScale = initialScale;
            rb.isKinematic = false;
            ClearVelocity();
            if (CurrentState != BallState.Playing && !stateMachine.TransitionTo(BallState.Playing)) return false;
            manager.SetResolvingHole(false);
            return true;
        }
        private void OnPlayerInput(bool isPaused, Vector2 input) {
            if (rb == null || controller == null) return;
            if (isPaused || CurrentState != BallState.Playing) { if (!rb.isKinematic) ClearVelocity(); return; }
            controller.CalculateTargetVelocity(input);
            Vector3 force = controller.CalculateForceToReachTarget(rb.linearVelocity);
            Vector3 velocity = rb.linearVelocity + force * Time.fixedDeltaTime;
            Vector3 horizontal = Vector3.ClampMagnitude(new Vector3(velocity.x, 0f, velocity.z), maxSpeed);
            rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
        }
        public void OnMarbleDropped(bool isCorrect, Vector3 holeLocation) { TryEnterHole(isCorrect, holeLocation); }
        public bool TryEnterHole(bool isCorrect, Vector3 holeLocation) {
            if (!CanCallForHelp || !IsFinite(holeLocation)) return false;
            // Freeze the outcome before any event/coroutine can re-enter this method.
            if (!stateMachine.TransitionTo(BallState.Falling)) return false;
            pendingIsCorrect = isCorrect;
            pendingHolePosition = holeLocation;
            manager.SetResolvingHole(true);
            ClearVelocity(); rb.isKinematic = true;
            resolution = StartCoroutine(ResolveHole());
            return true;
        }
        private IEnumerator ResolveHole() {
            // Always yield once so the coroutine handle exists even with zero-duration tests.
            yield return null;
            Vector3 scale = transform.localScale, start = transform.position;
            for (float elapsed = 0; elapsed < shrinkDuration;) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / shrinkDuration);
                transform.localScale = Vector3.Lerp(scale, Vector3.zero, t);
                transform.position = Vector3.Lerp(start, pendingHolePosition, t);
                yield return null;
            }
            transform.localScale = Vector3.zero;
            transform.position = pendingHolePosition;
            if (pendingIsCorrect) {
                stateMachine.TransitionTo(BallState.LevelComplete);
                resolution = null;
                manager.Events.LevelCompleted();
                yield break;
            }
            stateMachine.TransitionTo(BallState.Respawning);
            transform.position = respawnPosition;
            for (float elapsed = 0; elapsed < shrinkDuration;) {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, elapsed / shrinkDuration);
                yield return null;
            }
            transform.localScale = initialScale;
            rb.isKinematic = false;
            ClearVelocity();
            stateMachine.TransitionTo(BallState.Playing);
            resolution = null;
            manager.SetResolvingHole(false);
        }
        // The board owns legal-position calculation; this method is internal to that validated path.
        internal bool ApplyHelpPosition(Vector3 position) {
            if (!CanCallForHelp || !IsFinite(position)) return false;
            rb.position = position;
            transform.position = position;
            ClearVelocity();
            return true;
        }
        private void ClearVelocity() { if (rb.isKinematic) return; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        private static bool IsFinite(Vector3 p) => !(float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z) || float.IsInfinity(p.x) || float.IsInfinity(p.y) || float.IsInfinity(p.z));
    }
}
