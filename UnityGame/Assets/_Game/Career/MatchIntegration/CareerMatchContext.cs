using System;
using CricketGame.Cricket;
using CricketGame.Players;
using CricketGame.Tournaments;
using CricketGame.Career.Evaluation;

namespace CricketGame.Career.MatchIntegration
{
    [Serializable]
    public class CareerMatchContext
    {
        public string careerId;
        public string tournamentId;
        public string tournamentName;
        public TournamentFixture fixture;
        public string homeTeamName;
        public string awayTeamName;
        public string playerTeamName;
        public string opponentTeamName;
        public MatchFormat format;
        public int totalOvers;
        public string venue;
        public PlayerProfile playerProfile;

        // Baseline snapshots before match for delta reporting
        public int preMatchCareerRuns;
        public int preMatchCareerWickets;
        public int preMatchTournamentPosition;
        public int preMatchSkillPoints;
        public float preMatchReputation;

        // Post-match outputs
        public MatchResult lastMatchResult;
        public CareerPerformanceReport lastPerformanceReport;
        public bool isMatchInProgress;
        public bool isResultProcessed;

        public CareerMatchContext()
        {
            careerId = string.Empty;
            tournamentId = string.Empty;
            tournamentName = string.Empty;
            format = MatchFormat.T20;
            totalOvers = 20;
            venue = "National Stadium";
            isMatchInProgress = false;
            isResultProcessed = false;
        }
    }
}
