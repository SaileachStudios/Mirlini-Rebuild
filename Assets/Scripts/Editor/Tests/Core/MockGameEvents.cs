using SaileachStudios.Mirlini.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Board
{
    public class MockGameEvents : GameEvents
    {
        //ChangeHoleStatus
        public bool HoleStatusWasChanged = false;
        public bool HoleStatusChangedTo = false;

        //BallDropped
        public bool BallDroppedCalled = false;
        public bool BallDroppedCalledWithStatus = false;
        public Vector3 BallDroppedCalledWithLocation = Vector3.zero;
        public bool LevelCompletedCalled = false;

        public override void BallDropped(bool isCorrect, Vector3 location) {
            BallDroppedCalled = true;
            BallDroppedCalledWithStatus = isCorrect;
            BallDroppedCalledWithLocation = location;
        }

        public override void ChangeHoleStatus(bool newStatus) {
            HoleStatusWasChanged = true;
            HoleStatusChangedTo = newStatus;
        }

        public override void LevelCompleted() {
            LevelCompletedCalled = true;
        }
    }
}
