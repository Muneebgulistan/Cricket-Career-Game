using System;
using UnityEngine;
using CricketGame.Players;

namespace CricketGame.Gameplay.Batting
{
    public static class BattingEvaluator
    {
        public static BattingResult EvaluateShot(
            BattingShot shot,
            TimingQuality timing,
            float timingDelta,
            ContactQuality contactQuality,
            float distanceFromSweetSpot,
            PlayerProfile profile,
            BattingSettings settings,
            Vector3 batsmanForward,
            float incomingBallSpeedKph)
        {
            BattingResult result = new BattingResult();
            result.shotType = shot.shotType;
            result.timing = timing;
            result.timingDelta = timingDelta;
            result.contactQuality = contactQuality;
            result.distanceFromSweetSpot = distanceFromSweetSpot;

            // Check miss
            if (timing == TimingQuality.Miss || contactQuality == ContactQuality.Miss)
            {
                result.isMiss = true;
                result.exitVelocity = Vector3.zero;
                result.exitSpeedKph = 0f;
                result.distanceEstimate = 0f;
                result.estimatedRuns = 0;
                result.direction = batsmanForward;
                return result;
            }

            result.isMiss = false;

            // Multipliers
            float timingMul = GetTimingMultiplier(timing);
            float contactMul = GetContactMultiplier(contactQuality);
            float attrMul = CalculateAttributeMultiplier(profile, settings);

            bool isEdge = (contactQuality == ContactQuality.Edge);
            result.isEdge = isEdge;

            // Exit speed calculation
            float calculatedSpeedKph = 0f;
            if (shot.shotType == BattingShotType.Defensive)
            {
                calculatedSpeedKph = settings.defensiveExitSpeedKph * timingMul * contactMul;
            }
            else
            {
                float baseSpeed = shot.basePower * 1.3f; // 100 power -> 130 km/h
                float ballRebound = incomingBallSpeedKph * 0.20f;
                calculatedSpeedKph = (baseSpeed + ballRebound) * timingMul * contactMul * attrMul;

                if (isEdge)
                {
                    calculatedSpeedKph *= settings.edgePowerDampener;
                }

                calculatedSpeedKph = Mathf.Clamp(calculatedSpeedKph, settings.minExitSpeedKph, settings.maxExitSpeedKph);
            }

            result.exitSpeedKph = calculatedSpeedKph;
            float exitSpeedMps = calculatedSpeedKph * (1000f / 3600f);

            // Direction calculation
            float finalAngle = shot.directionAngle;
            if (isEdge)
            {
                // Edge deflects toward slips/gully/third-man (typically off-side for right-hander)
                float edgeSign = (shot.directionAngle >= 0f) ? -1f : 1f;
                finalAngle += edgeSign * UnityEngine.Random.Range(settings.edgeDeflectionMinAngle, settings.edgeDeflectionMaxAngle);
            }
            else
            {
                // Slight timing deviation on direction: early pulls/drags more, late pushes more
                float timingAngleDeviation = timingDelta * 40f; // e.g. 0.1s early -> -4 deg
                finalAngle += timingAngleDeviation;
            }

            result.directionAngle = finalAngle;
            Quaternion rot = Quaternion.AngleAxis(finalAngle, Vector3.up);
            Vector3 horizontalDir = (rot * batsmanForward).normalized;
            result.direction = horizontalDir;

            // Launch angle calculation
            float launchAngle = shot.elevationAngle;
            if (isEdge)
            {
                launchAngle = UnityEngine.Random.Range(10f, 30f);
            }
            else if (timing == TimingQuality.Early && shot.isLofted)
            {
                launchAngle += 5f; // Spliced / spooned up in the air
            }
            else if (timing == TimingQuality.Late && !shot.isLofted)
            {
                launchAngle = Mathf.Max(2f, launchAngle - 3f); // Beaten into the ground
            }

            result.launchAngle = launchAngle;

            // 3D Exit velocity
            float rad = launchAngle * Mathf.Deg2Rad;
            float verticalSpeed = exitSpeedMps * Mathf.Sin(rad);
            float horizSpeed = exitSpeedMps * Mathf.Cos(rad);
            result.exitVelocity = new Vector3(horizontalDir.x * horizSpeed, verticalSpeed, horizontalDir.z * horizSpeed);

            // Distance estimate
            float dist = CalculateEstimatedDistance(exitSpeedMps, launchAngle, shot.shotType == BattingShotType.Defensive);
            result.distanceEstimate = dist;

            // Estimated runs
            result.estimatedRuns = CalculateEstimatedRuns(dist, launchAngle, shot.isLofted, isEdge);

            return result;
        }

        private static float GetTimingMultiplier(TimingQuality quality)
        {
            switch (quality)
            {
                case TimingQuality.Perfect: return 1.0f;
                case TimingQuality.Good: return 0.85f;
                case TimingQuality.Early: return 0.60f;
                case TimingQuality.Late: return 0.55f;
                default: return 0.0f;
            }
        }

        private static float GetContactMultiplier(ContactQuality quality)
        {
            switch (quality)
            {
                case ContactQuality.SweetSpot: return 1.0f;
                case ContactQuality.GoodContact: return 0.85f;
                case ContactQuality.Edge: return 0.45f;
                default: return 0.0f;
            }
        }

        public static float CalculateAttributeMultiplier(PlayerProfile profile, BattingSettings settings)
        {
            if (profile == null) return 1.0f;

            float normBatting = Mathf.Clamp01(profile.battingRating / 100f);
            float normForm = Mathf.Clamp01(profile.form / 100f);
            float normFitness = Mathf.Clamp01(profile.fitness / 100f);
            float normExp = Mathf.Clamp01(profile.experience / 100f);

            float combined = (normBatting * settings.battingRatingWeight)
                           + (normForm * settings.formWeight)
                           + (normFitness * settings.fitnessWeight)
                           + (normExp * settings.experienceWeight);

            // Normalized around 0.65 (average rating 65): results in [0.90, 1.10]
            float mult = 1.0f + (combined - 0.65f) * settings.attributeImpactWeight;
            return Mathf.Clamp(mult, 0.85f, 1.15f);
        }

        public static float CalculateEstimatedDistance(float exitSpeedMps, float launchAngleDeg, bool isDefensive)
        {
            if (isDefensive || exitSpeedMps < 2f)
            {
                return Mathf.Max(1f, exitSpeedMps * 0.8f);
            }

            float rad = launchAngleDeg * Mathf.Deg2Rad;
            float g = 9.81f;

            if (launchAngleDeg > 15f)
            {
                // Aerial trajectory
                float flightDist = (exitSpeedMps * exitSpeedMps * Mathf.Sin(2f * rad)) / g;
                // Add bounce/roll (15% of flight)
                return flightDist * 1.15f;
            }
            else
            {
                // Ground shot roll estimate
                return (exitSpeedMps * exitSpeedMps) / (2f * 0.35f * g); // friction coeff 0.35
            }
        }

        public static int CalculateEstimatedRuns(float distanceMeters, float launchAngleDeg, bool isLofted, bool isEdge)
        {
            if (distanceMeters < 5f)
            {
                return 0;
            }

            // Boundary is at 70 meters
            if (distanceMeters >= 70f)
            {
                return (isLofted && launchAngleDeg >= 22f) ? 6 : 4;
            }
            if (distanceMeters >= 55f)
            {
                return 4;
            }
            if (distanceMeters >= 35f)
            {
                return 3;
            }
            if (distanceMeters >= 20f)
            {
                return 2;
            }
            if (distanceMeters >= 8f)
            {
                return 1;
            }

            return 0;
        }
    }
}
