using System;
using UnityEngine;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Players;

namespace CricketGame.AI
{
    public class OpponentBatsmanAI
    {
        private AIDifficultyConfig config;

        public AIDifficultyConfig Config
        {
            get { return config; }
            set { config = value; }
        }

        public OpponentBatsmanAI()
        {
            config = AIDifficultyConfig.CreateNormal();
        }

        public OpponentBatsmanAI(AIDifficulty difficulty)
        {
            config = AIDifficultyConfig.Create(difficulty);
        }

        public OpponentBatsmanAI(AIDifficultyConfig customConfig)
        {
            config = customConfig != null ? customConfig : AIDifficultyConfig.CreateNormal();
        }

        public BattingShotType DecideShot(
            BowlingLength length,
            BowlingLine line,
            float speedKph,
            AIMatchContext context,
            PlayerProfile batsmanProfile,
            out bool isLofted)
        {
            if (context == null) context = AIMatchContext.CreateDefault();

            System.Random rng = (context.seed != 0) 
                ? new System.Random(context.seed + (context.currentOver * 50) + (context.currentBallInOver * 7)) 
                : new System.Random();

            int battingSkill = (batsmanProfile != null) ? batsmanProfile.battingRating : 65;
            bool isAggressiveSituation = context.isDeathOvers || (context.isSecondInnings && context.requiredRunRate > 8.0f) || (context.isPowerplay && battingSkill > 60);
            float loftChance = isAggressiveSituation ? (0.45f * config.aggressionFactor) : (0.15f * config.aggressionFactor);

            // Roll for lofted intent
            isLofted = ((float)rng.NextDouble() < loftChance);

            // Tailenders or low difficulty misjudge shots occasionally
            float mistakeRoll = (float)rng.NextDouble();
            if (mistakeRoll > config.shotSelectionQuality && battingSkill < 55)
            {
                // Poor shot selection under pressure
                if (length == BowlingLength.Short || length == BowlingLength.Bouncer)
                {
                    isLofted = true;
                    return BattingShotType.Hook; // High risk hook for a tailender
                }
                return BattingShotType.LoftedDrive;
            }

            // Standard intelligent shot selection based on line and length:
            switch (length)
            {
                case BowlingLength.Yorker:
                    isLofted = false; // Cannot safely loft a yorker
                    if (isAggressiveSituation && line == BowlingLine.OutsideOff)
                    {
                        return BattingShotType.SquareDrive;
                    }
                    return BattingShotType.Defensive;

                case BowlingLength.Full:
                    if (isLofted)
                    {
                        if (line == BowlingLine.OffStump || line == BowlingLine.MiddleStump)
                            return BattingShotType.StraightLoft;
                        else
                            return BattingShotType.LoftedDrive;
                    }

                    if (line == BowlingLine.OutsideOff)
                        return BattingShotType.CoverDrive;
                    if (line == BowlingLine.OffStump)
                        return BattingShotType.StraightDrive;
                    if (line == BowlingLine.MiddleStump)
                        return BattingShotType.OnDrive;
                    if (line == BowlingLine.LegStump || line == BowlingLine.DownLeg)
                        return BattingShotType.Sweep;
                    return BattingShotType.StraightDrive;

                case BowlingLength.GoodLength:
                    if (isAggressiveSituation && (float)rng.NextDouble() < 0.35f)
                    {
                        if (line == BowlingLine.OutsideOff)
                            return BattingShotType.CoverDrive;
                        if (line == BowlingLine.OffStump)
                            return isLofted ? BattingShotType.StraightLoft : BattingShotType.StraightDrive;
                        return BattingShotType.OnDrive;
                    }

                    // Respect good length on stumps
                    if (line == BowlingLine.MiddleStump || line == BowlingLine.OffStump)
                    {
                        if (context.wicketsLost > 6 && !isAggressiveSituation)
                            return BattingShotType.Defensive;
                        return BattingShotType.StraightDrive;
                    }

                    if (line == BowlingLine.OutsideOff)
                        return BattingShotType.CoverDrive;
                    if (line == BowlingLine.LegStump)
                        return BattingShotType.OnDrive;
                    return BattingShotType.Defensive;

                case BowlingLength.Short:
                    if (line == BowlingLine.OutsideOff || line == BowlingLine.OffStump)
                    {
                        isLofted = isAggressiveSituation;
                        return BattingShotType.Cut;
                    }
                    else
                    {
                        isLofted = isAggressiveSituation;
                        return BattingShotType.Pull;
                    }

                case BowlingLength.Bouncer:
                    if (battingSkill > 70 && isAggressiveSituation)
                    {
                        isLofted = true;
                        return BattingShotType.Hook;
                    }
                    if (line == BowlingLine.OutsideOff)
                    {
                        return BattingShotType.Cut;
                    }
                    // Duck / defensive evasion against dangerous bouncers
                    isLofted = false;
                    return BattingShotType.Defensive;

                default:
                    return BattingShotType.StraightDrive;
            }
        }

        public void SimulateTiming(
            AIMatchContext context,
            PlayerProfile batsmanProfile,
            BattingShot shot,
            out float timingDelta,
            out TimingQuality quality,
            out ContactQuality contact)
        {
            if (context == null) context = AIMatchContext.CreateDefault();

            System.Random rng = (context.seed != 0) 
                ? new System.Random(context.seed + (context.currentOver * 80) + (context.currentBallInOver * 13)) 
                : new System.Random();

            int rating = batsmanProfile != null ? batsmanProfile.battingRating : 65;
            float skillFactor = Mathf.Clamp01(rating / 100f);

            // Timing error window scales with difficulty & skill
            float maxError = 0.20f * (1.2f - (skillFactor * 0.4f)) * (1.0f / config.timingToleranceMultiplier);
            float rawDelta = ((float)(rng.NextDouble() * 2.0 - 1.0)) * maxError;
            timingDelta = rawDelta;

            float absDelta = Mathf.Abs(timingDelta);
            if (absDelta <= 0.04f)
            {
                quality = TimingQuality.Perfect;
                contact = ContactQuality.SweetSpot;
            }
            else if (absDelta <= 0.10f)
            {
                quality = TimingQuality.Good;
                contact = ContactQuality.GoodContact;
            }
            else if (absDelta <= 0.20f)
            {
                quality = (timingDelta < 0f) ? TimingQuality.Early : TimingQuality.Late;
                // Chance of edge on poor timing
                float edgeRoll = (float)rng.NextDouble();
                contact = (edgeRoll < (shot.risk * config.edgeProbabilityMultiplier)) ? ContactQuality.Edge : ContactQuality.GoodContact;
            }
            else
            {
                quality = TimingQuality.Miss;
                contact = ContactQuality.Miss;
            }
        }

        public BattingResult SimulateShotResult(
            BowlingDelivery delivery,
            AIMatchContext context,
            PlayerProfile batsmanProfile,
            BattingSettings settings,
            Vector3 batsmanForward)
        {
            if (delivery == null) delivery = new BowlingDelivery();
            if (settings == null) settings = BattingSettings.CreateDefault();

            bool isLofted;
            BattingShotType shotType = DecideShot(delivery.targetLength, delivery.targetLine, delivery.releaseSpeedKph, context, batsmanProfile, out isLofted);

            BattingShot shot = BattingShot.CreateDefault(shotType);
            if (isLofted && !shot.isLofted)
            {
                shot.isLofted = true;
                shot.elevationAngle = Mathf.Max(shot.elevationAngle, 28f);
            }

            float timingDelta;
            TimingQuality timingQuality;
            ContactQuality contactQuality;
            SimulateTiming(context, batsmanProfile, shot, out timingDelta, out timingQuality, out contactQuality);

            float sweetSpotDistance = 0f;
            if (contactQuality == ContactQuality.SweetSpot) sweetSpotDistance = 0.02f;
            else if (contactQuality == ContactQuality.GoodContact) sweetSpotDistance = 0.08f;
            else if (contactQuality == ContactQuality.Edge) sweetSpotDistance = 0.24f;
            else sweetSpotDistance = 0.50f;

            return BattingEvaluator.EvaluateShot(
                shot,
                timingQuality,
                timingDelta,
                contactQuality,
                sweetSpotDistance,
                batsmanProfile,
                settings,
                batsmanForward,
                delivery.releaseSpeedKph
            );
        }
    }
}
