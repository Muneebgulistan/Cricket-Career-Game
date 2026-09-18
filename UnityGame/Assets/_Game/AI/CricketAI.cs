using UnityEngine;
using CricketGame.Batting;
using CricketGame.Bowling;

namespace CricketGame.AI
{
    public class CricketAI : MonoBehaviour
    {
        public ShotType DecideAIShot(DeliveryLength length, DeliveryLine line, bool isPowerplay)
        {
            if (isPowerplay && length == DeliveryLength.Full)
            {
                return ShotType.Lofted;
            }

            if (length == DeliveryLength.Short)
            {
                return ShotType.Pull;
            }

            return ShotType.Drive;
        }

        public void DecideAIBowling(out DeliveryLength length, out DeliveryLine line, bool deathOvers)
        {
            if (deathOvers)
            {
                length = DeliveryLength.Yorker;
                line = DeliveryLine.OutsideOff;
            }
            else
            {
                length = DeliveryLength.GoodLength;
                line = DeliveryLine.Stumps;
            }
        }
    }
}
