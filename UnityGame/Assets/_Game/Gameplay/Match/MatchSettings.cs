using System;
using CricketGame.Cricket;

namespace CricketGame.Gameplay.Match
{
    [Serializable]
    public class MatchSettings
    {
        public MatchFormat format = MatchFormat.T20;
        public int totalOvers = 20;
        public int maxOversPerBowler = 4;
        public int maxWickets = 10;
        public int powerplayOvers = 6;
        public bool allowFreeHit = true;
        public bool isFastSimulation = false;

        public static MatchSettings DefaultT20()
        {
            MatchSettings s = new MatchSettings();
            s.format = MatchFormat.T20;
            s.totalOvers = 20;
            s.maxOversPerBowler = 4;
            s.maxWickets = 10;
            s.powerplayOvers = 6;
            s.allowFreeHit = true;
            return s;
        }

        public static MatchSettings FastTestMatch(int overs)
        {
            MatchSettings s = new MatchSettings();
            s.format = MatchFormat.T20;
            s.totalOvers = overs;
            s.maxOversPerBowler = Math.Max(1, overs / 2);
            s.maxWickets = 10;
            s.powerplayOvers = Math.Max(1, overs / 4);
            s.allowFreeHit = true;
            s.isFastSimulation = true;
            return s;
        }
    }
}
