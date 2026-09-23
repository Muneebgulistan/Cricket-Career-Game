using System;

namespace CricketGame.Gameplay.Scoring
{
    [Serializable]
    public class ScoreEvent
    {
        public ScoreEventType eventType;
        public int runsBat;
        public int runsExtra;
        public bool isLegalBall;
        public bool isWicket;
        public bool isBoundaryFour;
        public bool isBoundarySix;
        public string dismissedBatterName;
        public string description;

        public int TotalRuns
        {
            get { return runsBat + runsExtra; }
        }

        public ScoreEvent()
        {
            eventType = ScoreEventType.Dot;
            runsBat = 0;
            runsExtra = 0;
            isLegalBall = true;
            isWicket = false;
            isBoundaryFour = false;
            isBoundarySix = false;
            dismissedBatterName = string.Empty;
            description = "Dot ball";
        }

        public static ScoreEvent DotBall()
        {
            ScoreEvent e = new ScoreEvent();
            e.eventType = ScoreEventType.Dot;
            e.description = "Dot ball";
            return e;
        }

        public static ScoreEvent Runs(int runs)
        {
            ScoreEvent e = new ScoreEvent();
            e.runsBat = runs;
            e.isLegalBall = true;
            if (runs == 1) { e.eventType = ScoreEventType.Single; e.description = "1 run"; }
            else if (runs == 2) { e.eventType = ScoreEventType.Two; e.description = "2 runs"; }
            else if (runs == 3) { e.eventType = ScoreEventType.Three; e.description = "3 runs"; }
            else if (runs == 4) { e.eventType = ScoreEventType.Four; e.isBoundaryFour = true; e.description = "FOUR! Boundary scored!"; }
            else if (runs == 6) { e.eventType = ScoreEventType.Six; e.isBoundarySix = true; e.description = "SIX! Out of the ground!"; }
            else { e.eventType = ScoreEventType.Single; e.description = string.Format("{0} runs", runs); }
            return e;
        }

        public static ScoreEvent Extra(ScoreEventType extraType, int penaltyRuns, bool legal)
        {
            ScoreEvent e = new ScoreEvent();
            e.eventType = extraType;
            e.runsExtra = penaltyRuns;
            e.isLegalBall = legal;
            e.description = string.Format("{0} ({1} runs)", extraType, penaltyRuns);
            return e;
        }

        public static ScoreEvent Wicket(ScoreEventType dismissalType, string batterName)
        {
            ScoreEvent e = new ScoreEvent();
            e.eventType = dismissalType;
            e.isLegalBall = true;
            e.isWicket = true;
            e.dismissedBatterName = batterName;
            e.description = string.Format("WICKET! {0} is out ({1})!", batterName, dismissalType);
            return e;
        }
    }
}
