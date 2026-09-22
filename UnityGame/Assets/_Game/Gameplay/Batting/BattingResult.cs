using System;
using UnityEngine;

namespace CricketGame.Gameplay.Batting
{
    [Serializable]
    public class BattingResult
    {
        public BattingShotType shotType;
        public TimingQuality timing;
        public float timingDelta;
        public ContactQuality contactQuality;
        public float distanceFromSweetSpot;
        public Vector3 exitVelocity;
        public float exitSpeedKph;
        public float launchAngle;
        public Vector3 direction;
        public float directionAngle;
        public float distanceEstimate;
        public bool isEdge;
        public bool isMiss;
        public int estimatedRuns;

        public BattingResult()
        {
            shotType = BattingShotType.Defensive;
            timing = TimingQuality.Miss;
            timingDelta = 0f;
            contactQuality = ContactQuality.Miss;
            distanceFromSweetSpot = 1f;
            exitVelocity = Vector3.zero;
            exitSpeedKph = 0f;
            launchAngle = 0f;
            direction = Vector3.forward;
            directionAngle = 0f;
            distanceEstimate = 0f;
            isEdge = false;
            isMiss = true;
            estimatedRuns = 0;
        }

        public override string ToString()
        {
            return string.Format("{0} | Timing: {1} ({2:F3}s) | Contact: {3} | Speed: {4:F1} km/h | Dist: {5:F1}m | Runs: {6}",
                shotType, timing, timingDelta, contactQuality, exitSpeedKph, distanceEstimate, estimatedRuns);
        }
    }
}
