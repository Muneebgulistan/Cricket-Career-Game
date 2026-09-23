using System;

namespace CricketGame.Gameplay.Scoring
{
    [Serializable]
    public class TeamScore
    {
        public string teamId;
        public string teamName;
        public int totalScore;
        public int wicketsLost;
        public string oversCompleted;

        public TeamScore()
        {
            teamId = string.Empty;
            teamName = string.Empty;
            totalScore = 0;
            wicketsLost = 0;
            oversCompleted = "0.0";
        }

        public TeamScore(string id, string name)
        {
            teamId = id;
            teamName = name;
            totalScore = 0;
            wicketsLost = 0;
            oversCompleted = "0.0";
        }
    }
}
