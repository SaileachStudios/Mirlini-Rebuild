using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaileachStudios.Mirlini.Core;

namespace SaileachStudios.Mirlini.Board
{
    public class HoleController
    {
        private bool isCorrectHole = false;
        private GameEvents events = null; 

        public HoleController() {
            Debug.LogError("HoleController Instatiated Without GameEvents");
        }

        public HoleController(GameEvents eventManager ) {
            events = eventManager;
        }
        public void SetAsCorrectHole(bool isCorrect) {
            isCorrectHole = isCorrect;
            events.ChangeHoleStatus(isCorrect);
        }

        public void MarbleDropped(Vector3 holeLocation) {
            events.BallDropped(isCorrectHole, holeLocation);
        }

    }
}