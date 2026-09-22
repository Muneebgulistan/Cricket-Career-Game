using System;
using UnityEngine;

namespace CricketGame.Gameplay.Batting
{
    [Serializable]
    public class BattingRuntimeData
    {
        public BattingState currentState = BattingState.Ready;
        public BattingShotType currentShotType = BattingShotType.StraightDrive;
        public bool isSwinging = false;
        public bool canSwing = true;
        public float swingTimer = 0f;
        public float idealContactTime = 0f;
        public float actualSwingTime = 0f;
        public Vector3 lastContactPosition = Vector3.zero;
        public BattingResult lastResult = null;
        public bool hasMadeContactThisSwing = false;

        public void ResetForNextDelivery()
        {
            currentState = BattingState.Ready;
            isSwinging = false;
            canSwing = true;
            swingTimer = 0f;
            idealContactTime = 0f;
            actualSwingTime = 0f;
            lastContactPosition = Vector3.zero;
            hasMadeContactThisSwing = false;
        }
    }
}
