using System;
using UnityEngine;

namespace CricketGame.Gameplay.Running
{
    [Serializable]
    public class RunningRuntimeData
    {
        public RunningState strikerState;
        public RunningState nonStrikerState;
        public Vector3 strikerPosition;
        public Vector3 nonStrikerPosition;
        public CreaseEnd strikerHomeEnd;
        public CreaseEnd nonStrikerHomeEnd;
        public CreaseEnd strikerTargetEnd;
        public CreaseEnd nonStrikerTargetEnd;
        public float strikerCurrentSpeed;
        public float nonStrikerCurrentSpeed;
        public int runsAttempted;
        public int runsCompleted;
        public bool isRunActive;
        public bool strikerDiving;
        public bool nonStrikerDiving;
        public float currentRunDuration;

        public RunningRuntimeData()
        {
            Reset();
        }

        public void Reset()
        {
            strikerState = RunningState.Idle;
            nonStrikerState = RunningState.Idle;
            strikerPosition = RunningTarget.GetCreasePosition(CreaseEnd.StrikerEnd, true);
            nonStrikerPosition = RunningTarget.GetCreasePosition(CreaseEnd.NonStrikerEnd, false);
            strikerHomeEnd = CreaseEnd.StrikerEnd;
            nonStrikerHomeEnd = CreaseEnd.NonStrikerEnd;
            strikerTargetEnd = CreaseEnd.NonStrikerEnd;
            nonStrikerTargetEnd = CreaseEnd.StrikerEnd;
            strikerCurrentSpeed = 0f;
            nonStrikerCurrentSpeed = 0f;
            runsAttempted = 0;
            runsCompleted = 0;
            isRunActive = false;
            strikerDiving = false;
            nonStrikerDiving = false;
            currentRunDuration = 0f;
        }

        public void SwapEnds()
        {
            CreaseEnd temp = strikerHomeEnd;
            strikerHomeEnd = nonStrikerHomeEnd;
            nonStrikerHomeEnd = temp;
        }
    }
}
