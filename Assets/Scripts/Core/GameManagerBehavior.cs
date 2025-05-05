using SaileachStudios.Mirlini.Audio;
using SaileachStudios.Mirlini.Board;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Core
{
    public class GameEvents {
        public event Action<bool, Vector3> OnMarbleDropped;
        public event Action<bool> OnHoleStatusChanged;

        public virtual void BallDropped(bool isCorrect, Vector3 location) {
            if(OnMarbleDropped != null) {
                OnMarbleDropped.Invoke(isCorrect, location);
            }
        }

        public virtual void ChangeHoleStatus(bool newStatus) {
            if (OnHoleStatusChanged != null) {
                OnHoleStatusChanged.Invoke(newStatus);
            }
        }
    }

    public class GameManagerBehavior : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;

        public static GameManagerBehavior Instance { get; private set; }
        public GameEvents Events { get; private set; } = new GameEvents();

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);

            Validate();
        }

        private void Validate() {
            if (levelManager == null) {
                Debug.LogError("LevelManager not set");
            }
        }

        private void Start() {
        }

        private void Update() {
            //Temp Testing code
            if (Input.GetKeyDown(KeyCode.Space)) {
                levelManager.SetupLevel(0);
            }
        }
    }
}