using System;
using UnityEngine;
using CricketGame.Players;

namespace CricketGame.Gameplay.Bowling
{
    public static class BowlingEvaluator
    {
        public static Vector3 GetTargetPitchPoint(BowlingLine line, BowlingLength length, BowlingSettings settings)
        {
            float z = settings.goodLengthZ;
            switch (length)
            {
                case BowlingLength.Yorker: z = settings.yorkerZ; break;
                case BowlingLength.Full: z = settings.fullZ; break;
                case BowlingLength.GoodLength: z = settings.goodLengthZ; break;
                case BowlingLength.Short: z = settings.shortZ; break;
                case BowlingLength.Bouncer: z = settings.bouncerZ; break;
            }

            float x = settings.offStumpX;
            switch (line)
            {
                case BowlingLine.OutsideOff: x = settings.outsideOffX; break;
                case BowlingLine.OffStump: x = settings.offStumpX; break;
                case BowlingLine.MiddleStump: x = settings.middleStumpX; break;
                case BowlingLine.LegStump: x = settings.legStumpX; break;
                case BowlingLine.DownLeg: x = settings.downLegX; break;
            }

            return new Vector3(x, 0.02f, z);
        }

        public static float CalculateAttributeMultiplier(PlayerProfile profile, BowlingSettings settings)
        {
            if (profile == null) return 1.0f;

            float normBowling = Mathf.Clamp01(profile.bowlingRating / 100f);
            float normForm = Mathf.Clamp01(profile.form / 100f);
            float normFitness = Mathf.Clamp01(profile.fitness / 100f);
            float normExp = Mathf.Clamp01(profile.experience / 100f);

            float combined = (normBowling * settings.bowlingRatingWeight)
                           + (normFitness * settings.fitnessWeight)
                           + (normForm * settings.formWeight)
                           + (normExp * settings.experienceWeight);

            // Normalized around 0.65 (base average): produces [0.85, 1.15]
            float mult = 1.0f + (combined - 0.65f) * 0.35f;
            return Mathf.Clamp(mult, 0.85f, 1.15f);
        }

        public static BowlingReleaseData CalculateReleaseData(
            BowlingDelivery delivery,
            Vector3 releasePoint,
            PlayerProfile profile,
            BowlingSettings settings,
            float inputAccuracyMeter,
            int seed = 0)
        {
            BowlingReleaseData data = new BowlingReleaseData();
            data.releasePosition = releasePoint;
            data.deliveryName = string.Format("{0} {1}", delivery.baseType, delivery.variation);

            // 1. Determine base speed with attribute influence
            float attrMultiplier = CalculateAttributeMultiplier(profile, settings);
            float finalSpeedKph = delivery.releaseSpeedKph;

            if (delivery.baseType == BowlingBaseType.Fast || delivery.baseType == BowlingBaseType.Medium)
            {
                finalSpeedKph *= attrMultiplier;
            }

            // Clamp speed within boundaries
            switch (delivery.baseType)
            {
                case BowlingBaseType.Fast:
                    finalSpeedKph = Mathf.Clamp(finalSpeedKph, settings.fastMinSpeed, settings.fastMaxSpeed);
                    break;
                case BowlingBaseType.Medium:
                    finalSpeedKph = Mathf.Clamp(finalSpeedKph, settings.mediumMinSpeed, settings.mediumMaxSpeed);
                    break;
                case BowlingBaseType.OffSpin:
                case BowlingBaseType.LegSpin:
                    finalSpeedKph = Mathf.Clamp(finalSpeedKph, settings.spinMinSpeed, settings.spinMaxSpeed);
                    break;
            }
            data.speedKph = finalSpeedKph;
            float speedMps = finalSpeedKph * (1000f / 3600f);

            // 2. Intended pitch target
            Vector3 intendedTarget = GetTargetPitchPoint(delivery.targetLine, delivery.targetLength, settings);
            data.intendedPitchPoint = intendedTarget;

            // 3. Accuracy deviation (combining player rating & input meter)
            float bowlerSkillAccuracy = profile != null ? Mathf.Clamp01(profile.bowlingRating / 100f) : 0.70f;
            float combinedAccuracy = (bowlerSkillAccuracy * 0.4f) + (Mathf.Clamp01(inputAccuracyMeter) * 0.6f);

            // Random deviation scaled inversely with accuracy
            float maxDeviation = (1.0f - combinedAccuracy) * 0.40f; // at 1.0 acc: 0m, at 0.5 acc: 0.20m
            System.Random rng = (seed != 0) ? new System.Random(seed) : new System.Random();
            float devX = (float)(rng.NextDouble() * 2.0 - 1.0) * maxDeviation;
            float devZ = (float)(rng.NextDouble() * 2.0 - 1.0) * maxDeviation * 1.5f;

            Vector3 actualTarget = intendedTarget + new Vector3(devX, 0f, devZ);

            // 4. Calculate swing acceleration
            float swingAcc = delivery.swingAmount * settings.maxSwingAcceleration;
            data.swingAcceleration = swingAcc;

            // 5. Calculate 3D velocity to hit actualTarget from releasePoint
            float distZ = actualTarget.z - releasePoint.z;
            float vZ = speedMps * 0.98f; // Primary forward velocity
            float timeToPitch = Mathf.Max(0.1f, distZ / vZ);

            // Vertical velocity (gravity equation: y_target - y_0 = vY * t + 0.5 * g * t^2)
            float g = -9.81f;
            float deltaY = actualTarget.y - releasePoint.y;
            float vY = (deltaY - 0.5f * g * (timeToPitch * timeToPitch)) / timeToPitch;

            // Lateral velocity (accounting for airborne swing acceleration: x_target - x_0 = vX * t + 0.5 * a_swing * t^2)
            float deltaX = actualTarget.x - releasePoint.x;
            float vX = (deltaX - 0.5f * swingAcc * (timeToPitch * timeToPitch)) / timeToPitch;

            data.initialVelocity = new Vector3(vX, vY, vZ);

            // 6. Seam and Spin post-bounce effects
            data.seamDeviationAngle = delivery.seamAmount;
            data.spinTurnAngle = delivery.spinAmount;
            if (delivery.baseType == BowlingBaseType.OffSpin || delivery.baseType == BowlingBaseType.LegSpin)
            {
                // Spin turn scales with bowler rating
                data.spinTurnAngle *= (0.8f + (bowlerSkillAccuracy * 0.4f));
            }
            data.bounceMultiplier = delivery.bounceMultiplier;

            return data;
        }

        public static BowlingResult EvaluateResult(
            BowlingDelivery delivery,
            BowlingReleaseData releaseData,
            Vector3 actualBouncePos,
            bool wasHit,
            bool hasBounced)
        {
            BowlingResult result = new BowlingResult();
            result.deliveryType = delivery.baseType;
            result.variation = delivery.variation;
            result.speedKph = releaseData != null ? releaseData.speedKph : delivery.releaseSpeedKph;
            result.line = delivery.targetLine;
            result.length = delivery.targetLength;
            result.swing = releaseData != null ? releaseData.swingAcceleration : 0f;
            result.seam = releaseData != null ? releaseData.seamDeviationAngle : 0f;
            result.spin = releaseData != null ? releaseData.spinTurnAngle : 0f;
            result.bouncePosition = actualBouncePos;
            result.targetPosition = releaseData != null ? releaseData.intendedPitchPoint : Vector3.zero;
            result.hasBounced = hasBounced;
            result.wasHitByBatsman = wasHit;

            // Check if delivery was on the pitch (pitch width is 3.05m -> X in [-1.52, 1.52])
            bool isOnPitch = Mathf.Abs(actualBouncePos.x) <= 1.52f && actualBouncePos.z > 0f && actualBouncePos.z < 10.5f;
            result.wasValidDelivery = isOnPitch || !hasBounced;

            // Accuracy evaluation: distance between intended target and actual bounce spot
            if (releaseData != null && hasBounced)
            {
                float errorDist = Vector3.Distance(new Vector3(actualBouncePos.x, 0f, actualBouncePos.z),
                                                   new Vector3(releaseData.intendedPitchPoint.x, 0f, releaseData.intendedPitchPoint.z));
                result.accuracy = Mathf.Clamp01(1.0f - (errorDist / 1.5f));
            }
            else
            {
                result.accuracy = 1.0f;
            }

            return result;
        }
    }
}
