using System;
using CricketGame.Career.Stages;
using CricketGame.Career.Objectives;
using CricketGame.Tournaments;

namespace CricketGame.Career.ViewModels
{
    [Serializable]
    public class CareerHubViewModel
    {
        public string playerName;
        public int playerAge;
        public string currentTeam;
        public string position;              // e.g. "Batsman", "AllRounder"
        public int overallRating;
        public string careerStage;           // e.g. "Under-16 Championship"
        public string currentTournament;     // e.g. "National Under-16 Championship"
        public string nextObjective;         // e.g. "Score 50 Runs (32/50)"
        public string recentResult;          // e.g. "Won by 24 runs vs Karachi Kings"
        public float formPercent;
        public float fitnessPercent;
        public float reputation;
        public int skillPoints;

        public CareerHubViewModel()
        {
            playerName = string.Empty;
            playerAge = 16;
            currentTeam = string.Empty;
            position = "AllRounder";
            overallRating = 50;
            careerStage = "Under-16 Championship";
            currentTournament = "U16 National Cup";
            nextObjective = "None";
            recentResult = "No recent matches";
            formPercent = 75f;
            fitnessPercent = 100f;
            reputation = 50f;
            skillPoints = 0;
        }

        public static CareerHubViewModel FromCareerProfile(
            CareerProfile profile, 
            TournamentProgress tournament, 
            string recentMatchSummary)
        {
            CareerHubViewModel vm = new CareerHubViewModel();
            if (profile == null) return vm;

            if (profile.player != null)
            {
                vm.playerName = profile.player.name;
                vm.playerAge = profile.player.age;
                vm.currentTeam = profile.player.currentTeam;
                vm.position = profile.player.playingRole.ToString();
                vm.overallRating = profile.player.overallRating;
                vm.formPercent = profile.player.form;
                vm.fitnessPercent = profile.player.fitness;
            }

            vm.reputation = profile.reputation;
            vm.skillPoints = profile.skillPoints;

            CareerStage stage = CareerStageDefinition.FromCareerLevel(profile.progression.currentLevel);
            vm.careerStage = CareerProgression.GetLevelDisplayName(profile.progression.currentLevel);

            if (tournament != null)
            {
                vm.currentTournament = tournament.tournamentName;
            }

            if (profile.activeObjectives != null && profile.activeObjectives.Count > 0)
            {
                var obj = profile.activeObjectives[0];
                vm.nextObjective = string.Format("{0} ({1:F0}/{2:F0})", obj.title, obj.currentValue, obj.targetValue);
            }
            else
            {
                vm.nextObjective = "Complete upcoming match fixture";
            }

            vm.recentResult = !string.IsNullOrEmpty(recentMatchSummary) ? recentMatchSummary : "No recent match recorded";

            return vm;
        }
    }
}
