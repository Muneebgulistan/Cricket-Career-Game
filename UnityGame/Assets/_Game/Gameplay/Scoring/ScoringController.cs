using System;
using UnityEngine;

namespace CricketGame.Gameplay.Scoring
{
    public class ScoringController
    {
        public void ApplyDeliveryResult(
            UnifiedDeliveryResult result, 
            InningsScore innings, 
            BatterScore striker, 
            BatterScore nonStriker, 
            BowlerFigures bowler, 
            out bool shouldRotateStrike, 
            out bool isOverComplete)
        {
            shouldRotateStrike = false;
            isOverComplete = false;

            if (result == null || innings == null) return;

            // 1. Update Innings totals
            innings.totalRuns += result.TotalRuns;
            if (result.isExtraOrPenalty())
            {
                // wide or no ball
            }
            if (result.runsExtra > 0)
            {
                innings.extras += result.runsExtra;
                if (!result.isLegalBall)
                {
                    if (result.commentary.IndexOf("wide", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        innings.wides += result.runsExtra;
                    }
                    else
                    {
                        innings.noBalls += result.runsExtra;
                    }
                }
            }

            if (result.isWicket)
            {
                innings.wickets++;
            }

            if (result.isLegalBall)
            {
                innings.legalBalls++;
            }

            // Over ball notation
            string symbol = GetBallSymbol(result);
            innings.currentOverBalls.Add(symbol);

            // 2. Update Striker stats
            if (striker != null)
            {
                if (result.isLegalBall)
                {
                    striker.AddBallFaced();
                }

                if (result.runsBat > 0 || (result.isLegalBall && !result.isWicket))
                {
                    striker.AddRuns(result.runsBat);
                }

                if (result.isWicket && result.dismissedPlayerName == striker.playerName)
                {
                    striker.Dismiss(result.wicketType);
                }
            }

            // 3. Update Bowler figures
            if (bowler != null)
            {
                bool isWide = (!result.isLegalBall && result.commentary.IndexOf("wide", StringComparison.OrdinalIgnoreCase) >= 0);
                bool isNb = (!result.isLegalBall && !isWide);
                bool creditBowlerWicket = result.isWicket && !string.Equals(result.wicketType, "RunOut", StringComparison.OrdinalIgnoreCase);

                bowler.RecordBall(result.runsBat + (result.runsExtra), result.isLegalBall, creditBowlerWicket, isWide, isNb);
            }

            // 4. Strike Rotation Check
            // Odd runs on delivery swap strike
            if (result.runsBat % 2 == 1)
            {
                shouldRotateStrike = true;
            }

            // 5. Over Completion Check
            if (result.isLegalBall && (innings.legalBalls % 6 == 0))
            {
                isOverComplete = true;
                if (bowler != null)
                {
                    bowler.CompleteOver();
                }
                // At over completion, strike swaps end
                shouldRotateStrike = !shouldRotateStrike;
            }
        }

        private string GetBallSymbol(UnifiedDeliveryResult res)
        {
            if (res.isWicket) return "W";
            if (!res.isLegalBall)
            {
                if (res.commentary.IndexOf("wide", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return res.runsExtra > 1 ? string.Format("{0}Wd", res.runsExtra) : "Wd";
                }
                return res.runsExtra > 1 ? string.Format("{0}Nb", res.runsExtra) : "Nb";
            }
            if (res.runsBat == 0) return "•";
            return res.runsBat.ToString();
        }
    }

    public static class DeliveryResultExtensions
    {
        public static bool isExtraOrPenalty(this UnifiedDeliveryResult r)
        {
            return r.runsExtra > 0 || !r.isLegalBall;
        }
    }
}
