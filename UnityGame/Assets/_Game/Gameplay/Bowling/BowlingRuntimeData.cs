using System;
using UnityEngine;

namespace CricketGame.Gameplay.Bowling
{
    [Serializable]
    public class BowlingRuntimeData
    {
        public BowlingState currentState = BowlingState.Idle;
        public BowlingDelivery activeDelivery = new BowlingDelivery();
        public bool isBowling = false;
        public bool canBowl = true;
        public float stateTimer = 0f;
        public float accuracyMeterValue = 0.85f;
        public BowlingReleaseData lastReleaseData = null;
        public BowlingResult lastResult = null;

        public void ResetForNextDelivery()
        {
            currentState = BowlingState.Idle;
            isBowling = false;
            canBowl = true;
            stateTimer = 0f;
            lastReleaseData = null;
        }
    }
}
