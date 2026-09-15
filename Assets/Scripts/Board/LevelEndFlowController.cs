using System.Collections;
using SaileachStudios.Mirlini.Core;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    [RequireComponent(typeof(LevelManager))]
    public class LevelEndFlowController : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private float temporaryCompletionDelay = 0.15f;

        private bool isSubscribedToGameEvents = false;
        private GameEvents subscribedEvents;
        private bool isHandlingLevelCompletion = false;
        private Coroutine activeCompletionFlow = null;
        private Coroutine pendingSubscriptionRetry = null;

        private void Awake() {
            if (levelManager == null) {
                levelManager = GetComponent<LevelManager>();
            }
        }

        private void OnEnable() {
            TrySubscribeToGameEvents();
        }

        private void OnDisable() {
            UnsubscribeFromGameEvents();
            StopActiveCompletionFlow();
            StopPendingSubscriptionRetry();
        }

        private void OnLevelCompleted() {
            if (isHandlingLevelCompletion || levelManager == null) {
                return;
            }

            isHandlingLevelCompletion = true;
            activeCompletionFlow = StartCoroutine(HandleLevelCompletedFlow());
        }

        private IEnumerator HandleLevelCompletedFlow() {
            // Temporary scaffolding for future end-of-level UI. This currently follows Time.timeScale.
            if (temporaryCompletionDelay > 0f) {
                yield return new WaitForSeconds(temporaryCompletionDelay);
            }

            Debug.Log("Level completed. Routing progression through LevelEndFlowController.");
            if (!levelManager.LoadNextLevel()) {
                Debug.Log("No additional levels are configured.");
            }

            // Re-open the completion gate on the next frame so duplicate
            // LevelCompleted events raised in the same frame are ignored.
            yield return null;

            activeCompletionFlow = null;
            isHandlingLevelCompletion = false;
        }

        private void TrySubscribeToGameEvents() {
            if (isSubscribedToGameEvents) {
                return;
            }

            if (GameManagerBehavior.Instance == null) {
                if (pendingSubscriptionRetry == null) {
                    Debug.LogWarning("LevelEndFlowController could not subscribe because GameManagerBehavior.Instance was null. Retrying next frame.");
                    pendingSubscriptionRetry = StartCoroutine(WaitForGameManagerAndSubscribe());
                }
                return;
            }

            subscribedEvents = GameManagerBehavior.Instance.Events;
            subscribedEvents.OnLevelCompleted += OnLevelCompleted;
            isSubscribedToGameEvents = true;
        }

        private void UnsubscribeFromGameEvents() {
            if (!isSubscribedToGameEvents || subscribedEvents == null) {
                return;
            }

            subscribedEvents.OnLevelCompleted -= OnLevelCompleted;
            subscribedEvents = null;
            isSubscribedToGameEvents = false;
        }

        private void StopActiveCompletionFlow() {
            if (activeCompletionFlow == null) {
                isHandlingLevelCompletion = false;
                return;
            }

            StopCoroutine(activeCompletionFlow);
            activeCompletionFlow = null;
            isHandlingLevelCompletion = false;
        }

        private IEnumerator WaitForGameManagerAndSubscribe() {
            while (!isSubscribedToGameEvents && GameManagerBehavior.Instance == null) {
                yield return null;
            }

            pendingSubscriptionRetry = null;
            TrySubscribeToGameEvents();
        }

        private void StopPendingSubscriptionRetry() {
            if (pendingSubscriptionRetry == null) {
                return;
            }

            StopCoroutine(pendingSubscriptionRetry);
            pendingSubscriptionRetry = null;
        }
    }
}
