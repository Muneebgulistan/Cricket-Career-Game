using System;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Players;
using CricketGame.Career;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Gameplay.Running;
using CricketGame.Gameplay.Match;

namespace CricketGame.Career.MatchIntegration
{
    public class CareerMatchBridge
    {
        public static void InjectPlayerAttributes(
            PlayerProfile profile, 
            BattingSettings battingSettings, 
            BowlingSettings bowlingSettings, 
            RunningSettings runningSettings)
        {
            if (profile == null) return;

            if (battingSettings != null)
            {
                float timingFactor = Mathf.Clamp(profile.battingRating / 100f, 0.4f, 1.2f);
                battingSettings.perfectTimingWindow = Mathf.Lerp(0.03f, 0.07f, timingFactor);
                battingSettings.goodTimingWindow = Mathf.Lerp(0.08f, 0.16f, timingFactor);
                battingSettings.sweetSpotRadius = Mathf.Lerp(0.06f, 0.12f, timingFactor);
            }

            if (bowlingSettings != null)
            {
                float speedFactor = Mathf.Clamp(profile.bowlingRating / 100f, 0.5f, 1.2f);
                bowlingSettings.fastMaxSpeed = Mathf.Lerp(130f, 160f, speedFactor);
                bowlingSettings.mediumMaxSpeed = Mathf.Lerp(110f, 138f, speedFactor);
                bowlingSettings.spinMaxSpeed = Mathf.Lerp(80f, 105f, speedFactor);
            }

            if (runningSettings != null)
            {
                runningSettings.minRunSpeed = 5.0f + (profile.fitness * 0.015f);
                runningSettings.maxRunSpeed = 6.8f + (profile.fitness * 0.02f);
            }
        }

        public static void InjectPlayerAttributes(
            PlayerAttributes attributes, 
            BattingSettings battingSettings, 
            BowlingSettings bowlingSettings, 
            RunningSettings runningSettings)
        {
            if (attributes == null) return;

            if (battingSettings != null)
            {
                float timingFactor = Mathf.Clamp(attributes.battingRating / 100f, 0.4f, 1.2f);
                battingSettings.perfectTimingWindow = Mathf.Lerp(0.03f, 0.07f, timingFactor);
                battingSettings.goodTimingWindow = Mathf.Lerp(0.08f, 0.16f, timingFactor);
                battingSettings.sweetSpotRadius = Mathf.Lerp(0.06f, 0.12f, timingFactor);
            }

            if (bowlingSettings != null)
            {
                float speedFactor = Mathf.Clamp(attributes.bowlingRating / 100f, 0.5f, 1.2f);
                bowlingSettings.fastMaxSpeed = Mathf.Lerp(130f, 160f, speedFactor);
                bowlingSettings.mediumMaxSpeed = Mathf.Lerp(110f, 138f, speedFactor);
                bowlingSettings.spinMaxSpeed = Mathf.Lerp(80f, 105f, speedFactor);
            }

            if (runningSettings != null)
            {
                runningSettings.minRunSpeed = 5.0f + (attributes.speed * 0.015f);
                runningSettings.maxRunSpeed = 6.8f + (attributes.speed * 0.02f);
            }
        }

        public static void ConfigureMatchForCareer(
            CareerProfile profile, 
            CareerMatchData matchData, 
            MatchController matchController)
        {
            if (profile == null || matchData == null || matchController == null) return;

            string homeTeam = profile.player.currentTeam;
            string awayTeam = matchData.opponentTeamName;

            matchController.SetupMatch(homeTeam, awayTeam);
        }
    }
}
