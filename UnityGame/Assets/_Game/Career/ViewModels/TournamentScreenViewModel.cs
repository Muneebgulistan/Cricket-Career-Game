using System;
using CricketGame.Tournaments;
using CricketGame.Career.Stages;

namespace CricketGame.Career.ViewModels
{
    [Serializable]
    public class TournamentScreenViewModel
    {
        public string tournamentName;
        public string stage;
        public string team;
        public int matches;
        public int wins;
        public int losses;
        public int position;
        public int currentRound;
        public string qualificationStatus;
        public float progress;
        public int playerRuns;
        public int playerWickets;
        public float playerBattingAverage;
        public float playerBowlingEconomy;

        public TournamentScreenViewModel()
        {
            tournamentName = string.Empty;
            stage = string.Empty;
            team = string.Empty;
            matches = 0;
            wins = 0;
            losses = 0;
            position = 1;
            currentRound = 1;
            qualificationStatus = "In Contention";
            progress = 0f;
            playerRuns = 0;
            playerWickets = 0;
            playerBattingAverage = 0f;
            playerBowlingEconomy = 0f;
        }

        public static TournamentScreenViewModel FromTournament(
            TournamentProgress tournament, 
            CareerProfile profile)
        {
            TournamentScreenViewModel vm = new TournamentScreenViewModel();
            if (tournament == null) return vm;

            vm.tournamentName = tournament.tournamentName;
            vm.stage = CareerProgression.GetLevelDisplayName(tournament.level);
            vm.team = (profile != null && profile.player != null) ? profile.player.currentTeam : "My Team";

            vm.matches = tournament.matchesPlayed;
            vm.wins = tournament.matchesWon;
            vm.losses = tournament.matchesLost;
            vm.position = tournament.tournamentPosition;
            vm.currentRound = tournament.currentRound;
            vm.qualificationStatus = tournament.qualificationStatus.ToString();

            if (tournament.fixtures != null && tournament.fixtures.Count > 0)
            {
                vm.progress = ((float)tournament.currentFixtureIndex / tournament.fixtures.Count) * 100f;
            }
            else
            {
                vm.progress = tournament.isCompleted ? 100f : 0f;
            }

            vm.playerRuns = tournament.playerRunsScored;
            vm.playerWickets = tournament.playerWicketsTaken;
            vm.playerBattingAverage = tournament.playerBattingAverage;
            vm.playerBowlingEconomy = tournament.playerBowlingEconomy;

            return vm;
        }
    }
}
