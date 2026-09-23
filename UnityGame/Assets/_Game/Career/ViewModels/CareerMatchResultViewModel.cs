using System;
using CricketGame.Cricket;
using CricketGame.Tournaments;
using CricketGame.Career.Stages;
using CricketGame.Career.MatchIntegration;

namespace CricketGame.Career.ViewModels
{
    [Serializable]
    public class CareerMatchResultViewModel
    {
        // 1. Match Result
        public string homeTeamName;
        public string awayTeamName;
        public string winnerTeamName;
        public string resultText;
        public bool isPlayerVictory;

        // 2. Player Performance
        public int runs;
        public int balls;
        public float strikeRate;
        public int wickets;
        public float oversBowled;
        public int runsConceded;
        public float economy;
        public int catches;
        public int runOuts;
        public float matchRating;
        public string performanceGrade;
        public bool isPlayerOfTheMatch;

        // 3. Career Delta
        public int careerRunsBefore;
        public int careerRunsAfter;
        public int careerWicketsBefore;
        public int careerWicketsAfter;
        public int skillPointsGained;
        public float reputationDelta;
        public string completedObjectivesText;

        // 4. Tournament Delta
        public int tournamentPositionBefore;
        public int tournamentPositionAfter;
        public int tournamentMatches;
        public int tournamentWins;
        public int tournamentLosses;
        public int tournamentRound;
        public string qualificationStatus;

        // 5. Progression
        public bool isPromoted;
        public string previousStage;
        public string newStage;
        public string unlockedOpportunity;

        public CareerMatchResultViewModel()
        {
            homeTeamName = "Team A";
            awayTeamName = "Team B";
            winnerTeamName = "Team A";
            resultText = "Match Completed";
            performanceGrade = "B";
            qualificationStatus = "In Contention";
            completedObjectivesText = "None";
            previousStage = "Under-16";
            newStage = "Under-19";
            unlockedOpportunity = "None";
        }

        public static CareerMatchResultViewModel FromContext(
            CareerMatchContext context, 
            CareerProfile profile, 
            TournamentProgress tournament)
        {
            CareerMatchResultViewModel vm = new CareerMatchResultViewModel();
            if (context == null && profile == null) return vm;

            // 1. Match Result
            if (context != null && context.lastMatchResult != null)
            {
                var res = context.lastMatchResult;
                vm.homeTeamName = res.homeTeamName;
                vm.awayTeamName = res.awayTeamName;
                vm.winnerTeamName = res.winnerTeamName;
                vm.isPlayerVictory = res.isPlayerVictory;
                vm.resultText = res.isPlayerVictory ? "VICTORY!" : "DEFEAT";
            }

            // 2. Player Performance
            if (context != null && context.lastPerformanceReport != null)
            {
                var rep = context.lastPerformanceReport;
                vm.matchRating = rep.overallMatchRating;
                vm.performanceGrade = rep.performanceGrade;
                vm.isPlayerOfTheMatch = rep.isPlayerOfTheMatch;
            }

            if (context != null && context.lastMatchResult != null && context.lastMatchResult.userPerformance != null)
            {
                var perf = context.lastMatchResult.userPerformance;
                vm.runs = perf.runs;
                vm.balls = perf.balls;
                vm.strikeRate = perf.balls > 0 ? ((float)perf.runs / perf.balls) * 100f : 0f;
                vm.wickets = perf.wickets;
                vm.oversBowled = perf.oversBowled;
                vm.runsConceded = perf.runsConceded;
                vm.economy = perf.oversBowled > 0 ? perf.runsConceded / perf.oversBowled : 0f;
                vm.catches = perf.catches;
                vm.runOuts = perf.runOuts;
            }

            // 3. Career Delta
            if (context != null)
            {
                vm.careerRunsBefore = context.preMatchCareerRuns;
                vm.careerWicketsBefore = context.preMatchCareerWickets;
                vm.tournamentPositionBefore = context.preMatchTournamentPosition;
            }

            if (profile != null)
            {
                if (profile.statistics != null && profile.statistics.allTimeBatting != null)
                {
                    vm.careerRunsAfter = profile.statistics.allTimeBatting.runs;
                }
                if (profile.statistics != null && profile.statistics.allTimeBowling != null)
                {
                    vm.careerWicketsAfter = profile.statistics.allTimeBowling.wickets;
                }

                if (context != null)
                {
                    vm.skillPointsGained = Math.Max(0, profile.skillPoints - context.preMatchSkillPoints);
                    vm.reputationDelta = profile.reputation - context.preMatchReputation;
                }

                if (profile.completedObjectives != null && profile.completedObjectives.Count > 0)
                {
                    vm.completedObjectivesText = string.Format("{0} objective(s) completed", profile.completedObjectives.Count);
                }
                else
                {
                    vm.completedObjectivesText = "Keep striving towards milestones";
                }
            }

            // 4. Tournament Delta
            if (tournament != null)
            {
                vm.tournamentPositionAfter = tournament.tournamentPosition;
                vm.tournamentMatches = tournament.matchesPlayed;
                vm.tournamentWins = tournament.matchesWon;
                vm.tournamentLosses = tournament.matchesLost;
                vm.tournamentRound = tournament.currentRound;
                vm.qualificationStatus = tournament.qualificationStatus.ToString();
            }

            // 5. Stage Promotion
            if (profile != null && profile.progression != null)
            {
                CareerStage current = profile.progression.currentStage;
                if (current > CareerStage.UNDER_16)
                {
                    vm.isPromoted = true;
                    vm.newStage = CareerProgression.GetLevelDisplayName(CareerStageDefinition.ToCareerLevel(current));
                    vm.previousStage = "Under-16 Cup";
                    vm.unlockedOpportunity = "Eligible for Higher Tier Tournaments";
                }
            }

            return vm;
        }
    }
}
