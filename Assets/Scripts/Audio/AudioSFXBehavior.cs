using SaileachStudios.Mirlini.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Audio
{
    public class AudioSFXBehavior : MonoBehaviour
    {
        [SerializeField] private AudioClip clangSFX;
        [SerializeField] private AudioClip dropSFX;

        private AudioSource audioSource;

        private void Awake() {
            if (clangSFX == null) {
                Debug.LogError("Clang Sound not set");
            }
            if (dropSFX == null) {
                Debug.LogError("drop Sound not set");
            }
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) {
                Debug.LogError("AudioSource not added to behavior");
            }
        }

        private void Start() {
            GameManagerBehavior.Instance.Events.OnMarbleDropped += OnBallDropped;
        }

        private void OnBallDropped(bool isCorrect, Vector3 holeLocation) {
            if (!audioSource.isPlaying) {
                audioSource.clip = dropSFX;
                audioSource.Play();
            }
        }

        private void OnDestroy() {
            GameManagerBehavior.Instance.Events.OnMarbleDropped -= OnBallDropped;
        }
    }
}