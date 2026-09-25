using System;
using UnityEngine;
using CricketGame.AI;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Players;

namespace CricketGame.Gameplay.Match
{
    public class PlayerTurnController : MonoBehaviour
    {
        [Header("Human Player Identity")]
        [SerializeField] private string humanPlayerName = "Muneeb Gulistan";
        [SerializeField] private string humanTeamName = "Lahore Eagles";
        [SerializeField] private AIDifficulty difficulty = AIDifficulty.Normal;

        private OpponentBowlerAI bowlerAI;
        private OpponentBatsmanAI batsmanAI;

        public string HumanPlayerName
        {
            get { return humanPlayerName; }
            set { humanPlayerName = value; }
        }

        public string HumanTeamName
        {
            get { return humanTeamName; }
            set { humanTeamName = value; }
        }

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

        public void Configure(string playerName, string teamName, AIDifficulty diff)
        {
            humanPlayerName = playerName;
            humanTeamName = teamName;
            Difficulty = diff;
        }

        public bool IsHumanStriker(string strikerName)
        {
            if (string.IsNullOrEmpty(strikerName) || string.IsNullOrEmpty(humanPlayerName)) return false;
            string cleanStriker = strikerName.Replace("*", "").Trim();
            string cleanHuman = humanPlayerName.Replace("*", "").Trim();
            return string.Equals(cleanStriker, cleanHuman, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsHumanBowler(string bowlerName)
        {
            if (string.IsNullOrEmpty(bowlerName) || string.IsNullOrEmpty(humanPlayerName)) return false;
            string cleanBowler = bowlerName.Replace("*", "").Trim();
            string cleanHuman = humanPlayerName.Replace("*", "").Trim();
            return string.Equals(cleanBowler, cleanHuman, StringComparison.OrdinalIgnoreCase);
        }

        public BowlingDelivery ExecuteAIBowling(
            AIMatchContext context, 
            PlayerProfile batsmanProfile, 
            BowlingBaseType bowlerType,
            PlayerProfile bowlerProfile = null)
        {
            return BowlerAI.DecideDelivery(context, batsmanProfile, bowlerType, bowlerProfile);
        }

        public BattingResult ExecuteAIBatting(
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
