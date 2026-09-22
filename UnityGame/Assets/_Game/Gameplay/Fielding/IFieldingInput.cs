using UnityEngine;

namespace CricketGame.Gameplay.Fielding
{
    public interface IFieldingInput
    {
        bool IsFieldingActive { get; }
        Vector2 MoveDirection { get; }
        bool WantsSprint { get; }
        bool IsPickupRequested { get; }
        bool IsCatchRequested { get; }
        bool IsThrowRequested { get; }
        FieldingTarget SelectedThrowTarget { get; }

        void MoveFieldPlayer(Vector2 direction, bool sprint);
        void AttemptPickup();
        void AttemptCatch();
        void SelectThrowTarget(FieldingTarget target);
        void TriggerThrow();
        void ActivateFielding(bool activate);
        void ConsumePickupRequest();
        void ConsumeCatchRequest();
        void ConsumeThrowRequest();
        void ResetInput();
    }
}
