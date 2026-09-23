using System;
using UnityEngine;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.Gameplay.Match
{
    public static class MatchRules
    {
        public static int CalculateBowlerQuota(int totalOvers)
        {
            if (totalOvers <= 2) return 1;
            if (totalOvers <= 5) return 2;
            return Mathf.Max(1, totalOvers / 5);
        }

        public static bool CanBowlerBowl(BowlerFigures bowler, int maxQuota, string lastBowlerId)
        {
            if (bowler == null) return false;
            // A bowler cannot bowl consecutive overs
            if (!string.IsNullOrEmpty(lastBowlerId) && bowler.playerId == lastBowlerId)
            {
                return false;
            }
            // Check quota
            return bowler.CompletedOvers < maxQuota;
        }

        public static bool IsPowerplayActive(int legalBalls, int powerplayOvers)
        {
            return legalBalls < (powerplayOvers * 6);
        }

        public static bool HasInningsEnded(InningsScore innings, int maxOvers, int targetScore)
        {
            if (innings == null) return true;
            if (innings.wickets >= 10) return true;
            if (innings.legalBalls >= maxOvers * 6) return true;
            if (targetScore > 0 && innings.totalRuns >= targetScore) return true;
            return false;
        }
    }
}
