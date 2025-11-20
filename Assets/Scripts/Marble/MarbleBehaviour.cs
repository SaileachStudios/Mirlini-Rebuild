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
        private MarbleController controller;
        private float currentSpeed = 0f;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
        }

        private void Start() {
            controller = new MarbleController(speed);

            GameManagerBehavior.Instance.Events.OnMarbleDropped += OnMarbleDropped;
            GameManagerBehavior.Instance.Events.OnFixedUpdate += OnPlayerInput;
        }

        private void OnPlayerInput(bool isPaused, Vector2 playerInput) {
            if (!isPaused) {
                rb.AddForce(controller.CalculateMovementForce(playerInput));
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