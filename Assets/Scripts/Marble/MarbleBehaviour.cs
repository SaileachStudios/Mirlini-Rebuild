using SaileachStudios.Mirlini.InputSystem;
using UnityEngine;
using SaileachStudios.Mirlini.Board;
using System.Collections;
using SaileachStudios.Mirlini.Core;

namespace SaileachStudios.Mirlini.Marble
{
    public class MarbleBehaviour : MonoBehaviour
    {
        [SerializeField] private float speed = 1f;
        [SerializeField] private float shrinkDuration = 1f;

        private Rigidbody rb;
        private IInputProvider inputProvider;
        private MarbleController controller;
        private bool isPaused = false;
        private float currentSpeed = 0f;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
        }

        private void Start() {
            var factory = new InputProviderFactory(new PlatformDetector(), new UnityInputWrapper());
            inputProvider = factory.Create();
            controller = new MarbleController(inputProvider, speed);

            GameManagerBehavior.Instance.Events.OnMarbleDropped += OnMarbleDropped;
        }

        private void FixedUpdate() {
            if (!isPaused) {
                rb.AddForce(controller.CalculateMovementForce());
                currentSpeed = rb.velocity.magnitude;
            }
            else {
                rb.velocity = Vector3.zero;
            }
        }

        private void OnDrawGizmosSelected() {
            if (controller != null) {
                var force = controller.CalculateMovementForce();
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + force.normalized * 5f);
            }
        }

        public void OnMarbleDropped(bool isCorrect, Vector3 holeLocation) {
            isPaused = true;
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
        }

        private void OnDestroy() {
            GameManagerBehavior.Instance.Events.OnMarbleDropped -= OnMarbleDropped;
        }
    }
}