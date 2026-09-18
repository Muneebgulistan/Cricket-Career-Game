using System;

namespace CricketGame.Cricket
{
    [Serializable]
    public class PlayerMatchPerformance
    {
        public string playerId;
        public string playerName;
        public int runs;
        public int balls;
        public int fours;
        public int sixes;
        public bool isOut;
        
        public float oversBowled;
        public int maidens;
        public int runsConceded;
        public int wickets;
        
        public int catches;
        public int runOuts;
        public int stumpings;

        public float matchRating; // 1.0 - 10.0
    }

    [Serializable]
    public class InningsSummary
    {
        public string battingTeamName;
        public string bowlingTeamName;
        public int totalRuns;
        public int wicketsLost;
        public float oversCompleted;
        public int extras;
    }

    [Serializable]
    public class MatchResult
    {
        public string matchId;
        public string homeTeamName;
        public string awayTeamName;
        public string winnerTeamName;
        public string resultDescription;
        public bool isPlayerVictory;
        public string manOfTheMatch;
        
        public InningsSummary firstInnings = new InningsSummary();
        public InningsSummary secondInnings = new InningsSummary();
        public PlayerMatchPerformance userPerformance = new PlayerMatchPerformance();
    }
}
