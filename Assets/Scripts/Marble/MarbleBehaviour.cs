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
        private float currentSpeed = 0f;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
        }

        private void Start() {
            controller = new MarbleController(speed, accelerationRate); ;

            GameManagerBehavior.Instance.Events.OnMarbleDropped += OnMarbleDropped;
            GameManagerBehavior.Instance.Events.OnFixedUpdate += OnPlayerInput;
        }

        private void OnPlayerInput(bool isPaused, Vector2 playerInput) {
            if (controller == null || rb == null) {
                return;
            }

            if (!isPaused) {

                Vector3 targetVelocity = controller.CalculateTargetVelocity(playerInput);

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
            else {
                rb.linearVelocity = Vector3.zero;
            }
        }

        public void OnMarbleDropped(bool isCorrect, Vector3 holeLocation) {
            StartCoroutine(ShrinkOverTime(holeLocation));
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
            //Add in animation done event
        }

        //TODO for Marble respawn later.
        IEnumerator GrowOverTime(Vector3 startPosition) {
            Vector3 startScale = Vector3.zero; 
            Vector3 endScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < shrinkDuration) {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / shrinkDuration);
                transform.position = Vector3.Lerp(transform.position, startPosition, elapsed / shrinkDuration);
                yield return null;
            }

            transform.localScale = endScale;
            //Add in animation done event
        }

        private void OnDestroy() {
            GameManagerBehavior.Instance.Events.OnMarbleDropped -= OnMarbleDropped;
            GameManagerBehavior.Instance.Events.OnFixedUpdate -= OnPlayerInput;
        }
    }
}