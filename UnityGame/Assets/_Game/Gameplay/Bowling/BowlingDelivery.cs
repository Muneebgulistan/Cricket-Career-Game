using System;
using UnityEngine;

namespace CricketGame.Gameplay.Bowling
{
    [Serializable]
    public class BowlingDelivery
    {
        public BowlingBaseType baseType;
        public DeliveryVariation variation;
        public BowlingLine targetLine;
        public BowlingLength targetLength;

        [Header("Parameters")]
        public float releaseSpeedKph;
        public float swingAmount;       // -1.0 (inswing to RHB) to +1.0 (outswing)
        public float seamAmount;        // Lateral deviation in degrees on seam
        public float spinAmount;        // Lateral turn in degrees on pitch
        public float bounceMultiplier;  // Restitution scalar
        public float releaseHeight;     // Release vertical height in meters
        public float releaseAngle;      // Release vertical pitch angle
        public float accuracy;          // 0.0 to 1.0 (1.0 = perfect spot)

        public BowlingDelivery()
        {
            baseType = BowlingBaseType.Fast;
            variation = DeliveryVariation.GoodLength;
            targetLine = BowlingLine.OffStump;
            targetLength = BowlingLength.GoodLength;
            releaseSpeedKph = 140f;
            swingAmount = 0f;
            seamAmount = 0f;
            spinAmount = 0f;
            bounceMultiplier = 1.0f;
            releaseHeight = 2.1f;
            releaseAngle = 0f;
            accuracy = 0.85f;
        }

        public static BowlingDelivery CreateDefault(BowlingBaseType type, BowlingLength length, BowlingLine line)
        {
            BowlingDelivery delivery = new BowlingDelivery();
            delivery.baseType = type;
            delivery.targetLength = length;
            delivery.targetLine = line;

            switch (type)
            {
                case BowlingBaseType.Fast:
                    delivery.releaseSpeedKph = 142f;
                    delivery.swingAmount = 0f;
                    delivery.seamAmount = 0.5f;
                    delivery.spinAmount = 0f;
                    delivery.bounceMultiplier = 1.05f;
                    break;

                case BowlingBaseType.Medium:
                    delivery.releaseSpeedKph = 125f;
                    delivery.swingAmount = 0.35f;
                    delivery.seamAmount = 1.2f;
                    delivery.spinAmount = 0f;
                    delivery.bounceMultiplier = 0.95f;
                    break;

                case BowlingBaseType.OffSpin:
                    delivery.releaseSpeedKph = 90f;
                    delivery.swingAmount = -0.1f; // Drift into right-hander
                    delivery.seamAmount = 0f;
                    delivery.spinAmount = -4.5f;  // Turns into right-hander (towards leg)
                    delivery.bounceMultiplier = 0.90f;
                    break;

                case BowlingBaseType.LegSpin:
                    delivery.releaseSpeedKph = 86f;
                    delivery.swingAmount = 0.1f;  // Drift away from right-hander
                    delivery.seamAmount = 0f;
                    delivery.spinAmount = 5.2f;   // Turns away from right-hander (towards off)
                    delivery.bounceMultiplier = 1.0f;
                    break;
            }

            // Adjust parameters based on length
            switch (length)
            {
                case BowlingLength.Yorker:
                    delivery.variation = DeliveryVariation.Yorker;
                    delivery.bounceMultiplier *= 0.8f;
                    break;
                case BowlingLength.Full:
                    delivery.variation = DeliveryVariation.Full;
                    break;
                case BowlingLength.GoodLength:
                    delivery.variation = DeliveryVariation.GoodLength;
                    break;
                case BowlingLength.Short:
                    delivery.variation = DeliveryVariation.Short;
                    delivery.bounceMultiplier *= 1.15f;
                    break;
                case BowlingLength.Bouncer:
                    delivery.variation = DeliveryVariation.Bouncer;
                    delivery.bounceMultiplier *= 1.30f;
                    delivery.releaseSpeedKph += 3f;
                    break;
            }

            return delivery;
        }
    }
}
