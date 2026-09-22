using System;
using UnityEngine;

namespace CricketGame.Gameplay.Bowling
{
    [Serializable]
    public class BowlingResult
    {
        public BowlingBaseType deliveryType;
        public DeliveryVariation variation;
        public float speedKph;
        public BowlingLine line;
        public BowlingLength length;
        public float swing;
        public float seam;
        public float spin;
        public Vector3 bouncePosition;
        public Vector3 targetPosition;
        public float accuracy;
        public bool wasValidDelivery;
        public bool hasBounced;
        public bool wasHitByBatsman;

        public BowlingResult()
        {
            deliveryType = BowlingBaseType.Fast;
            variation = DeliveryVariation.GoodLength;
            speedKph = 140f;
            line = BowlingLine.OffStump;
            length = BowlingLength.GoodLength;
            swing = 0f;
            seam = 0f;
            spin = 0f;
            bouncePosition = Vector3.zero;
            targetPosition = Vector3.zero;
            accuracy = 1.0f;
            wasValidDelivery = true;
            hasBounced = false;
            wasHitByBatsman = false;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}) | Speed: {2:F1} km/h | {3} / {4} | Acc: {5:P0} | Valid: {6}",
                deliveryType, variation, speedKph, length, line, accuracy, wasValidDelivery);
        }
    }
}
