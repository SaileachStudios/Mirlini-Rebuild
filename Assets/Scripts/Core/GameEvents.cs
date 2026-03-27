using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Core
{
    public class GameEvents
    {
        public event Action<bool, Vector3> OnMarbleDropped;
        public event Action<bool> OnHoleStatusChanged;
        public event Action OnLevelCompleted;
        public event Action<bool, Vector2> OnFixedUpdate;

        public virtual void BallDropped(bool isCorrect, Vector3 location) {
            if (OnMarbleDropped != null) {
                OnMarbleDropped.Invoke(isCorrect, location);
            }
        }

        public virtual void ChangeHoleStatus(bool newStatus) {
            if (OnHoleStatusChanged != null) {
                OnHoleStatusChanged.Invoke(newStatus);
            }
        }

        public virtual void LevelCompleted() {
            if (OnLevelCompleted != null) {
                OnLevelCompleted.Invoke();
            }
        }

        public virtual void InputUpdated(bool isPaused, Vector2 playerInput) {
            if (OnFixedUpdate != null) {
                OnFixedUpdate.Invoke(isPaused, playerInput);
            }
        }
    }
}
