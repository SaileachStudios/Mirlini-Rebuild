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

        private void Validate() {
            if (levelManager == null) {
                Debug.LogError("LevelManager not set");
            }
        }

        private void Start() {
            var factory = new InputProviderFactory(new PlatformDetector(), new UnityInputWrapper());
            inputProvider = factory.Create();

        }

        private void Update() {
            //Temp Testing code
            if (Input.GetKeyDown(KeyCode.Space)) {
                levelManager.SetupLevel(0);
            }
        }

        private void FixedUpdate() {
            Vector2 playerInput = inputProvider.GetInput().normalized;
            Events.InputUpdated(isPaused, playerInput);
        }

        private void OnBalledDropped(bool isCorrect, Vector3 location) {
            isPaused = true;
        }
    }
}