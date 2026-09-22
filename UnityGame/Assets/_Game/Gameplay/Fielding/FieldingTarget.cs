using UnityEngine;
using CricketGame.Fielding;

namespace CricketGame.Gameplay.Fielding
{
    public enum FieldingTarget
    {
        WicketKeeper,
        Bowler,
        StrikerEnd,
        NonStrikerEnd,
        NearestWicket
    }

    public static class FieldingTargetUtility
    {
        public static Vector3 GetTargetWorldPosition(FieldingTarget target)
        {
            if (CricketPositionManager.Instance != null)
            {
                switch (target)
                {
                    case FieldingTarget.WicketKeeper:
                    case FieldingTarget.StrikerEnd:
                        if (CricketPositionManager.Instance.wicketKeeperPosition != null)
                        {
                            return CricketPositionManager.Instance.wicketKeeperPosition.position;
                        }
                        if (CricketPositionManager.Instance.strikerPosition != null)
                        {
                            return CricketPositionManager.Instance.strikerPosition.position;
                        }
                        break;
                    case FieldingTarget.Bowler:
                    case FieldingTarget.NonStrikerEnd:
                        if (CricketPositionManager.Instance.bowlerReleasePosition != null)
                        {
                            return CricketPositionManager.Instance.bowlerReleasePosition.position;
                        }
                        if (CricketPositionManager.Instance.nonStrikerPosition != null)
                        {
                            return CricketPositionManager.Instance.nonStrikerPosition.position;
                        }
                        break;
                }
            }

            // Defaults: Striker crease at z=10.06m, Non-striker crease at z=-10.06m
            switch (target)
            {
                case FieldingTarget.WicketKeeper:
                    return new Vector3(0f, 0.7f, 15.0f);
                case FieldingTarget.StrikerEnd:
                    return new Vector3(0f, 0.71f, 10.06f);
                case FieldingTarget.Bowler:
                    return new Vector3(0f, 0.7f, -10.5f);
                case FieldingTarget.NonStrikerEnd:
                    return new Vector3(0f, 0.71f, -10.06f);
                default:
                    return new Vector3(0f, 0.71f, 10.06f);
            }
        }
    }
}
