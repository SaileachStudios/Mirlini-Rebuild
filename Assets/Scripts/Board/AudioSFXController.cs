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

            HoleController controller = GameObject.FindObjectOfType<HoleBehavior>().GetHoleController();
            controller.onMarbleDropped += OnBallDropped;
        }

        private void OnBallDropped(bool isCorrect) {
            audioSource.clip = dropSFX;
            audioSource.Play();
        }


    }
}