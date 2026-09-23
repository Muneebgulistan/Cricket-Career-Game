using System;

namespace CricketGame.Gameplay.Scoring
{
    [Serializable]
    public class BowlerFigures
    {
        public string playerId;
        public string playerName;
        public int legalBalls;
        public int maidens;
        public int runsConceded;
        public int wickets;
        public int wides;
        public int noBalls;
        public int dots;
        public int currentOverRuns;

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

        public float Economy
        {
            get { return OversFloat > 0.05f ? runsConceded / OversFloat : 0f; }
        }

        public string FiguresFormatted
        {
            get
            {
                return string.Format("{0}-{1}-{2}-{3}", OversFormatted, maidens, runsConceded, wickets);
            }
        }

        public BowlerFigures()
        {
            playerId = Guid.NewGuid().ToString();
            playerName = "Bowler";
            legalBalls = 0;
            maidens = 0;
            runsConceded = 0;
            wickets = 0;
            wides = 0;
            noBalls = 0;
            dots = 0;
            currentOverRuns = 0;
        }

        public BowlerFigures(string id, string name)
        {
            playerId = id;
            playerName = name;
            legalBalls = 0;
            maidens = 0;
            runsConceded = 0;
            wickets = 0;
            wides = 0;
            noBalls = 0;
            dots = 0;
            currentOverRuns = 0;
        }

        public void RecordBall(int runs, bool isLegal, bool isWicket, bool isWide, bool isNoBall)
        {
            runsConceded += runs;
            currentOverRuns += runs;

            if (isLegal)
            {
                legalBalls++;
                if (runs == 0 && !isWicket)
                {
                    dots++;
                }
            }

            if (isWicket)
            {
                wickets++;
            }

            if (isWide)
            {
                wides++;
            }

            if (isNoBall)
            {
                noBalls++;
            }
        }

        public void CompleteOver()
        {
            if (currentOverRuns == 0 && BallsInCurrentOver == 0 && legalBalls > 0)
            {
                maidens++;
            }
            currentOverRuns = 0;
        }
    }
}
