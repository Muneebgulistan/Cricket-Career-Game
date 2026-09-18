using UnityEngine;
using CricketGame.Cricket;

namespace CricketGame.Bowling
{
    public enum DeliveryLine
    {
        OutsideOff,
        Stumps,
        LegSide
    }

    public enum DeliveryLength
    {
        Yorker,
        Full,
        GoodLength,
        Short
    }

    public class BowlingController : MonoBehaviour
    {
        [Header("Bowling Attributes")]
        public BowlingStyle style = BowlingStyle.RightArmFast;
        public DeliveryLine targetLine = DeliveryLine.OutsideOff;
        public DeliveryLength targetLength = DeliveryLength.GoodLength;
        public float paceEffortPercentage = 95f;

        public void BowlDelivery(float accuracyMeterValue)
        {
            Debug.Log($"[BowlingController] Bowled delivery with accuracy {accuracyMeterValue:P0} at {targetLength} on {targetLine}");
        }
    }
}
