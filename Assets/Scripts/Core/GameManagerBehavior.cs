using SaileachStudios.Mirlini.Audio;
using SaileachStudios.Mirlini.Board;
using SaileachStudios.Mirlini.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Core
{
    public class GameManagerBehavior : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;

        public static GameManagerBehavior Instance { get; private set; }
        public GameEvents Events { get; private set; } = new GameEvents();
        public bool HasLevelManager => levelManager != null;

        private IInputProvider inputProvider;
        private bool isPaused = false;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);

            Validate();
        }

        public bool IsGyroEnabled() {
            return inputProvider is GyroInputProvider;
        }

        public void SetPaused(bool paused) {
            isPaused = paused;
        }

        public void LoadNextLevel() {
            if (levelManager == null) {
                return;
            }

            if (!levelManager.LoadNextLevel()) {
                Debug.Log("No additional levels are configured.");
            }
        }

        private void Validate() {
            if (levelManager == null) {
                Debug.LogError("LevelManager not set");
            }
        }

        private void Start() {
            var factory = new InputProviderFactory(new PlatformDetector(), new UnityInputWrapper());
            inputProvider = factory.Create();
            Events.OnMarbleDropped += OnBalledDropped;
        }

        private void FixedUpdate() {
            Vector2 playerInput = inputProvider.GetInput().normalized;
            Events.InputUpdated(isPaused, playerInput);
        }

        private void OnBalledDropped(bool isCorrect, Vector3 location) {
            isPaused = true;
        }

        private void OnDestroy() {
            Events.OnMarbleDropped -= OnBalledDropped;

            if (Instance == this) {
                Instance = null;
            }
        }
    }
}
