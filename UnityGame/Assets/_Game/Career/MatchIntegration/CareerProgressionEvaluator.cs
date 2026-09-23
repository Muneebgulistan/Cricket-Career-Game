using System;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Career;
using CricketGame.Players;

namespace CricketGame.Career.MatchIntegration
{
    public static class CareerProgressionEvaluator
    {
        public static void ApplyPostMatchProgression(
            CareerProfile profile, 
            PlayerMatchPerformance performance, 
            bool isTeamVictory)
        {
            if (profile == null || performance == null) return;

            // 1. Update Career Stats
            profile.statistics.RecordMatchBatting(
                performance.runs, 
                performance.balls, 
                performance.fours, 
                performance.sixes, 
                performance.isOut);

            profile.statistics.RecordMatchBowling(
                performance.oversBowled, 
                performance.maidens, 
                performance.runsConceded, 
                performance.wickets);

            profile.statistics.RecordMatchFielding(
                performance.catches, 
                performance.runOuts, 
                performance.stumpings);

            // 2. Form adjustment based on performance and match result
            int formDelta = 0;
            if (performance.matchRating >= 8.0f) formDelta += 8;
            else if (performance.matchRating >= 6.5f) formDelta += 4;
            else if (performance.matchRating < 4.0f) formDelta -= 4;

            if (isTeamVictory) formDelta += 3;
            else formDelta -= 2;

            profile.player.form = Mathf.Clamp(profile.player.form + formDelta, 10, 100);

            // 3. Fitness decrease (fatigue)
            profile.player.fitness = Mathf.Clamp(profile.player.fitness - 6, 20, 100);

            // 4. Experience & skill points
            int xpGain = 15 + performance.runs + (performance.wickets * 12);
            profile.player.experience += xpGain;
            profile.skillPoints += (xpGain / 25);

            // 5. Update level matches and rating average
            profile.progression.matchesAtCurrentLevel++;
            int mCount = profile.progression.matchesAtCurrentLevel;
            float prevAvg = profile.progression.averageMatchRatingAtLevel;
            profile.progression.averageMatchRatingAtLevel = ((prevAvg * (mCount - 1)) + performance.matchRating) / mCount;

            // 6. Selection evaluation
            EvaluateSelectionStatus(profile);

            // 7. Check level promotion eligibility
            CheckPromotionEligibility(profile);
        }

        public static void EvaluateSelectionStatus(CareerProfile profile)
        {
            if (profile == null) return;

            // Fails selection if form is critically low and poor match ratings
            if (profile.player.form < 30 && profile.progression.averageMatchRatingAtLevel < 4.0f)
            {
                profile.isSelectedInPlayingXI = false;
            }
            else
            {
                profile.isSelectedInPlayingXI = true;
            }
        }

        public static bool CheckPromotionEligibility(CareerProfile profile)
        {
            if (profile == null) return false;

            var prog = profile.progression;
            var stats = profile.statistics.allTimeBatting;
            var bowlStats = profile.statistics.allTimeBowling;

            // Requirements for Under-16 to Under-19 promotion:
            // At least 4 matches played at current level, 120+ runs or 6+ wickets, average rating >= 6.0
            bool matchesMet = prog.matchesAtCurrentLevel >= 4;
            bool performanceMet = (stats.runs >= 120) || (bowlStats.wickets >= 6);
            bool ratingMet = prog.averageMatchRatingAtLevel >= 6.0f;

            if (matchesMet && performanceMet && ratingMet)
            {
                prog.isEligibleForPromotion = true;
                prog.levelProgressPercent = 100;
                return true;
            }
            else
            {
                int progress = Mathf.Clamp((prog.matchesAtCurrentLevel * 20) + (stats.runs / 2), 0, 95);
                prog.levelProgressPercent = progress;
                return false;
            }
        }

        public static bool PromotePlayerToNextLevel(CareerProfile profile)
        {
            if (profile != null && profile.progression.CanPromoteToNextLevel())
            {
                profile.progression.Promote();
                profile.player.experience += 50;
                profile.skillPoints += 5;
                return true;
            }
            return false;
        }
    }
}
