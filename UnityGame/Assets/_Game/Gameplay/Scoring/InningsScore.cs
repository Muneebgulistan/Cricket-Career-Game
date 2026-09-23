using System;
using System.Collections.Generic;

namespace CricketGame.Gameplay.Scoring
{
    [Serializable]
    public class InningsScore
    {
        public string battingTeam;
        public string bowlingTeam;
        public int totalRuns;
        public int wickets;
        public int legalBalls;
        public int extras;
        public int wides;
        public int noBalls;
        public int byes;
        public int legByes;
        public int maxOvers;
        public int targetScore; // 0 if setting score (1st innings), >0 if chasing (2nd innings)

        public List<BatterScore> batters;
        public List<BowlerFigures> bowlers;
        public List<string> currentOverBalls;

        public int CompletedOvers
        {
            get { return legalBalls / 6; }
        }

        public int BallsInCurrentOver
        {
            get { return legalBalls % 6; }
        }

        public float OversFloat
        {
            get { return CompletedOvers + (BallsInCurrentOver / 6.0f); }
        }

        public string OversFormatted
        {
            get { return string.Format("{0}.{1}", CompletedOvers, BallsInCurrentOver); }
        }

        public float CurrentRunRate
        {
            get { return OversFloat > 0.05f ? totalRuns / OversFloat : 0f; }
        }

        public int RemainingBalls
        {
            get
            {
                int maxBalls = maxOvers * 6;
                return Math.Max(0, maxBalls - legalBalls);
            }
        }

        public int RunsNeeded
        {
            get
            {
                if (targetScore <= 0) return 0;
                return Math.Max(0, targetScore - totalRuns);
            }
        }

        public float RequiredRunRate
        {
            get
            {
                if (targetScore <= 0 || RemainingBalls <= 0) return 0f;
                return ((float)RunsNeeded / RemainingBalls) * 6.0f;
            }
        }

        public bool IsAllOut
        {
            get { return wickets >= 10; }
        }

        public bool IsOversFinished
        {
            get { return legalBalls >= maxOvers * 6; }
        }

        public bool HasReachedTarget
        {
            get { return targetScore > 0 && totalRuns >= targetScore; }
        }

        public bool IsInningsComplete
        {
            get { return IsAllOut || IsOversFinished || HasReachedTarget; }
        }

        public InningsScore()
        {
            battingTeam = "Team A";
            bowlingTeam = "Team B";
            totalRuns = 0;
            wickets = 0;
            legalBalls = 0;
            extras = 0;
            wides = 0;
            noBalls = 0;
            byes = 0;
            legByes = 0;
            maxOvers = 20;
            targetScore = 0;
            batters = new List<BatterScore>();
            bowlers = new List<BowlerFigures>();
            currentOverBalls = new List<string>();
        }

        public InningsScore(string batting, string bowling, int overs, int target)
        {
            battingTeam = batting;
            bowlingTeam = bowling;
            totalRuns = 0;
            wickets = 0;
            legalBalls = 0;
            extras = 0;
            wides = 0;
            noBalls = 0;
            byes = 0;
            legByes = 0;
            maxOvers = overs;
            targetScore = target;
            batters = new List<BatterScore>();
            bowlers = new List<BowlerFigures>();
            currentOverBalls = new List<string>();
        }

        public BatterScore GetOrCreateBatter(string id, string name, bool onStrike)
        {
            for (int i = 0; i < batters.Count; i++)
            {
                if (batters[i].playerId == id) return batters[i];
            }
            BatterScore b = new BatterScore(id, name, onStrike);
            batters.Add(b);
            return b;
        }

        public BowlerFigures GetOrCreateBowler(string id, string name)
        {
            for (int i = 0; i < bowlers.Count; i++)
            {
                if (bowlers[i].playerId == id) return bowlers[i];
            }
            BowlerFigures b = new BowlerFigures(id, name);
            bowlers.Add(b);
            return b;
        }
    }
}
