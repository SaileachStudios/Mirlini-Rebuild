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
        public static GameManagerBehavior Instance { get; private set; }
        public GameEvents Events { get; private set; } = new GameEvents();

        private IInputProvider inputProvider;
        private bool isPaused = false;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public bool IsGyroEnabled() {
            return inputProvider is GyroInputProvider;
        }

        public void SetPaused(bool paused) {
            isPaused = paused;
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
