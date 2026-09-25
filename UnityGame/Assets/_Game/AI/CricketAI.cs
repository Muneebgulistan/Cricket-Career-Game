using UnityEngine;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Players;

namespace CricketGame.AI
{
    /// <summary>
    /// AI decision layer for cricket gameplay.
    /// Bridges the specialized OpponentBowlerAI and OpponentBatsmanAI systems
    /// with the core Unity game loop and provides backwards compatibility.
    /// </summary>
    public class CricketAI : MonoBehaviour
    {
        [Header("Difficulty Setting")]
        [SerializeField] private AIDifficulty difficulty = AIDifficulty.Normal;

        private OpponentBowlerAI bowlerAI;
        private OpponentBatsmanAI batsmanAI;

        public AIDifficulty Difficulty
        {
            get { return difficulty; }
            set 
            { 
                difficulty = value; 
                if (bowlerAI != null) bowlerAI.Config = AIDifficultyConfig.Create(value);
                if (batsmanAI != null) batsmanAI.Config = AIDifficultyConfig.Create(value);
            }
        }

        public OpponentBowlerAI BowlerAI
        {
            get
            {
                if (bowlerAI == null) bowlerAI = new OpponentBowlerAI(difficulty);
                return bowlerAI;
            }
        }

        public OpponentBatsmanAI BatsmanAI
        {
            get
            {
                if (batsmanAI == null) batsmanAI = new OpponentBatsmanAI(difficulty);
                return batsmanAI;
            }
        }

        private void Awake()
        {
            bowlerAI = new OpponentBowlerAI(difficulty);
            batsmanAI = new OpponentBatsmanAI(difficulty);
        }

        /// <summary>
        /// Decide the AI batsman's shot based on the incoming delivery (simple API).
        /// </summary>
        public BattingShotType DecideAIShot(BowlingLength length, BowlingLine line, bool isPowerplay)
        {
            bool isLofted;
            AIMatchContext ctx = AIMatchContext.CreateDefault();
            ctx.isPowerplay = isPowerplay;
            ctx.difficulty = difficulty;
            return BatsmanAI.DecideShot(length, line, 135f, ctx, null, out isLofted);
        }

        /// <summary>
        /// Decide the AI bowler's delivery length and line based on match context (simple API).
        /// </summary>
        public void DecideAIBowling(out BowlingLength length, out BowlingLine line, bool deathOvers)
        {
            AIMatchContext ctx = AIMatchContext.CreateDefault();
            ctx.isDeathOvers = deathOvers;
            ctx.difficulty = difficulty;
            BowlerAI.DecideLineAndLength(ctx, null, out length, out line);
        }

        /// <summary>
        /// Advanced: Decide complete bowling delivery for AI bowler.
        /// </summary>
        public BowlingDelivery DecideCompleteDelivery(AIMatchContext context, PlayerProfile batsmanProfile, BowlingBaseType bowlerType)
        {
            return BowlerAI.DecideDelivery(context, batsmanProfile, bowlerType);
        }

        /// <summary>
        /// Advanced: Simulate complete batting result for AI batsman.
        /// </summary>
        public BattingResult SimulateCompleteBatting(
            BowlingDelivery delivery,
            AIMatchContext context,
            PlayerProfile batsmanProfile,
            BattingSettings settings,
            Vector3 batsmanForward)
        {
            return BatsmanAI.SimulateShotResult(delivery, context, batsmanProfile, settings, batsmanForward);
        }
    }
}
