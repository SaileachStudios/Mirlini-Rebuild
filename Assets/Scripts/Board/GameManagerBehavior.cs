using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class GameManagerBehavior : MonoBehaviour
    {
        [SerializeField] private AudioClip clangAudio;
        [SerializeField] private AudioClip dropAudio;

        private AudioSource audioSource;

        private AudioSFXController audioSFXController;

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
        }

        private void Start() {
            audioSFXController = new AudioSFXController();
            audioSFXController.Initialize(audioSource, dropAudio, clangAudio);
        }
    }
}