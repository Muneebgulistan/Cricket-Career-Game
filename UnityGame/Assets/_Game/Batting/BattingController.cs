using UnityEngine;
using CricketGame.Cricket;

namespace CricketGame.Batting
{
    public enum ShotDirection
    {
        OffSide,
        Straight,
        LegSide
    }

    public enum ShotType
    {
        Defensive,
        Drive,
        Cut,
        Pull,
        Sweep,
        Lofted
    }

    public class BattingController : MonoBehaviour
    {
        [Header("Batting State")]
        public BattingStyle stance = BattingStyle.RightHand;
        public ShotDirection selectedDirection = ShotDirection.Straight;
        public ShotType selectedShot = ShotType.Drive;

        public void PrepareStance()
        {
            Debug.Log($"[BattingController] Batter set in {stance} stance.");
        }

        public void ExecuteShot(ShotDirection dir, ShotType shot, float timingQuality)
        {
            selectedDirection = dir;
            selectedShot = shot;
            Debug.Log($"[BattingController] Executed {shot} towards {dir} with timing {timingQuality:P0}");
        }
    }
}
