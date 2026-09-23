using System;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.Gameplay.Match
{
    public class InningsController
    {
        private int matchTotalOvers;

        public InningsController(int totalOvers)
        {
            matchTotalOvers = totalOvers;
        }

        public bool CheckInningsEnded(InningsScore innings)
        {
            if (innings == null) return true;
            return MatchRules.HasInningsEnded(innings, matchTotalOvers, innings.targetScore);
        }

        public int CalculateTargetScore(InningsScore firstInnings)
        {
            if (firstInnings == null) return 1;
            return firstInnings.totalRuns + 1;
        }

        public bool HasChasingTeamWon(InningsScore secondInnings)
        {
            if (secondInnings == null || secondInnings.targetScore <= 0) return false;
            return secondInnings.totalRuns >= secondInnings.targetScore;
        }
    }
}
