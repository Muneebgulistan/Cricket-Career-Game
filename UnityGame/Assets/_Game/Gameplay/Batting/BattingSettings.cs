using System;
using UnityEngine;

namespace CricketGame.Gameplay.Batting
{
    [Serializable]
    public class BattingSettings
    {
        [Header("Timing Thresholds (seconds)")]
        public float perfectTimingWindow = 0.05f;
        public float goodTimingWindow = 0.12f;
        public float validTimingWindow = 0.25f;

        [Header("Contact Zone Radii (meters)")]
        public float sweetSpotRadius = 0.08f;
        public float goodContactRadius = 0.18f;
        public float edgeContactRadius = 0.28f;

        [Header("Power & Exit Velocity Scaling")]
        public float maxExitSpeedKph = 155f;
        public float minExitSpeedKph = 25f;
        public float defensiveExitSpeedKph = 18f;

        [Header("Player Attribute Influence Weights")]
        [Range(0f, 1f)] public float attributeImpactWeight = 0.25f; // Attributes modify outcome up to 25%, timing/skill remains 75%
        [Range(0f, 1f)] public float battingRatingWeight = 0.50f;
        [Range(0f, 1f)] public float formWeight = 0.25f;
        [Range(0f, 1f)] public float fitnessWeight = 0.15f;
        [Range(0f, 1f)] public float experienceWeight = 0.10f;

        [Header("Edge Mechanics")]
        public float edgeDeflectionMinAngle = 25f;
        public float edgeDeflectionMaxAngle = 65f;
        public float edgePowerDampener = 0.40f;

        [Header("Swing Duration")]
        public float swingDuration = 0.35f;
        public float backliftDuration = 0.15f;
        public float followThroughDuration = 0.30f;
        public float recoveryDuration = 0.25f;

        public static BattingSettings CreateDefault()
        {
            return new BattingSettings();
        }
    }
}
