using System;
using UnityEngine;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.AI
{
    [Serializable]
    public class AIMatchContext
    {
        public int currentOver;          // 0-indexed
        public int currentBallInOver;    // 0 to 5
        public int totalOvers;           // e.g. 20 for T20, 50 for ODI
        public int currentRuns;          // batting team runs so far
        public int wicketsLost;          // 0 to 10
        public int targetRuns;           // 0 in first innings, >0 in second innings
        public bool isSecondInnings;
        public bool isPowerplay;
        public bool isDeathOvers;
        public float currentRunRate;
        public float requiredRunRate;
        public AIDifficulty difficulty;
        public int seed;

        public int BallsRemaining
        {
            get
            {
                int ballsBowled = (currentOver * 6) + currentBallInOver;
                int totalBalls = totalOvers * 6;
                return Mathf.Max(0, totalBalls - ballsBowled);
            }
        }

        public int RunsNeeded
        {
            get
            {
                if (!isSecondInnings || targetRuns <= 0) return 0;
                return Mathf.Max(0, targetRuns - currentRuns);
            }
        }

        public AIMatchContext()
        {
            currentOver = 0;
            currentBallInOver = 0;
            totalOvers = 20;
            currentRuns = 0;
            wicketsLost = 0;
            targetRuns = 0;
            isSecondInnings = false;
            isPowerplay = true;
            isDeathOvers = false;
            currentRunRate = 0f;
            requiredRunRate = 0f;
            difficulty = AIDifficulty.Normal;
            seed = 0;
        }

        public static AIMatchContext CreateDefault()
        {
            return new AIMatchContext();
        }

        public static AIMatchContext Create(
            int over,
            int ballInOver,
            int maxOvers,
            int runs,
            int wickets,
            int target,
            bool secondInnings,
            AIDifficulty diff,
            int randomSeed = 0)
        {
            AIMatchContext ctx = new AIMatchContext();
            ctx.currentOver = over;
            ctx.currentBallInOver = ballInOver;
            ctx.totalOvers = maxOvers > 0 ? maxOvers : 20;
            ctx.currentRuns = runs;
            ctx.wicketsLost = wickets;
            ctx.targetRuns = target;
            ctx.isSecondInnings = secondInnings;
            ctx.difficulty = diff;
            ctx.seed = randomSeed;

            // Powerplay is typically overs 0 to 5 in T20 (first 6 overs)
            ctx.isPowerplay = (over < (ctx.totalOvers >= 20 ? 6 : Mathf.CeilToInt(ctx.totalOvers * 0.3f)));

            // Death overs are typically the last 4 overs
            ctx.isDeathOvers = (over >= ctx.totalOvers - 4);

            int totalBallsBowled = (over * 6) + ballInOver;
            if (totalBallsBowled > 0)
            {
                ctx.currentRunRate = ((float)runs / totalBallsBowled) * 6f;
            }
            else
            {
                ctx.currentRunRate = 0f;
            }

            if (secondInnings && target > 0)
            {
                int ballsLeft = ctx.BallsRemaining;
                int runsNeeded = ctx.RunsNeeded;
                if (ballsLeft > 0)
                {
                    ctx.requiredRunRate = ((float)runsNeeded / ballsLeft) * 6f;
                }
                else
                {
                    ctx.requiredRunRate = (runsNeeded > 0) ? 99f : 0f;
                }
            }
            else
            {
                ctx.requiredRunRate = 0f;
            }

            return ctx;
        }

        public static AIMatchContext FromInnings(
            InningsScore innings,
            int target,
            int totalOvers,
            bool secondInnings,
            AIDifficulty diff,
            int randomSeed = 0)
        {
            if (innings == null)
            {
                return Create(0, 0, totalOvers, 0, 0, target, secondInnings, diff, randomSeed);
            }

            int over = innings.CompletedOvers;
            int ball = innings.BallsInCurrentOver;
            int runs = innings.totalRuns;
            int wickets = innings.wickets;

            return Create(over, ball, totalOvers, runs, wickets, target, secondInnings, diff, randomSeed);
        }
    }
}
