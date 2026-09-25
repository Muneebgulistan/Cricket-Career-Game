using System;
using UnityEngine;
using CricketGame.Gameplay.Bowling;
using CricketGame.Players;

namespace CricketGame.AI
{
    public class OpponentBowlerAI
    {
        private AIDifficultyConfig config;

        public AIDifficultyConfig Config
        {
            get { return config; }
            set { config = value; }
        }

        public OpponentBowlerAI()
        {
            config = AIDifficultyConfig.CreateNormal();
        }

        public OpponentBowlerAI(AIDifficulty difficulty)
        {
            config = AIDifficultyConfig.Create(difficulty);
        }

        public OpponentBowlerAI(AIDifficultyConfig customConfig)
        {
            config = customConfig != null ? customConfig : AIDifficultyConfig.CreateNormal();
        }

        public void DecideLineAndLength(
            AIMatchContext context,
            PlayerProfile batsmanProfile,
            out BowlingLength length,
            out BowlingLine line)
        {
            if (context == null) context = AIMatchContext.CreateDefault();

            System.Random rng = (context.seed != 0) 
                ? new System.Random(context.seed + (context.currentOver * 10) + context.currentBallInOver) 
                : new System.Random();

            int batsmanSkill = (batsmanProfile != null) ? batsmanProfile.battingRating : 60;
            bool isTailender = batsmanSkill < 45;

            // Decision based on match situation & difficulty
            if (context.isDeathOvers)
            {
                // Death overs strategy
                float roll = (float)rng.NextDouble();
                if (config.difficulty == AIDifficulty.Hard)
                {
                    if (roll < 0.55f)
                    {
                        length = BowlingLength.Yorker;
                        line = (roll < 0.30f) ? BowlingLine.OutsideOff : BowlingLine.OffStump;
                    }
                    else if (roll < 0.80f)
                    {
                        length = BowlingLength.Short;
                        line = BowlingLine.OutsideOff;
                    }
                    else
                    {
                        length = BowlingLength.GoodLength;
                        line = BowlingLine.OffStump;
                    }
                }
                else if (config.difficulty == AIDifficulty.Normal)
                {
                    if (roll < 0.45f)
                    {
                        length = BowlingLength.Yorker;
                        line = BowlingLine.OutsideOff;
                    }
                    else if (roll < 0.75f)
                    {
                        length = BowlingLength.GoodLength;
                        line = BowlingLine.OffStump;
                    }
                    else
                    {
                        length = BowlingLength.Full;
                        line = BowlingLine.MiddleStump;
                    }
                }
                else // Easy
                {
                    if (roll < 0.30f)
                    {
                        length = BowlingLength.Yorker;
                        line = BowlingLine.MiddleStump;
                    }
                    else if (roll < 0.65f)
                    {
                        length = BowlingLength.Full;
                        line = BowlingLine.OutsideOff;
                    }
                    else
                    {
                        length = BowlingLength.GoodLength;
                        line = (roll < 0.85f) ? BowlingLine.OffStump : BowlingLine.DownLeg;
                    }
                }
            }
            else if (context.isPowerplay)
            {
                // Powerplay: attack stumps or bowl back-of-a-length
                float roll = (float)rng.NextDouble();
                if (isTailender)
                {
                    length = (roll < 0.5f) ? BowlingLength.Yorker : BowlingLength.GoodLength;
                    line = BowlingLine.MiddleStump;
                }
                else if (config.difficulty == AIDifficulty.Hard)
                {
                    if (roll < 0.50f)
                    {
                        length = BowlingLength.GoodLength;
                        line = (roll < 0.30f) ? BowlingLine.OffStump : BowlingLine.OutsideOff;
                    }
                    else if (roll < 0.80f)
                    {
                        length = BowlingLength.Short;
                        line = BowlingLine.OffStump;
                    }
                    else
                    {
                        length = BowlingLength.Full;
                        line = BowlingLine.MiddleStump;
                    }
                }
                else
                {
                    // Normal / Easy powerplay
                    if (roll < 0.55f)
                    {
                        length = BowlingLength.GoodLength;
                        line = BowlingLine.OffStump;
                    }
                    else if (roll < 0.85f)
                    {
                        length = BowlingLength.Full;
                        line = BowlingLine.MiddleStump;
                    }
                    else
                    {
                        length = BowlingLength.Short;
                        line = (config.difficulty == AIDifficulty.Easy && roll > 0.92f) ? BowlingLine.DownLeg : BowlingLine.OutsideOff;
                    }
                }
            }
            else
            {
                // Middle overs
                float roll = (float)rng.NextDouble();
                if (isTailender)
                {
                    length = (roll < 0.6f) ? BowlingLength.GoodLength : BowlingLength.Bouncer;
                    line = BowlingLine.MiddleStump;
                }
                else if (context.isSecondInnings && context.requiredRunRate > 9.0f)
                {
                    // Defensive middle overs (high pressure on batting team)
                    if (roll < 0.40f)
                    {
                        length = BowlingLength.GoodLength;
                        line = BowlingLine.OutsideOff;
                    }
                    else if (roll < 0.70f)
                    {
                        length = BowlingLength.Short;
                        line = BowlingLine.OutsideOff;
                    }
                    else
                    {
                        length = BowlingLength.Yorker;
                        line = BowlingLine.OffStump;
                    }
                }
                else
                {
                    // Standard middle overs
                    if (roll < 0.50f)
                    {
                        length = BowlingLength.GoodLength;
                        line = (roll < 0.25f) ? BowlingLine.OffStump : BowlingLine.OutsideOff;
                    }
                    else if (roll < 0.75f)
                    {
                        length = BowlingLength.Full;
                        line = BowlingLine.MiddleStump;
                    }
                    else if (roll < 0.90f)
                    {
                        length = BowlingLength.Short;
                        line = BowlingLine.OffStump;
                    }
                    else
                    {
                        length = BowlingLength.Bouncer;
                        line = BowlingLine.OutsideOff;
                    }
                }
            }
        }

        public BowlingDelivery DecideDelivery(
            AIMatchContext context,
            PlayerProfile batsmanProfile,
            BowlingBaseType preferredBowlerType,
            PlayerProfile bowlerProfile = null)
        {
            if (context == null) context = AIMatchContext.CreateDefault();

            BowlingLength length;
            BowlingLine line;
            DecideLineAndLength(context, batsmanProfile, out length, out line);

            BowlingDelivery delivery = BowlingDelivery.CreateDefault(preferredBowlerType, length, line);

            System.Random rng = (context.seed != 0) 
                ? new System.Random(context.seed + (context.currentOver * 100) + context.currentBallInOver) 
                : new System.Random();

            // Set delivery variation based on bowler type & difficulty
            float varRoll = (float)rng.NextDouble();
            if (varRoll < config.variationProbability)
            {
                switch (preferredBowlerType)
                {
                    case BowlingBaseType.Fast:
                    case BowlingBaseType.Medium:
                        if (line == BowlingLine.OutsideOff || line == BowlingLine.OffStump)
                        {
                            delivery.variation = (varRoll < config.variationProbability * 0.5f) 
                                ? DeliveryVariation.Outswinger 
                                : DeliveryVariation.Inswinger;
                            delivery.swingAmount = (delivery.variation == DeliveryVariation.Outswinger) ? 0.45f : -0.40f;
                        }
                        else
                        {
                            delivery.variation = DeliveryVariation.Inswinger;
                            delivery.swingAmount = -0.35f;
                        }
                        break;

                    case BowlingBaseType.OffSpin:
                        delivery.variation = (varRoll < config.variationProbability * 0.4f) 
                            ? DeliveryVariation.ArmBall 
                            : DeliveryVariation.GoodLength;
                        if (delivery.variation == DeliveryVariation.ArmBall)
                        {
                            delivery.spinAmount = 0f;
                            delivery.swingAmount = -0.2f;
                        }
                        break;

                    case BowlingBaseType.LegSpin:
                        delivery.variation = DeliveryVariation.GoodLength;
                        delivery.spinAmount = 6.0f;
                        break;
                }
            }

            // Adjust release speed based on bowler rating and difficulty
            int bowlerSkill = (bowlerProfile != null) ? bowlerProfile.bowlingRating : 70;
            float skillSpeedBonus = ((bowlerSkill - 60) / 40f) * 6f; // -3 to +6 km/h
            delivery.releaseSpeedKph += skillSpeedBonus;

            // Accuracy scalar
            delivery.accuracy = Mathf.Clamp01(0.75f * config.bowlingAccuracyMultiplier);

            return delivery;
        }
    }
}
