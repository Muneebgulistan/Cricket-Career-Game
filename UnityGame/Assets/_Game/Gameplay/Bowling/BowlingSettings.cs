using System;
using UnityEngine;

namespace CricketGame.Gameplay.Bowling
{
    [Serializable]
    public class BowlingSettings
    {
        [Header("Pace Boundaries (km/h)")]
        public float fastMinSpeed = 135f;
        public float fastMaxSpeed = 155f;
        public float mediumMinSpeed = 115f;
        public float mediumMaxSpeed = 135f;
        public float spinMinSpeed = 80f;
        public float spinMaxSpeed = 100f;

        [Header("Target Pitch Length Coordinates (Z meters)")]
        public float yorkerZ = 8.8f;
        public float fullZ = 7.5f;
        public float goodLengthZ = 6.0f;
        public float shortZ = 3.5f;
        public float bouncerZ = 2.2f;

        [Header("Target Pitch Line Coordinates (X meters)")]
        public float outsideOffX = 0.35f;
        public float offStumpX = 0.12f;
        public float middleStumpX = 0.00f;
        public float legStumpX = -0.12f;
        public float downLegX = -0.35f;

        [Header("Physics Influences")]
        public float maxSwingAcceleration = 4.0f; // m/s^2 lateral acceleration
        public float maxSeamDeviation = 3.5f;      // degrees
        public float maxSpinTurn = 7.5f;          // degrees

        [Header("Attribute Weights")]
        [Range(0f, 1f)] public float bowlingRatingWeight = 0.50f;
        [Range(0f, 1f)] public float fitnessWeight = 0.20f;
        [Range(0f, 1f)] public float formWeight = 0.20f;
        [Range(0f, 1f)] public float experienceWeight = 0.10f;

        [Header("Run-Up Timings (seconds)")]
        public float runUpDuration = 1.2f;
        public float deliveryStrideDuration = 0.35f;
        public float recoveryDuration = 0.8f;

        public static BowlingSettings CreateDefault()
        {
            return new BowlingSettings();
        }
    }
}
