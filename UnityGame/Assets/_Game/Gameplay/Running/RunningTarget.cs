using System;
using UnityEngine;

namespace CricketGame.Gameplay.Running
{
    public static class RunningTarget
    {
        public const float StrikerCreaseZ = 8.84f;
        public const float NonStrikerCreaseZ = -8.84f;
        public const float CreaseDistance = 17.68f;
        public const float CreaseSafetyThreshold = 0.45f;
        public const float PitchLaneXOffset = 0.8f;

        public static Vector3 GetCreasePosition(CreaseEnd end, bool isStrikerLane)
        {
            float x = isStrikerLane ? -PitchLaneXOffset : PitchLaneXOffset;
            float z = end == CreaseEnd.StrikerEnd ? StrikerCreaseZ : NonStrikerCreaseZ;
            return new Vector3(x, 0f, z);
        }

        public static bool IsPastCrease(Vector3 position, CreaseEnd destinationEnd)
        {
            if (destinationEnd == CreaseEnd.StrikerEnd)
            {
                return position.z >= (StrikerCreaseZ - CreaseSafetyThreshold);
            }
            else
            {
                return position.z <= (NonStrikerCreaseZ + CreaseSafetyThreshold);
            }
        }

        public static float DistanceToCrease(Vector3 position, CreaseEnd destinationEnd)
        {
            float targetZ = destinationEnd == CreaseEnd.StrikerEnd ? StrikerCreaseZ : NonStrikerCreaseZ;
            return Mathf.Abs(position.z - targetZ);
        }
    }
}
