using UnityEngine;

namespace CricketGame.Utilities
{
    public static class CricketMath
    {
        public static float CalculateRequiredRunRate(int targetRuns, int currentRuns, int ballsRemaining)
        {
            int runsNeeded = targetRuns - currentRuns;
            if (runsNeeded <= 0) return 0f;
            if (ballsRemaining <= 0) return 99.99f;
            return (runsNeeded / (float)ballsRemaining) * 6f;
        }

        public static float CalculateCurrentRunRate(int runs, float overs)
        {
            if (overs <= 0f) return 0f;
            return runs / overs;
        }

        public static float FormatOvers(int legalDeliveries)
        {
            int completedOvers = legalDeliveries / 6;
            int ballsInOver = legalDeliveries % 6;
            return completedOvers + (ballsInOver * 0.1f);
        }
    }
}
