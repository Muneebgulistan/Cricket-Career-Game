using UnityEngine;

namespace CricketGame.Fielding
{
    public enum FieldingPreset
    {
        Attacking,
        Balanced,
        Defensive
    }

    public enum ThrowTarget
    {
        KeeperEnd,
        BowlerEnd,
        DirectHit
    }

    public class FieldingController : MonoBehaviour
    {
        public FieldingPreset currentPreset = FieldingPreset.Balanced;

        public void ApplyPreset(FieldingPreset preset)
        {
            currentPreset = preset;
            CricketGame.Core.CricketLogger.Log($"[FieldingController] Applied fielding preset: {preset}");
        }

        public void ThrowBall(ThrowTarget target, float accuracy)
        {
            CricketGame.Core.CricketLogger.Log($"[FieldingController] Fielder threw ball to {target} with accuracy {accuracy:P0}");
        }
    }
}
