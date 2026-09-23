using System;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Career.Stages;
using CricketGame.Tournaments;

namespace CricketGame.Career.Progression
{
    public class CareerProgressionService
    {
        public static bool IsEligibleForStagePromotion(
            CareerProfile profile, 
            TournamentProgress tournament, 
            out string failureReason)
        {
            failureReason = string.Empty;
            if (profile == null)
            {
                failureReason = "No active career profile.";
                return false;
            }

            CareerStage currentStage = CareerStageDefinition.FromCareerLevel(profile.progression.currentLevel);
            if (currentStage == CareerStage.ODI_WORLD_CUP)
            {
                failureReason = "Already at highest career tier (ODI World Cup).";
                return false;
            }

            CareerStage nextStage = (CareerStage)((int)currentStage + 1);
            StageProgressionCriteria criteria = CareerProgressionRequirements.GetCriteriaForStage(nextStage);

            var batting = profile.statistics.allTimeBatting;
            var bowling = profile.statistics.allTimeBowling;

            // 1. Matches played
            int matches = profile.progression.matchesAtCurrentLevel;
            if (matches < criteria.minMatchesPlayed)
            {
                failureReason = string.Format("Requires {0} matches at current level (played {1}).", criteria.minMatchesPlayed, matches);
                return false;
            }

            // 2. Performance (Runs OR Wickets)
            bool runsMet = batting.runs >= criteria.minRuns;
            bool wktsMet = bowling.wickets >= criteria.minWickets;
            if (!runsMet && !wktsMet)
            {
                failureReason = string.Format("Requires {0} runs or {1} wickets (has {2} runs, {3} wkts).", criteria.minRuns, criteria.minWickets, batting.runs, bowling.wickets);
                return false;
            }

            // 3. Tournament completion
            if (criteria.requireTournamentCompletion)
            {
                bool tournamentDone = (tournament != null && tournament.isCompleted);
                if (!tournamentDone)
                {
                    failureReason = "Current stage tournament must be completed.";
                    return false;
                }
            }

            // 4. Rating / POTM
            if (criteria.minPlayerOfTheMatchAwards > 0 && tournament != null)
            {
                if (tournament.playerOfTheMatchCount < criteria.minPlayerOfTheMatchAwards)
                {
                    failureReason = string.Format("Requires at least {0} Player of the Match award(s).", criteria.minPlayerOfTheMatchAwards);
                    return false;
                }
            }

            return true;
        }

        public static int CalculateStageProgressPercent(CareerProfile profile, TournamentProgress tournament)
        {
            if (profile == null) return 0;

            CareerStage currentStage = CareerStageDefinition.FromCareerLevel(profile.progression.currentLevel);
            if (currentStage == CareerStage.ODI_WORLD_CUP) return 100;

            CareerStage nextStage = (CareerStage)((int)currentStage + 1);
            StageProgressionCriteria criteria = CareerProgressionRequirements.GetCriteriaForStage(nextStage);

            int matchPoints = criteria.minMatchesPlayed > 0 ? (profile.progression.matchesAtCurrentLevel * 40 / criteria.minMatchesPlayed) : 40;
            int runPoints = criteria.minRuns > 0 ? (profile.statistics.allTimeBatting.runs * 30 / criteria.minRuns) : 30;
            int wktPoints = criteria.minWickets > 0 ? (profile.statistics.allTimeBowling.wickets * 30 / criteria.minWickets) : 30;
            int perfPoints = Mathf.Max(runPoints, wktPoints);
            int tourPoints = (tournament != null && tournament.isCompleted) ? 30 : (tournament != null && tournament.matchesPlayed > 0 ? 15 : 0);

            int total = Mathf.Clamp(matchPoints + perfPoints + tourPoints, 0, 100);
            return total;
        }

        public static bool TryPromoteCareer(
            CareerProfile profile, 
            TournamentProgress tournament, 
            out CareerStage newStage)
        {
            string reason;
            newStage = CareerStageDefinition.FromCareerLevel(profile.progression.currentLevel);

            if (IsEligibleForStagePromotion(profile, tournament, out reason))
            {
                newStage = (CareerStage)((int)newStage + 1);
                profile.progression.currentLevel = CareerStageDefinition.ToCareerLevel(newStage);
                profile.progression.matchesAtCurrentLevel = 0;
                profile.progression.averageMatchRatingAtLevel = 0f;
                profile.progression.isEligibleForPromotion = false;
                profile.progression.levelProgressPercent = 0;

                // Stage reward bonus
                profile.skillPoints += 10;
                profile.player.experience += 100;
                profile.reputation = Mathf.Clamp(profile.reputation + 15f, 0f, 100f);

                return true;
            }

            return false;
        }
    }
}
