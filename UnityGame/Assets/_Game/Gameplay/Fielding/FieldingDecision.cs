using System;
using UnityEngine;

namespace CricketGame.Gameplay.Fielding
{
    [Serializable]
    public class FieldingDecision
    {
        public FieldingPosition selectedFielderPosition;
        public Vector3 interceptionPoint;
        public float estimatedArrivalTime;
        public float ballArrivalTime;
        public bool isCatchOpportunity;
        public float catchDifficulty;
        public FieldingActionType recommendedAction;
        public FieldingTarget bestThrowTarget;
        public float score;

        public FieldingDecision()
        {
            selectedFielderPosition = FieldingPosition.Cover;
            interceptionPoint = Vector3.zero;
            estimatedArrivalTime = 0f;
            ballArrivalTime = 0f;
            isCatchOpportunity = false;
            catchDifficulty = 0f;
            recommendedAction = FieldingActionType.GroundPickup;
            bestThrowTarget = FieldingTarget.WicketKeeper;
            score = 0f;
        }
    }
}
