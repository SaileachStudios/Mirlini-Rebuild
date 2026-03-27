using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaileachStudios.Mirlini.Marble
{
    public class BallStateMachine
    {
        public event Action<BallState, BallState> OnStateChanged;

        private BallState currentState = BallState.Idle;

        // Define valid transitions
        private static readonly Dictionary<BallState, HashSet<BallState>> validTransitions = new Dictionary<BallState, HashSet<BallState>>
        {
            { BallState.Idle, new HashSet<BallState> { BallState.Playing } },
            { BallState.Playing, new HashSet<BallState> { BallState.Falling, BallState.Stuck } },
            { BallState.Falling, new HashSet<BallState> { BallState.Respawning, BallState.LevelComplete } },
            { BallState.Respawning, new HashSet<BallState> { BallState.Playing } },
            { BallState.Stuck, new HashSet<BallState> { BallState.Respawning } },
            { BallState.LevelComplete, new HashSet<BallState> { BallState.Playing } }
        };

        public BallState CurrentState => currentState;

        public bool TransitionTo(BallState newState)
        {
            if (!IsValidTransition(currentState, newState))
            {
                return false;
            }

            BallState oldState = currentState;
            currentState = newState;

            OnStateChanged?.Invoke(oldState, newState);

            return true;
        }

        private bool IsValidTransition(BallState from, BallState to)
        {
            if (!validTransitions.ContainsKey(from))
            {
                return false;
            }

            return validTransitions[from].Contains(to);
        }
    }
}
