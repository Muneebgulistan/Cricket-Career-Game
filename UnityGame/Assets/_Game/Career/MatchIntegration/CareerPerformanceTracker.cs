using System;
using UnityEngine;
using CricketGame.Cricket;

namespace CricketGame.Career.MatchIntegration
{
    public class CareerPerformanceTracker
    {
        public string playerId;
        public string playerName;
        public int runsScored;
        public int ballsFaced;
        public int fours;
        public int sixes;
        public bool isOut;
        public string dismissalReason;

        public float oversBowled;
        public int maidens;
        public int runsConceded;
        public int wicketsTaken;

        public int catches;
        public int runOuts;
        public int stumpings;

        public float MatchRating
        {
            get { return CalculateMatchRating(); }
        }

        public CareerPerformanceTracker(string id, string name)
        {
            playerId = id;
            playerName = name;
            Reset();
        }

        public void Reset()
        {
            runsScored = 0;
            ballsFaced = 0;
            fours = 0;
            sixes = 0;
            isOut = false;
            dismissalReason = "not out";
            oversBowled = 0f;
            maidens = 0;
            runsConceded = 0;
            wicketsTaken = 0;
            catches = 0;
            runOuts = 0;
            stumpings = 0;
        }

        public void RecordBattingDelivery(int runs, bool legal)
        {
            if (legal) ballsFaced++;
            runsScored += runs;
            if (runs == 4) fours++;
            else if (runs == 6) sixes++;
        }

        public void RecordDismissal(string reason)
        {
            isOut = true;
            dismissalReason = reason;
        }

        public void RecordBowlingDelivery(int runs, bool legal, bool wicket)
        {
            runsConceded += runs;
            if (wicket) wicketsTaken++;
        }

        public void CompleteBowlingOver(bool isMaiden)
        {
            oversBowled += 1.0f;
            if (isMaiden) maidens++;
        }

        public void RecordCatch() { catches++; }
        public void RecordRunOut() { runOuts++; }
        public void RecordStumping() { stumpings++; }

        public float CalculateMatchRating()
        {
            float rating = 5.0f; // Neutral baseline

            // Batting influence
            rating += (runsScored * 0.05f);
            rating += (fours * 0.15f);
            rating += (sixes * 0.35f);
            if (runsScored >= 100) rating += 2.0f;
            else if (runsScored >= 50) rating += 1.0f;
            else if (runsScored >= 30) rating += 0.5f;

            if (isOut && runsScored == 0 && ballsFaced > 0)
            {
                rating -= 1.0f; // Duck penalty
            }

            // Bowling influence
            rating += (wicketsTaken * 1.25f);
            rating += (maidens * 0.6f);
            if (wicketsTaken >= 5) rating += 2.0f;
            else if (wicketsTaken >= 3) rating += 1.0f;

            if (oversBowled > 0)
            {
                float econ = runsConceded / oversBowled;
                if (econ < 6.0f) rating += 0.5f;
                else if (econ > 10.0f) rating -= 0.5f;
            }

            // Fielding influence
            rating += (catches * 0.6f);
            rating += (runOuts * 0.8f);
            rating += (stumpings * 0.7f);

            return Mathf.Clamp(rating, 1.0f, 10.0f);
        }

        public PlayerMatchPerformance ToPlayerPerformance()
        {
            PlayerMatchPerformance p = new PlayerMatchPerformance();
            p.playerId = playerId;
            p.playerName = playerName;
            p.runs = runsScored;
            p.balls = ballsFaced;
            p.fours = fours;
            p.sixes = sixes;
            p.isOut = isOut;
            p.oversBowled = oversBowled;
            p.maidens = maidens;
            p.runsConceded = runsConceded;
            p.wickets = wicketsTaken;
            p.catches = catches;
            p.runOuts = runOuts;
            p.stumpings = stumpings;
            p.matchRating = CalculateMatchRating();
            return p;
        }
    }
}
