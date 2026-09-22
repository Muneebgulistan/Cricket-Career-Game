using System;
using UnityEngine;

namespace CricketGame.Gameplay.Bowling
{
    [Serializable]
    public class BowlingReleaseData
    {
        public Vector3 releasePosition;
        public Vector3 initialVelocity;
        public float speedKph;
        public float swingAcceleration;   // Lateral m/s^2 while in flight
        public float seamDeviationAngle;  // Degrees lateral deviation on pitch contact
        public float spinTurnAngle;       // Degrees lateral turn on pitch contact
        public float bounceMultiplier;    // Multiplier on pitch restitution
        public Vector3 intendedPitchPoint;
        public string deliveryName;

        public BowlingReleaseData()
        {
            releasePosition = new Vector3(0f, 2.1f, -10.5f);
            initialVelocity = Vector3.forward * 37.5f; // ~135 km/h
            speedKph = 135f;
            swingAcceleration = 0f;
            seamDeviationAngle = 0f;
            spinTurnAngle = 0f;
            bounceMultiplier = 1.0f;
            intendedPitchPoint = new Vector3(0.1f, 0.02f, 6.5f);
            deliveryName = "Standard Delivery";
        }
    }
}
