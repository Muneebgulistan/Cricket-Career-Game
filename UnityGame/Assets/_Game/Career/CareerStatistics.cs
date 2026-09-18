using System;
using System.Collections.Generic;

namespace CricketGame.Career
{
    [Serializable]
    public class BattingStatistics
    {
        public int matches;
        public int innings;
        public int notOuts;
        public int runs;
        public int highestScore;
        public int ballsFaced;
        public int fours;
        public int sixes;
        public int fifties;
        public int hundreds;

        public float Average
        {
            get { return innings - notOuts > 0 ? (float)runs / (innings - notOuts) : runs; }
        }

        public float StrikeRate
        {
            get { return ballsFaced > 0 ? (float)runs / ballsFaced * 100f : 0f; }
        }
    }

    [Serializable]
    public class BowlingStatistics
    {
        public int matches;
        public int innings;
        public float overs;
        public int maidens;
        public int runsConceded;
        public int wickets;
        public int bestWickets;
        public int bestRunsConceded;
        public int fiveWicketHauls;

        public float Economy
        {
            get { return overs > 0 ? runsConceded / overs : 0f; }
        }

        public float Average
        {
            get { return wickets > 0 ? (float)runsConceded / wickets : 0f; }
        }

        public float StrikeRate
        {
            get { return wickets > 0 ? (overs * 6f) / wickets : 0f; }
        }
    }

    [Serializable]
    public class FieldingStatistics
    {
        public int catches;
        public int runOuts;
        public int stumpings;
    }

    [Serializable]
    public class TournamentStatisticsRecord
    {
        public string tournamentId;
        public string tournamentName;
        public BattingStatistics batting = new BattingStatistics();
        public BowlingStatistics bowling = new BowlingStatistics();
        public FieldingStatistics fielding = new FieldingStatistics();
    }

    [Serializable]
    public class CareerStatistics
    {
        public BattingStatistics allTimeBatting = new BattingStatistics();
        public BowlingStatistics allTimeBowling = new BowlingStatistics();
        public FieldingStatistics allTimeFielding = new FieldingStatistics();
        public List<TournamentStatisticsRecord> byTournament = new List<TournamentStatisticsRecord>();

        public void RecordMatchBatting(int runsScored, int balls, int f4, int f6, bool isOut)
        {
            allTimeBatting.matches++;
            allTimeBatting.innings++;
            allTimeBatting.runs += runsScored;
            allTimeBatting.ballsFaced += balls;
            allTimeBatting.fours += f4;
            allTimeBatting.sixes += f6;
            if (!isOut) allTimeBatting.notOuts++;
            if (runsScored > allTimeBatting.highestScore) allTimeBatting.highestScore = runsScored;
            if (runsScored >= 100) allTimeBatting.hundreds++;
            else if (runsScored >= 50) allTimeBatting.fifties++;
        }

        public void RecordMatchBowling(float oversBowled, int maidensBowled, int runsGiven, int wkts)
        {
            allTimeBowling.matches++;
            allTimeBowling.innings++;
            allTimeBowling.overs += oversBowled;
            allTimeBowling.maidens += maidensBowled;
            allTimeBowling.runsConceded += runsGiven;
            allTimeBowling.wickets += wkts;
            if (wkts >= 5) allTimeBowling.fiveWicketHauls++;
        }

        public void RecordMatchFielding(int catchCount, int runOutCount, int stumpingCount)
        {
            allTimeFielding.catches += catchCount;
            allTimeFielding.runOuts += runOutCount;
            allTimeFielding.stumpings += stumpingCount;
        }
    }
}
