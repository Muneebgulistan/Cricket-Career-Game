using System;
using UnityEngine;
using CricketGame.Players;

namespace CricketGame.Gameplay.Fielding
{
    public static class FieldingEvaluator
    {
        // ----------------------------------------------------
        // 1. Fielder Candidate Scoring
        // ----------------------------------------------------
        public static float ScoreFielder(
            FieldingPosition position,
            Vector3 fielderPos,
            Vector3 ballPos,
            Vector3 ballVelocity,
            PlayerAttributes attributes,
            FieldingSettings settings)
        {
            if (settings == null) settings = FieldingSettings.CreateDefault();

            // Ball 2D direction in XZ plane
            Vector2 ballDir2D = new Vector2(ballVelocity.x, ballVelocity.z);
            float ballSpeed2D = ballDir2D.magnitude;
            if (ballSpeed2D > 0.01f) ballDir2D.Normalize();

            // Vector from pitch origin to fielder
            Vector2 fielderDir2D = new Vector2(fielderPos.x, fielderPos.z);
            float fielderDistFromOrigin = fielderDir2D.magnitude;
            if (fielderDistFromOrigin > 0.01f) fielderDir2D.Normalize();

            // Directional alignment: dot product between ball trajectory and fielder angle from pitch
            float alignment = Vector2.Dot(ballDir2D, fielderDir2D); // -1.0 to +1.0

            // Distance from fielder to current ball position
            float distToBall = Vector3.Distance(fielderPos, ballPos);

            // Fielder attribute ratings
            int fieldingRating = attributes != null ? attributes.fieldingRating : 50;
            int reactionRating = attributes != null ? attributes.reaction : fieldingRating;
            int speedRating = attributes != null ? attributes.speed : 50;

            // Reaction delay (0.10s elite to 0.45s amateur)
            float reactionTime = Mathf.Lerp(settings.reactionDelayMax, settings.reactionDelayMin, reactionRating / 100f);

            // Effective speed (m/s)
            float effectiveSpeed = Mathf.Lerp(settings.baseRunSpeed * 0.85f, settings.baseSprintSpeed * 1.15f, speedRating / 100f);

            // Score components:
            // High alignment is heavily weighted (ball is heading towards fielder's sector)
            float alignmentScore = Mathf.Clamp01((alignment + 1f) * 0.5f) * 50f;

            // Distance penalty: closer fielders score much higher
            float distanceScore = Mathf.Max(0f, 40f - (distToBall * 0.5f));

            // Attribute bonus
            float skillBonus = (fieldingRating / 100f) * 10f;

            // Reaction bonus: faster reaction yields higher priority
            float reactionBonus = (1.0f - (reactionTime / settings.reactionDelayMax)) * 10f;

            float totalScore = alignmentScore + distanceScore + skillBonus + reactionBonus;
            return Mathf.Max(0.1f, totalScore);
        }

        // ----------------------------------------------------
        // 2. Ball Interception Calculation
        // ----------------------------------------------------
        public static bool CalculateInterception(
            Vector3 fielderPos,
            float fielderSpeed,
            float reactionDelay,
            Vector3 ballPos,
            Vector3 ballVelocity,
            float gravity,
            out float arrivalTime,
            out Vector3 interceptPoint)
        {
            arrivalTime = 0f;
            interceptPoint = ballPos;

            float timeStep = 0.05f;
            float maxTime = 6.0f;

            Vector3 currentBallPos = ballPos;
            Vector3 currentBallVel = ballVelocity;

            for (float t = timeStep; t <= maxTime; t += timeStep)
            {
                // Ball ballistic integration
                currentBallVel.y += gravity * timeStep;
                currentBallPos += currentBallVel * timeStep;

                // Ground contact check
                if (currentBallPos.y <= 0.05f)
                {
                    currentBallPos.y = 0.05f;
                    currentBallVel.y = 0f;
                    currentBallVel.x *= 0.95f;
                    currentBallVel.z *= 0.95f;
                }

                // Time available for fielder after reaction delay
                float fielderTime = t - reactionDelay;
                if (fielderTime > 0f)
                {
                    float distanceFielderCanCover = fielderSpeed * fielderTime;
                    float distanceToBall = Vector3.Distance(fielderPos, currentBallPos);

                    if (distanceFielderCanCover >= distanceToBall)
                    {
                        arrivalTime = t;
                        interceptPoint = currentBallPos;
                        return true;
                    }
                }
            }

            // Fallback: estimate where ball stops rolling
            arrivalTime = maxTime;
            interceptPoint = currentBallPos;
            return false;
        }

        // ----------------------------------------------------
        // 3. Catch Evaluation
        // ----------------------------------------------------
        public static bool IsCatchFeasible(Vector3 ballPos, Vector3 ballVelocity, float interceptHeight, float maxHeight)
        {
            // Ball must be airborne and at catchable height
            if (interceptHeight < 0.25f || interceptHeight > maxHeight) return false;
            return true;
        }

        public static float CalculateCatchDifficulty(
            Vector3 fielderInitialPos,
            Vector3 interceptPoint,
            float ballSpeed,
            float arrivalTime)
        {
            float distanceRan = Vector3.Distance(fielderInitialPos, interceptPoint);
            
            // Higher distance ran, higher ball speed, and shorter arrival time increase difficulty
            float runFactor = Mathf.Clamp01(distanceRan / 25.0f) * 0.40f;
            float speedFactor = Mathf.Clamp01(ballSpeed / 35.0f) * 0.35f;
            float timeFactor = Mathf.Clamp01(1.0f - (arrivalTime / 3.5f)) * 0.25f;

            float difficulty = Mathf.Clamp01(runFactor + speedFactor + timeFactor);
            return difficulty;
        }

        public static FieldingOutcome EvaluateCatchSuccess(
            PlayerAttributes attributes,
            float difficulty,
            int seed = 42)
        {
            int catchingRating = attributes != null ? attributes.catching : 50;
            int reactionRating = attributes != null ? attributes.reaction : 50;

            // Catching skill threshold: higher catching rating increases success chance
            float skillFactor = (catchingRating * 0.65f + reactionRating * 0.35f) / 100f; // 0.30 to 0.99

            // Success chance: skill vs difficulty
            float successChance = Mathf.Clamp01(skillFactor - (difficulty * 0.45f) + 0.20f);

            // Deterministic roll using seed
            System.Random rng = new System.Random(seed);
            float roll = (float)rng.NextDouble();

            if (roll <= successChance)
            {
                return FieldingOutcome.Catch;
            }
            else
            {
                return FieldingOutcome.DroppedCatch;
            }
        }

        // ----------------------------------------------------
        // 4. Ground Pickup Evaluation
        // ----------------------------------------------------
        public static FieldingOutcome EvaluateGroundPickup(
            PlayerAttributes attributes,
            float ballSpeedKph,
            float approachAngle,
            int seed = 42)
        {
            int fieldingRating = attributes != null ? attributes.fieldingRating : 50;
            int groundRating = attributes != null ? attributes.groundFielding : fieldingRating;
            int agilityRating = attributes != null ? attributes.agility : fieldingRating;

            float skill = (groundRating * 0.6f + agilityRating * 0.4f) / 100f;

            // High speed balls or sharp approach angles increase fumble chance
            float speedPenalty = Mathf.Clamp01((ballSpeedKph - 40f) / 80f) * 0.20f;
            float anglePenalty = Mathf.Clamp01(Mathf.Abs(approachAngle) / 90f) * 0.15f;

            float successChance = Mathf.Clamp(skill - speedPenalty - anglePenalty + 0.25f, 0.40f, 0.98f);

            System.Random rng = new System.Random(seed);
            float roll = (float)rng.NextDouble();

            if (roll <= successChance)
            {
                return FieldingOutcome.CleanPickup;
            }
            else
            {
                return FieldingOutcome.Fumble;
            }
        }

        // ----------------------------------------------------
        // 5. Throwing Calculation
        // ----------------------------------------------------
        public static FieldingThrowData CalculateThrow(
            Vector3 origin,
            FieldingTarget target,
            Vector3 targetPosition,
            PlayerAttributes attributes,
            FieldingSettings settings,
            int seed = 42)
        {
            if (settings == null) settings = FieldingSettings.CreateDefault();

            int throwPowerRating = attributes != null ? attributes.throwPower : 50;
            int throwAccuracyRating = attributes != null ? attributes.throwAccuracy : 50;

            // Throw speed between min and max
            float throwSpeed = Mathf.Lerp(settings.throwSpeedMin, settings.throwSpeedMax, throwPowerRating / 100f);

            // Vector towards target
            Vector3 toTarget = targetPosition - origin;
            float distance = toTarget.magnitude;
            Vector3 idealDirection = distance > 0.01f ? toTarget.normalized : Vector3.forward;

            // Accuracy deviation: higher accuracy gives tighter angle
            float maxDeviation = Mathf.Lerp(settings.maxThrowDeviationAngle, 0.5f, throwAccuracyRating / 100f);

            System.Random rng = new System.Random(seed);
            float devX = ((float)rng.NextDouble() * 2f - 1f) * maxDeviation;
            float devY = ((float)rng.NextDouble() * 2f - 1f) * (maxDeviation * 0.4f);

            Quaternion deviationRot = Quaternion.Euler(devY, devX, 0f);
            Vector3 actualDirection = deviationRot * idealDirection;

            Vector3 throwVelocity = actualDirection * throwSpeed;
            float travelTime = distance > 0.01f ? distance / throwSpeed : 0.1f;

            // Accuracy metric (1.0 = direct on line)
            float accuracy = Mathf.Clamp01(1.0f - (Mathf.Abs(devX) / settings.maxThrowDeviationAngle));

            // Direct hit evaluation
            float missDistance = Mathf.Sin(Mathf.Abs(devX) * Mathf.Deg2Rad) * distance;
            bool isDirectHit = missDistance <= settings.directHitTolerance;

            return new FieldingThrowData(
                origin,
                target,
                targetPosition,
                throwVelocity,
                throwSpeed,
                accuracy,
                travelTime,
                isDirectHit,
                devX
            );
        }

        // ----------------------------------------------------
        // 6. Run-Out Evaluation
        // ----------------------------------------------------
        public static bool EvaluateRunOut(
            FieldingThrowData throwData,
            float batsmanDistanceToCrease,
            float batsmanRunSpeed)
        {
            if (throwData == null) return false;

            float ballArrivalTime = throwData.travelTime;
            float batsmanArrivalTime = batsmanDistanceToCrease / Mathf.Max(0.1f, batsmanRunSpeed);

            // Direct hit breaks stumps immediately; throw to keeper adds small collection buffer (0.15s)
            float wicketBreakTime = throwData.isDirectHit ? ballArrivalTime : ballArrivalTime + 0.15f;

            // Run out if ball arrives and breaks wicket before batsman crosses the crease line
            return wicketBreakTime < batsmanArrivalTime;
        }

        // ----------------------------------------------------
        // 7. Boundary Check
        // ----------------------------------------------------
        public static bool CheckBoundary(Vector3 ballPos, float boundaryRadius, bool hasBounced, out bool isFour, out bool isSix)
        {
            Vector2 pos2D = new Vector2(ballPos.x, ballPos.z);
            float distFromPitch = pos2D.magnitude;

            if (distFromPitch >= boundaryRadius)
            {
                if (hasBounced)
                {
                    isFour = true;
                    isSix = false;
                }
                else
                {
                    isFour = false;
                    isSix = true;
                }
                return true;
            }

            isFour = false;
            isSix = false;
            return false;
        }
    }
}
