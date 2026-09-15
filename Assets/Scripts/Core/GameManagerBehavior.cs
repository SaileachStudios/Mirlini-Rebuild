using SaileachStudios.Mirlini.InputSystem;
using UnityEngine;

namespace SaileachStudios.Mirlini.Core
{
    [DefaultExecutionOrder(-1000)]
    public class GameManagerBehavior : MonoBehaviour
    {
        public static GameManagerBehavior Instance { get; private set; }
        public GameEvents Events { get; private set; } = new GameEvents();
        private IInputProvider inputProvider;
        private bool paused, resolvingHole, applicationPaused;
        public bool IsPaused => paused || resolvingHole || applicationPaused || !isActiveAndEnabled;

        private void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            inputProvider = new InputProviderFactory(new PlatformDetector(), new UnityInputWrapper()).Create();
            DontDestroyOnLoad(gameObject);
        }
        private void OnEnable() { if (Instance == this && !applicationPaused) (inputProvider as IInputLifecycle)?.Activate(); }
        private void OnDisable() { (inputProvider as IInputLifecycle)?.Deactivate(); }
        private void OnApplicationPause(bool value) {
            applicationPaused = value;
            if (value) (inputProvider as IInputLifecycle)?.Deactivate();
            else if (Instance == this && isActiveAndEnabled) (inputProvider as IInputLifecycle)?.Activate();
        }
        public bool IsGyroEnabled() => inputProvider is GyroInputProvider;
        public void SetPaused(bool value) { paused = value; }
        // Only an accepted marble resolution owns this pause reason. Menu/app pause is independent.
        public void SetResolvingHole(bool value) { resolvingHole = value; }
        private void FixedUpdate() {
            if (Instance != this || inputProvider == null) return;
            Events.InputUpdated(IsPaused, IsPaused ? Vector2.zero : InputRange.Clamp(inputProvider.GetInput()));
        }
        private void OnDestroy() {
            (inputProvider as IInputLifecycle)?.Deactivate();
            if (Instance == this) Instance = null;
        }
    }
}
