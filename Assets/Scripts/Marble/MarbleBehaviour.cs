using UnityEngine;
using SaileachStudios.Mirlini.Board;
using System.Collections;
using SaileachStudios.Mirlini.Core;

namespace SaileachStudios.Mirlini.Marble
{
    public class MarbleBehaviour : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float accelerationRate = 5f;
        [SerializeField] private float shrinkDuration = 1f;
        [SerializeField] private float maxSpeed = 10f; // Optional speed cap
        [SerializeField] private float stuckDetectionDuration = 1.5f;
        [SerializeField] private float stuckMovementThreshold = 0.1f;

        private Rigidbody rb;
        private MarbleController controller;
        private BallStateMachine stateMachine;
        private float currentSpeed = 0f;
        private bool pendingIsCorrect;
        private Vector3 pendingHolePosition;
        private Vector3 respawnPosition;
        private Vector3 initialScale;
        private Vector3 lastProgressPosition;
        private float stuckTimer = 0f;

        public BallState CurrentState => stateMachine?.CurrentState ?? BallState.Idle;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
            stateMachine = new BallStateMachine();
            respawnPosition = transform.position;
            initialScale = transform.localScale;
            lastProgressPosition = transform.position;
        }

        private void Start() {
            controller = new MarbleController(speed, accelerationRate); ;
            stateMachine.OnStateChanged += OnStateChanged;

            GameManagerBehavior.Instance.Events.OnMarbleDropped += OnMarbleDropped;
            GameManagerBehavior.Instance.Events.OnFixedUpdate += OnPlayerInput;
        }

        public bool StartPlaying() {
            respawnPosition = transform.position;
            transform.localScale = initialScale;
            if (rb != null) {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            ResetStuckTracking();
            if (CurrentState == BallState.Playing) {
                GameManagerBehavior.Instance?.SetPaused(false);
                return true;
            }

            return stateMachine != null && stateMachine.TransitionTo(BallState.Playing);
        }

        private void OnPlayerInput(bool isPaused, Vector2 playerInput) {
            if (controller == null || rb == null) {
                return;
            }

            if (isPaused || CurrentState != BallState.Playing) {
                rb.linearVelocity = Vector3.zero;
                return;
            }

            controller.CalculateTargetVelocity(playerInput);
            Vector3 force = controller.CalculateForceToReachTarget(rb.linearVelocity);

            // CHANGED: Set velocity directly instead of using AddForce
            // Preserve Y velocity for gravity
            Vector3 newVelocity = rb.linearVelocity + force * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);

            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > maxSpeed) {
                Vector3 cappedVelocity = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
            }

            currentSpeed = rb.linearVelocity.magnitude;
            UpdateStuckStatus();
        }

        public void OnMarbleDropped(bool isCorrect, Vector3 holeLocation) {
            pendingIsCorrect = isCorrect;
            pendingHolePosition = holeLocation;
            stateMachine?.TransitionTo(BallState.Falling);
        }

        private void OnStateChanged(BallState previousState, BallState newState) {
            switch (newState) {
                case BallState.Playing:
                    ResetStuckTracking();
                    GameManagerBehavior.Instance?.SetPaused(false);
                    break;
                case BallState.Falling:
                    ResetStuckTracking();
                    StartCoroutine(ShrinkOverTime(pendingHolePosition));
                    break;
                case BallState.Stuck:
                    RespawnFromStuck();
                    break;
                case BallState.Respawning:
                    ResetStuckTracking();
                    StartCoroutine(GrowOverTime(respawnPosition));
                    break;
            }
        }

        private void UpdateStuckStatus() {
            if (CurrentState != BallState.Playing || controller == null) {
                return;
            }

            if (!controller.IsMoving) {
                ResetStuckTracking();
                return;
            }

            if (Vector3.Distance(transform.position, lastProgressPosition) >= stuckMovementThreshold) {
                lastProgressPosition = transform.position;
                stuckTimer = 0f;
                return;
            }

            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckDetectionDuration) {
                stateMachine?.TransitionTo(BallState.Stuck);
            }
        }

        private void RespawnFromStuck() {
            if (rb != null) {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            transform.position = respawnPosition;
            transform.localScale = Vector3.zero;
            ResetStuckTracking();
            stateMachine?.TransitionTo(BallState.Respawning);
        }

        private void ResetStuckTracking() {
            stuckTimer = 0f;
            lastProgressPosition = transform.position;
        }

        IEnumerator ShrinkOverTime(Vector3 holePosition) {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < shrinkDuration) {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / shrinkDuration);
                transform.position = Vector3.Lerp(transform.position, holePosition, elapsed / shrinkDuration);
                yield return null;
            }

            transform.localScale = endScale;
            BallState nextState = pendingIsCorrect ? BallState.LevelComplete : BallState.Respawning;
            bool transitioned = stateMachine?.TransitionTo(nextState) ?? false;

            if (pendingIsCorrect && transitioned && GameManagerBehavior.Instance != null) {
                // This event is the post-shrink handoff point for level progression.
                GameManagerBehavior.Instance.Events.LevelCompleted();
            }
        }

        IEnumerator GrowOverTime(Vector3 startPosition) {
            transform.position = startPosition;
            Vector3 startScale = Vector3.zero; 
            Vector3 endScale = initialScale;
            float elapsed = 0f;

            while (elapsed < shrinkDuration) {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / shrinkDuration);
                yield return null;
            }

            transform.localScale = endScale;
            stateMachine?.TransitionTo(BallState.Playing);
        }

        private void OnDestroy() {
            if (stateMachine != null) {
                stateMachine.OnStateChanged -= OnStateChanged;
            }

            if (GameManagerBehavior.Instance != null) {
                GameManagerBehavior.Instance.Events.OnMarbleDropped -= OnMarbleDropped;
                GameManagerBehavior.Instance.Events.OnFixedUpdate -= OnPlayerInput;
            }
        }
    }
}
