using System;

namespace CricketGame.Gameplay.Scoring
{
    [Serializable]
    public class UnifiedDeliveryResult
    {
        public float releaseSpeed;
        public string deliveryType;
        public string shotType;
        public string shotTiming;
        public int runsBat;
        public int runsExtra;
        public bool isLegalBall;
        public bool isBoundaryFour;
        public bool isBoundarySix;
        public bool isWicket;
        public string wicketType;
        public string dismissedPlayerName;
        public string fielderName;
        public bool rotateStrike;
        public string commentary;

        public int TotalRuns
        {
            get { return runsBat + runsExtra; }
        }

        public UnifiedDeliveryResult()
        {
            releaseSpeed = 0f;
            deliveryType = "Standard";
            shotType = "Defense";
            shotTiming = "Good";
            runsBat = 0;
            runsExtra = 0;
            isLegalBall = true;
            isBoundaryFour = false;
            isBoundarySix = false;
            isWicket = false;
            wicketType = string.Empty;
            dismissedPlayerName = string.Empty;
            fielderName = string.Empty;
            rotateStrike = false;
            commentary = "Dot ball";
        }

        public static UnifiedDeliveryResult Dot()
        {
            UnifiedDeliveryResult res = new UnifiedDeliveryResult();
            res.runsBat = 0;
            res.commentary = "No run, played straight to the fielder.";
            return res;
        }

        public static UnifiedDeliveryResult Runs(int r)
        {
            UnifiedDeliveryResult res = new UnifiedDeliveryResult();
            res.runsBat = r;
            res.rotateStrike = (r % 2 == 1);
            if (r == 4)
            {
                res.isBoundaryFour = true;
                res.commentary = "FOUR! Cracking shot racing across the turf to the fence!";
            }
            else if (r == 6)
            {
                res.isBoundarySix = true;
                res.commentary = "SIX! High, handsome and deep into the stands!";
            }
            else
            {
                res.commentary = string.Format("{0} run(s) taken smartly between the wickets.", r);
            }
            return res;
        }

        public static UnifiedDeliveryResult Wide(int extraRuns)
        {
            UnifiedDeliveryResult res = new UnifiedDeliveryResult();
            res.runsExtra = extraRuns;
            res.isLegalBall = false;
            res.commentary = "Wide ball signaled by the umpire.";
            return res;
        }

        public static UnifiedDeliveryResult NoBall(int extraRuns)
        {
            UnifiedDeliveryResult res = new UnifiedDeliveryResult();
            res.runsExtra = extraRuns;
            res.isLegalBall = false;
            res.commentary = "No ball! Free hit coming up!";
            return res;
        }

        public static UnifiedDeliveryResult Wicket(string type, string batterName, string fielder)
        {
            UnifiedDeliveryResult res = new UnifiedDeliveryResult();
            res.isWicket = true;
            res.wicketType = type;
            res.dismissedPlayerName = batterName;
            res.fielderName = fielder;
            res.commentary = string.Format("OUT! {0} departs ({1})!", batterName, type);
            return res;
        }
    }
}
