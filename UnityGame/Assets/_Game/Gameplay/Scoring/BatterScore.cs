using System;

namespace CricketGame.Gameplay.Scoring
{
    [Serializable]
    public class BatterScore
    {
        public string playerId;
        public string playerName;
        public int runs;
        public int balls;
        public int fours;
        public int sixes;
        public int dots;
        public bool isOut;
        public string dismissalText;
        public bool isOnStrike;

        public float StrikeRate
        {
            get { return balls > 0 ? ((float)runs / balls) * 100f : 0f; }
        }

        public BatterScore()
        {
            playerId = Guid.NewGuid().ToString();
            playerName = "Batter";
            runs = 0;
            balls = 0;
            fours = 0;
            sixes = 0;
            dots = 0;
            isOut = false;
            dismissalText = "not out";
            isOnStrike = false;
        }

        public BatterScore(string id, string name, bool onStrike)
        {
            playerId = id;
            playerName = name;
            runs = 0;
            balls = 0;
            fours = 0;
            sixes = 0;
            dots = 0;
            isOut = false;
            dismissalText = "not out";
            isOnStrike = onStrike;
        }

        public void AddBallFaced()
        {
            balls++;
        }

        public void AddRuns(int r)
        {
            runs += r;
            if (r == 0) dots++;
            else if (r == 4) fours++;
            else if (r == 6) sixes++;
        }

        public void Dismiss(string dismissalInfo)
        {
            isOut = true;
            dismissalText = dismissalInfo;
            isOnStrike = false;
        }
    }
}
