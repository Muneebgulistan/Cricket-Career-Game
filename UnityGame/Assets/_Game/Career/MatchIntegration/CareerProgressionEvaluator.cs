using System;
using System.Collections.Generic;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Career;
using CricketGame.Career.Evaluation;
using CricketGame.Career.Objectives;
using CricketGame.Career.Progression;
using CricketGame.Tournaments;
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
            MatchResult matchRes = new MatchResult();
            matchRes.isPlayerVictory = isTeamVictory;
            matchRes.userPerformance = performance;
            ApplyPostMatchProgression(profile, performance, matchRes, null);
        }

        public static CareerPerformanceReport ApplyPostMatchProgression(
            CareerProfile profile,
            PlayerMatchPerformance performance,
            MatchResult matchResult,
            TournamentProgress tournament)
        {
            if (profile == null || performance == null)
            {
                return new CareerPerformanceReport();
            }

            bool isTeamVictory = (matchResult != null && matchResult.isPlayerVictory);

            // 1. Structured Performance Evaluation
            CareerPerformanceReport report = CareerPerformanceEvaluator.EvaluateMatch(performance, matchResult, tournament);

            // 2. Update Career All-Time Stats
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

            // 3. Update Tournament Record
            if (tournament != null)
            {
                tournament.UpdatePlayerPerformance(performance.runs, performance.wickets, isTeamVictory, report.isPlayerOfTheMatch);
            }

            // 4. Form adjustment based on performance and match result
            int formDelta = 0;
            if (report.overallMatchRating >= 8.0f) formDelta += 8;
            else if (report.overallMatchRating >= 6.5f) formDelta += 4;
            else if (report.overallMatchRating < 4.0f) formDelta -= 4;

            if (isTeamVictory) formDelta += 3;
            else formDelta -= 2;

            if (profile.player != null)
            {
                profile.player.form = Mathf.Clamp(profile.player.form + formDelta, 10, 100);
                profile.player.fitness = Mathf.Clamp(profile.player.fitness - 6, 20, 100);

                // Experience & skill points
                int xpGain = 15 + performance.runs + (performance.wickets * 12);
                if (report.isPlayerOfTheMatch) xpGain += 50;
                profile.player.experience += xpGain;
                profile.skillPoints += (xpGain / 25);
            }

            // 5. Update level matches and rating average
            profile.progression.matchesAtCurrentLevel++;
            int mCount = profile.progression.matchesAtCurrentLevel;
            float prevAvg = profile.progression.averageMatchRatingAtLevel;
            profile.progression.averageMatchRatingAtLevel = ((prevAvg * (mCount - 1)) + report.overallMatchRating) / mCount;

            // 6. Evaluate Career Objectives
            EvaluateObjectives(profile, performance, isTeamVictory, report.isPlayerOfTheMatch, tournament);

            // 7. Selection evaluation
            EvaluateSelectionStatus(profile);

            // 8. Progression requirements evaluation
            string failureReason;
            bool eligible = CareerProgressionService.IsEligibleForStagePromotion(profile, tournament, out failureReason);
            profile.progression.isEligibleForPromotion = eligible;
            profile.progression.levelProgressPercent = CareerProgressionService.CalculateStageProgressPercent(profile, tournament);

            if (matchResult != null)
            {
                profile.recentMatchResultSummary = string.Format("{0} • Rating {1:F1} ({2})", 
                    isTeamVictory ? "Victory" : "Defeat", 
                    report.overallMatchRating, 
                    report.performanceGrade);
            }

            return report;
        }

        private static void EvaluateObjectives(
            CareerProfile profile, 
            PlayerMatchPerformance performance, 
            bool isTeamVictory, 
            bool isPotm, 
            TournamentProgress tournament)
        {
            if (profile == null || profile.activeObjectives == null) return;

            for (int i = profile.activeObjectives.Count - 1; i >= 0; i--)
            {
                var obj = profile.activeObjectives[i];
                if (obj == null || !obj.IsActive) continue;

                switch (obj.type)
                {
                    case ObjectiveType.ScoreFifty:
                        if (performance.runs >= 50) obj.SetProgress(50f);
                        else obj.SetProgress(performance.runs);
                        break;

                    case ObjectiveType.ScoreHundred:
                        if (performance.runs >= 100) obj.SetProgress(100f);
                        else obj.SetProgress(performance.runs);
                        break;

                    case ObjectiveType.ScoreRuns:
                        obj.AddProgress(performance.runs);
                        break;

                    case ObjectiveType.TakeThreeWickets:
                        if (performance.wickets >= 3) obj.SetProgress(3f);
                        else obj.SetProgress(performance.wickets);
                        break;

                    case ObjectiveType.TakeWickets:
                        obj.AddProgress(performance.wickets);
                        break;

                    case ObjectiveType.WinMatch:
                        if (isTeamVictory) obj.SetProgress(1f);
                        break;

                    case ObjectiveType.EarnPlayerOfMatch:
                        if (isPotm) obj.SetProgress(1f);
                        break;

                    case ObjectiveType.CompleteTournament:
                        if (tournament != null && tournament.isCompleted) obj.SetProgress(1f);
                        break;

                    case ObjectiveType.MaintainBattingAverage:
                        if (profile.statistics.allTimeBatting.Average >= obj.targetValue) obj.SetProgress(obj.targetValue);
                        break;

                    case ObjectiveType.MaintainBowlingEconomy:
                        if (profile.statistics.allTimeBowling.overs >= 4f && profile.statistics.allTimeBowling.Economy <= obj.targetValue)
                            obj.SetProgress(obj.targetValue);
                        break;
                }

                if (obj.IsCompleted)
                {
                    if (obj.reward != null)
                    {
                        obj.reward.Apply(profile);
                    }
                    if (profile.completedObjectives == null)
                    {
                        profile.completedObjectives = new List<CareerObjective>();
                    }
                    profile.completedObjectives.Add(obj);
                    profile.activeObjectives.RemoveAt(i);
                }
            }
        }

        public static void EvaluateSelectionStatus(CareerProfile profile)
        {
            if (profile == null || profile.player == null) return;

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
            string reason;
            bool eligible = CareerProgressionService.IsEligibleForStagePromotion(profile, null, out reason);
            profile.progression.isEligibleForPromotion = eligible;
            return eligible;
        }

        public static bool PromotePlayerToNextLevel(CareerProfile profile)
        {
            if (profile != null)
            {
                Career.Stages.CareerStage nextStage;
                return CareerProgressionService.TryPromoteCareer(profile, null, out nextStage);
            }
            return false;
        }
    }
}
