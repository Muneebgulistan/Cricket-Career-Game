using System;
using UnityEngine;

namespace CricketGame.Gameplay.Fielding
{
    [Serializable]
    public class FieldingRuntimeData
    {
        public FieldingPosition activeFielderPosition;
        public FieldingState currentState;
        public FieldingActionType currentAction;
        public Vector3 interceptionPoint;
        public FieldingTarget selectedThrowTarget;
        public bool hasBall;
        public float distanceToBall;
        public bool isUserControlled;
        public int fumbleCount;
        public FieldingDecision lastDecision;
        public FieldingResult lastResult;
        public float timeInCurrentState;
        public bool pickupAttempted;
        public bool catchAttempted;
        public bool throwTriggered;

        public FieldingRuntimeData()
        {
            Reset();
        }

        public void Reset()
        {
            activeFielderPosition = FieldingPosition.Cover;
            currentState = FieldingState.Idle;
            currentAction = FieldingActionType.None;
            interceptionPoint = Vector3.zero;
            selectedThrowTarget = FieldingTarget.WicketKeeper;
            hasBall = false;
            distanceToBall = 0f;
            isUserControlled = false;
            fumbleCount = 0;
            lastDecision = null;
            lastResult = null;
            timeInCurrentState = 0f;
            pickupAttempted = false;
            catchAttempted = false;
            throwTriggered = false;
        }

        public void TransitionState(FieldingState newState)
        {
            currentState = newState;
            timeInCurrentState = 0f;
        }
    }
}
