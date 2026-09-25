using UnityEngine;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;

namespace CricketGame.AI
{
    /// <summary>
    /// Simple AI decision layer for batting shot selection and bowling delivery selection.
    /// Uses the existing BattingShotType, BowlingLength, and BowlingLine types from the
    /// real project architecture. Do NOT introduce duplicate enum definitions.
    /// </summary>
    public class CricketAI : MonoBehaviour
    {
        /// <summary>
        /// Decide the AI batsman's shot based on the incoming delivery.
        /// </summary>
        public BattingShotType DecideAIShot(BowlingLength length, BowlingLine line, bool isPowerplay)
        {
            if (isPowerplay && length == BowlingLength.Full)
            {
                return BattingShotType.LoftedDrive;
            }

            if (length == BowlingLength.Short || length == BowlingLength.Bouncer)
            {
                return BattingShotType.Pull;
            }

            if (length == BowlingLength.Yorker)
            {
                return BattingShotType.Defensive;
            }

            if (line == BowlingLine.OutsideOff || line == BowlingLine.OffStump)
            {
                return BattingShotType.CoverDrive;
            }

            return BattingShotType.StraightDrive;
        }

        /// <summary>
        /// Decide the AI bowler's delivery length and line based on match context.
        /// </summary>
        public void DecideAIBowling(out BowlingLength length, out BowlingLine line, bool deathOvers)
        {
            if (deathOvers)
            {
                length = BowlingLength.Yorker;
                line = BowlingLine.OutsideOff;
            }
            else
            {
                length = BowlingLength.GoodLength;
                line = BowlingLine.MiddleStump;
            }
        }
    }
}
