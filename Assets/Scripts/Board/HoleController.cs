using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class HoleController
    {
        public delegate void OnMarbleDropped(bool isCorrect);
        public OnMarbleDropped onMarbleDropped;
        public delegate void OnHoleStatusChanged(bool isCorrect);
        public OnHoleStatusChanged onHoleStatusChanged;

        private bool isCorrectHole = false;

        public void SetAsCorrectHole(bool isCorrect) {
            isCorrectHole = isCorrect;
            if(onHoleStatusChanged != null) {
                onHoleStatusChanged(isCorrectHole);
            }
        }

        public void MarbleDropped() {
            if (onMarbleDropped != null) {
                onMarbleDropped(isCorrectHole);
            }
        }

    }
}