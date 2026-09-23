using System;

namespace CricketGame.Gameplay.Match
{
    [Serializable]
    public class MatchRuntimeData
    {
        public string tossWinnerTeam;
        public TossDecision tossDecision;
        public string activeBattingTeam;
        public string activeBowlingTeam;
        public int currentInningsNumber; // 1 or 2
        public string strikerName;
        public string strikerId;
        public string nonStrikerName;
        public string nonStrikerId;
        public string currentBowlerName;
        public string currentBowlerId;
        public string lastBowlerId;
        public int targetScore;
        public bool isFreeHit;
        public bool isMatchPaused;

        public MatchRuntimeData()
        {
            Reset();
        }

        public void Reset()
        {
            tossWinnerTeam = string.Empty;
            tossDecision = TossDecision.Bat;
            activeBattingTeam = string.Empty;
            activeBowlingTeam = string.Empty;
            currentInningsNumber = 1;
            strikerName = string.Empty;
            strikerId = string.Empty;
            nonStrikerName = string.Empty;
            nonStrikerId = string.Empty;
            currentBowlerName = string.Empty;
            currentBowlerId = string.Empty;
            lastBowlerId = string.Empty;
            targetScore = 0;
            isFreeHit = false;
            isMatchPaused = false;
        }
    }
}
