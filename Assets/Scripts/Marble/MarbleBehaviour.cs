using SaileachStudios.Mirlini.InputSystem;
using UnityEngine;

namespace SaileachStudios.Mirlini.Marble
{
    public class MarbleBehaviour : MonoBehaviour
    {
        [SerializeField] private float speed = 1f;

        private Rigidbody rb;
        private IInputProvider inputProvider;
        private MarbleController controller;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
        }

        private void Start() {
            var factory = new InputProviderFactory(new PlatformDetector(), new UnityInputWrapper());
            inputProvider = factory.Create();
            controller = new MarbleController(inputProvider, speed);
        }

        private void FixedUpdate() {
            rb.AddForce(controller.CalculateMovementForce());
        }

        private void OnDrawGizmosSelected() {
            if (controller != null) {
                var force = controller.CalculateMovementForce();
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + force.normalized * 5f);
            }
        }
    }
}