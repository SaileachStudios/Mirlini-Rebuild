using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class GameManagerBehavior : MonoBehaviour
    {
        [SerializeField] private AudioClip clangAudio;
        [SerializeField] private AudioClip dropAudio;
        [SerializeField] private LevelManager levelManager;

        private AudioSource audioSource;

        private AudioSFXController audioSFXController;

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
            if(clangAudio == null) {
                Debug.LogError("Clang Sound not set");
            }
            if (dropAudio == null) {
                Debug.LogError("drop Sound not set");
            }
            if (levelManager == null) {
                Debug.LogError("LevelManager not set");
            }
        }

        private void Start() {
            audioSFXController = new AudioSFXController();
            audioSFXController.Initialize(audioSource, dropAudio, clangAudio);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                levelManager.SetupLevel(0);
            }
        }
    }
}