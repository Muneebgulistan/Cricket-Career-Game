using System;
using UnityEngine;

namespace CricketGame.Gameplay.Fielding
{
    [Serializable]
    public class FieldingSettings
    {
        [Header("Movement Speeds (m/s)")]
        public float baseWalkSpeed = 2.5f;
        public float baseRunSpeed = 5.5f;
        public float baseSprintSpeed = 7.0f;
        public float rotationSpeed = 12.0f;

        [Header("Reaction Time (s)")]
        public float reactionDelayMin = 0.10f; // Elite reaction
        public float reactionDelayMax = 0.45f; // Club reaction

        [Header("Interception & Interaction (m)")]
        public float pickupRadius = 1.35f;
        public float catchReachRadius = 1.85f;
        public float catchMaxHeight = 2.6f;
        public float boundaryRadius = 70.0f;

        [Header("Throwing (m/s)")]
        public float throwSpeedMin = 20.0f;
        public float throwSpeedMax = 32.0f;
        public float maxThrowDeviationAngle = 7.5f;
        public float directHitTolerance = 0.35f;

        [Header("Running Between Wickets")]
        public float batsmanRunSpeed = 5.8f;
        public float pitchLength = 20.12f;
        public float creaseSeparation = 17.68f;

        public static FieldingSettings CreateDefault()
        {
            return new FieldingSettings();
        }
    }
}
