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

        private Rigidbody rb;
        private MarbleController controller;
        private BallStateMachine stateMachine;
        private float currentSpeed = 0f;
        private bool pendingIsCorrect;
        private Vector3 pendingHolePosition;
        private Vector3 respawnPosition;
        private Vector3 initialScale;

        public BallState CurrentState => stateMachine?.CurrentState ?? BallState.Idle;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
            stateMachine = new BallStateMachine();
            respawnPosition = transform.position;
            initialScale = transform.localScale;
        }

        private void Start() {
            controller = new MarbleController(speed, accelerationRate); ;
            stateMachine.OnStateChanged += OnStateChanged;

            GameManagerBehavior.Instance.Events.OnMarbleDropped += OnMarbleDropped;
            GameManagerBehavior.Instance.Events.OnFixedUpdate += OnPlayerInput;
        }

        public bool StartPlaying() {
            respawnPosition = transform.position;
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
        }

        public void OnMarbleDropped(bool isCorrect, Vector3 holeLocation) {
            pendingIsCorrect = isCorrect;
            pendingHolePosition = holeLocation;
            stateMachine?.TransitionTo(BallState.Falling);
        }

        private void OnStateChanged(BallState previousState, BallState newState) {
            switch (newState) {
                case BallState.Playing:
                    GameManagerBehavior.Instance?.SetPaused(false);
                    break;
                case BallState.Falling:
                    StartCoroutine(ShrinkOverTime(pendingHolePosition));
                    break;
                case BallState.Respawning:
                    StartCoroutine(GrowOverTime(respawnPosition));
                    break;
            }
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
            stateMachine?.TransitionTo(pendingIsCorrect ? BallState.LevelComplete : BallState.Respawning);
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
