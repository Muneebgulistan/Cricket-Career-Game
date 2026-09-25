using System;
using UnityEngine;

namespace CricketGame.AI
{
    public enum AIDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    [Serializable]
    public class AIDifficultyConfig
    {
        public AIDifficulty difficulty;

        [Header("Batting AI Multipliers")]
        public float timingToleranceMultiplier;   // >1.0 means easier/wider window, <1.0 tighter
        public float shotSelectionQuality;        // 0.0 - 1.0 (1.0 = optimal shot picked every time)
        public float edgeProbabilityMultiplier;   // multiplier on edge/mis-hit chance
        public float aggressionFactor;            // 0.0 - 1.0 willingness to loft/attack

        [Header("Bowling AI Multipliers")]
        public float bowlingAccuracyMultiplier;   // 0.0 - 1.5 accuracy scalar
        public float variationProbability;        // 0.0 - 1.0 frequency of variations
        public float lineLengthDiscipline;        // 0.0 - 1.0 sticking to gameplan

        [Header("Fielding AI Multipliers")]
        public float fielderReactionDelay;        // seconds delay before fielder reacts
        public float catchSuccessMultiplier;      // scalar on catch rating

        public AIDifficultyConfig()
        {
            difficulty = AIDifficulty.Normal;
            timingToleranceMultiplier = 1.0f;
            shotSelectionQuality = 0.85f;
            edgeProbabilityMultiplier = 1.0f;
            aggressionFactor = 0.70f;
            bowlingAccuracyMultiplier = 0.90f;
            variationProbability = 0.35f;
            lineLengthDiscipline = 0.80f;
            fielderReactionDelay = 0.12f;
            catchSuccessMultiplier = 1.0f;
        }

        public static AIDifficultyConfig Create(AIDifficulty diff)
        {
            switch (diff)
            {
                case AIDifficulty.Easy:
                    return CreateEasy();
                case AIDifficulty.Hard:
                    return CreateHard();
                case AIDifficulty.Normal:
                default:
                    return CreateNormal();
            }
        }

        public static AIDifficultyConfig CreateEasy()
        {
            AIDifficultyConfig cfg = new AIDifficultyConfig();
            cfg.difficulty = AIDifficulty.Easy;
            cfg.timingToleranceMultiplier = 1.35f;
            cfg.shotSelectionQuality = 0.60f;
            cfg.edgeProbabilityMultiplier = 0.75f;
            cfg.aggressionFactor = 0.50f;
            cfg.bowlingAccuracyMultiplier = 0.70f;
            cfg.variationProbability = 0.20f;
            cfg.lineLengthDiscipline = 0.55f;
            cfg.fielderReactionDelay = 0.25f;
            cfg.catchSuccessMultiplier = 0.85f;
            return cfg;
        }

        public static AIDifficultyConfig CreateNormal()
        {
            AIDifficultyConfig cfg = new AIDifficultyConfig();
            cfg.difficulty = AIDifficulty.Normal;
            cfg.timingToleranceMultiplier = 1.0f;
            cfg.shotSelectionQuality = 0.85f;
            cfg.edgeProbabilityMultiplier = 1.0f;
            cfg.aggressionFactor = 0.70f;
            cfg.bowlingAccuracyMultiplier = 0.90f;
            cfg.variationProbability = 0.35f;
            cfg.lineLengthDiscipline = 0.80f;
            cfg.fielderReactionDelay = 0.12f;
            cfg.catchSuccessMultiplier = 1.0f;
            return cfg;
        }

        public static AIDifficultyConfig CreateHard()
        {
            AIDifficultyConfig cfg = new AIDifficultyConfig();
            cfg.difficulty = AIDifficulty.Hard;
            cfg.timingToleranceMultiplier = 0.75f;
            cfg.shotSelectionQuality = 0.98f;
            cfg.edgeProbabilityMultiplier = 1.30f;
            cfg.aggressionFactor = 0.90f;
            cfg.bowlingAccuracyMultiplier = 1.15f;
            cfg.variationProbability = 0.55f;
            cfg.lineLengthDiscipline = 0.95f;
            cfg.fielderReactionDelay = 0.04f;
            cfg.catchSuccessMultiplier = 1.15f;
            return cfg;
        }
    }
}
