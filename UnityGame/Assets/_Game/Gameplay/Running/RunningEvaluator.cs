using System;
using UnityEngine;
using CricketGame.Players;

namespace CricketGame.Gameplay.Running
{
    public static class RunningEvaluator
    {
        public static float CalculateRunnerSpeed(PlayerAttributes attributes, RunningSettings settings, int completedRunsThisDelivery)
        {
            if (settings == null) settings = new RunningSettings();
            if (attributes == null)
            {
                return (settings.minRunSpeed + settings.maxRunSpeed) * 0.5f;
            }

            float weightedScore = (attributes.speed * 0.60f) + (attributes.fitness * 0.25f) + (attributes.agility * 0.15f);
            float normalized = Mathf.Clamp01(weightedScore / 100f);
            float baseSpeed = Mathf.Lerp(settings.minRunSpeed, settings.maxRunSpeed, normalized);

            // Apply fatigue penalty for consecutive runs
            float fatigue = 1.0f - (completedRunsThisDelivery * settings.fatiguePenaltyPerRun);
            fatigue = Mathf.Clamp(fatigue, 0.75f, 1.0f);

            return baseSpeed * fatigue;
        }

        public static float CalculateSingleRunDuration(float runnerSpeed)
        {
            if (runnerSpeed <= 0.1f) return 4.0f;
            return RunningTarget.CreaseDistance / runnerSpeed;
        }

        public static bool IsSafeInCrease(Vector3 runnerPosition, CreaseEnd end, bool isDiving, RunningSettings settings)
        {
            float diveBonus = (isDiving && settings != null) ? settings.diveDistance : 0f;
            float targetZ = end == CreaseEnd.StrikerEnd ? RunningTarget.StrikerCreaseZ : RunningTarget.NonStrikerCreaseZ;

            if (end == CreaseEnd.StrikerEnd)
            {
                return runnerPosition.z >= (targetZ - RunningTarget.CreaseSafetyThreshold - diveBonus);
            }
            else
            {
                return runnerPosition.z <= (targetZ + RunningTarget.CreaseSafetyThreshold + diveBonus);
            }
        }

        public static RunningResult EvaluateRunOut(
            RunnerRole runner, 
            Vector3 runnerPosition, 
            CreaseEnd end, 
            float runnerArrivalSeconds, 
            float ballArrivalSeconds, 
            int runsCompletedSoFar)
        {
            float timeMargin = ballArrivalSeconds - runnerArrivalSeconds;
            // Negative time margin means ball arrived BEFORE runner -> OUT
            if (timeMargin < -0.05f)
            {
                float distanceMargin = timeMargin * 6.0f; // Approx distance runner was short
                return RunningResult.RunOutOccurred(runsCompletedSoFar, runner, end, distanceMargin);
            }
            else
            {
                float distanceMargin = timeMargin * 6.0f;
                return RunningResult.Success(runsCompletedSoFar, distanceMargin);
            }
        }

        public static float EstimateRunRisk(float fielderDistToBall, float fielderDistToStumps, float runnerRemainingDist, float runnerSpeed)
        {
            if (runnerSpeed <= 0.1f) runnerSpeed = 6.0f;
            float runnerETA = runnerRemainingDist / runnerSpeed;

            // Fielder pickup + throw time
            float fielderPickupTime = Mathf.Max(0.4f, fielderDistToBall / 6.0f);
            float throwFlightTime = fielderDistToStumps / 22.0f; // Approx 22 m/s throw speed
            float ballETA = fielderPickupTime + throwFlightTime;

            float margin = ballETA - runnerETA;
            // Margin > 1.0s: very safe, < 0.2s: high risk
            float risk = 1.0f - Mathf.Clamp01(margin / 1.5f);
            return risk;
        }
    }
}
