using System;
using UnityEngine;

namespace CricketGame.Gameplay.Fielding
{
    [Serializable]
    public class FieldingThrowData
    {
        public Vector3 origin;
        public FieldingTarget target;
        public Vector3 targetPosition;
        public Vector3 velocity;
        public float power;
        public float accuracy;
        public float travelTime;
        public bool isDirectHit;
        public float deviationAngle;

        public FieldingThrowData()
        {
            origin = Vector3.zero;
            target = FieldingTarget.WicketKeeper;
            targetPosition = Vector3.zero;
            velocity = Vector3.zero;
            power = 25f;
            accuracy = 0.85f;
            travelTime = 1.0f;
            isDirectHit = false;
            deviationAngle = 0f;
        }

        public FieldingThrowData(Vector3 origin, FieldingTarget target, Vector3 targetPos, Vector3 velocity, float power, float accuracy, float travelTime, bool isDirectHit, float deviationAngle)
        {
            this.origin = origin;
            this.target = target;
            this.targetPosition = targetPos;
            this.velocity = velocity;
            this.power = power;
            this.accuracy = accuracy;
            this.travelTime = travelTime;
            this.isDirectHit = isDirectHit;
            this.deviationAngle = deviationAngle;
        }
    }
}
