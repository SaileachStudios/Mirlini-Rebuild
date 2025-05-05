using SaileachStudios.Mirlini.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class AudioSFXController
    {
        private AudioClip clangSFX;
        private AudioClip dropSFX;
        private AudioSource audioSource;

        public AudioSFXController() {

        }

        public void Initialize(AudioSource source, AudioClip drop, AudioClip clang) {
            audioSource = source;
            dropSFX = drop;
            clangSFX = clang;

            GameManagerBehavior.Instance.Events.OnMarbleDropped += OnBallDropped;
        }

        private void OnBallDropped(bool isCorrect, Vector3 holeLocation) {
            audioSource.clip = dropSFX;
            audioSource.Play();
        }


    }
}