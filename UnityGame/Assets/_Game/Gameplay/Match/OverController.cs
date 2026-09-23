using System;
using System.Collections.Generic;
using UnityEngine;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.Gameplay.Match
{
    public class OverController
    {
        private int totalOversInMatch;
        private int maxBowlerQuota;
        private string previousBowlerId;
        private List<string> overSummaryBalls = new List<string>();

        public int LegalBallsThisOver { get; private set; }
        public bool IsOverComplete { get { return LegalBallsThisOver >= 6; } }

        public OverController(int totalOvers)
        {
            totalOversInMatch = totalOvers;
            maxBowlerQuota = MatchRules.CalculateBowlerQuota(totalOvers);
            previousBowlerId = string.Empty;
            LegalBallsThisOver = 0;
            overSummaryBalls.Clear();
        }

        public void StartNewOver(string currentBowlerId)
        {
            LegalBallsThisOver = 0;
            overSummaryBalls.Clear();
        }

        public void RecordDelivery(UnifiedDeliveryResult result)
        {
            if (result == null) return;

            if (result.isLegalBall)
            {
                LegalBallsThisOver++;
            }

            string symbol = "•";
            if (result.isWicket) symbol = "W";
            else if (!result.isLegalBall) symbol = "Ex";
            else if (result.runsBat > 0) symbol = result.runsBat.ToString();

            overSummaryBalls.Add(symbol);
        }

        public void CompleteOver(string completedBowlerId)
        {
            previousBowlerId = completedBowlerId;
            LegalBallsThisOver = 0;
            overSummaryBalls.Clear();
        }

        public bool CanBowlerBowlNext(BowlerFigures bowler)
        {
            return MatchRules.CanBowlerBowl(bowler, maxBowlerQuota, previousBowlerId);
        }

        public List<string> GetOverSummary()
        {
            return new List<string>(overSummaryBalls);
        }
    }
}
