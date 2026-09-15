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

        private GameEvents subscribedEvents;
        private void OnEnable() {
            if (GameManagerBehavior.Instance == null) return;
            subscribedEvents = GameManagerBehavior.Instance.Events;
            subscribedEvents.OnMarbleDropped += OnBallDropped;
        }

        private void OnBallDropped(bool isCorrect, Vector3 holeLocation) {
            if (audioSource != null && !audioSource.isPlaying) {
                audioSource.clip = dropSFX;
                audioSource.Play();
            }
        }

        private void OnDisable() {
            if (subscribedEvents != null) subscribedEvents.OnMarbleDropped -= OnBallDropped;
            subscribedEvents = null;
        }
    }
}
