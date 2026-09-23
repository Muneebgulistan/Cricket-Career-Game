using System;
using UnityEngine;

namespace CricketGame.Gameplay.Running
{
    [Serializable]
    public class RunningSettings
    {
        public float minRunSpeed = 5.2f;
        public float maxRunSpeed = 7.2f;
        public float acceleration = 8.0f;
        public float turnSpeedFactor = 0.65f;
        public float turnDuration = 0.35f;
        public float diveDistance = 0.85f;
        public float diveSpeedMultiplier = 1.2f;
        public float fatiguePenaltyPerRun = 0.04f;
        public float pitchLaneOffset = 0.8f;
    }
}
