using UnityEngine;

namespace CricketGame.Gameplay.Batting
{
    public interface IBattingInput
    {
        bool IsSwingRequested { get; }
        BattingShotType RequestedShotType { get; }
        Vector2 DirectionInput { get; }
        bool IsLofted { get; }
        bool IsDefensive { get; }

        void PollInput();
        void ConsumeSwingRequest();

        // Mobile / External API hooks
        void TriggerShot(BattingShotType shotType, Vector2 direction, bool isLofted);
        void SetShotDirection(Vector2 direction);
        void SetShotModifier(bool isLofted, bool isDefensive);
        void TriggerDefensiveShot();
        void RequestSwing();
    }
}
