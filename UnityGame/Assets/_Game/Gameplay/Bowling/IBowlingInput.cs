using UnityEngine;

namespace CricketGame.Gameplay.Bowling
{
    public interface IBowlingInput
    {
        bool IsDeliveryRequested { get; }
        BowlingBaseType SelectedType { get; }
        BowlingLength SelectedLength { get; }
        BowlingLine SelectedLine { get; }
        float SwingAmount { get; }
        float SpinAmount { get; }
        float AccuracyMeter { get; }

        void PollInput();
        void ConsumeDeliveryRequest();

        // Mobile / External API
        void SelectDelivery(BowlingBaseType baseType);
        void SetTargetLine(BowlingLine line);
        void SetTargetLength(BowlingLength length);
        void SetPower(float power01);
        void SetSwing(float swingAmount);
        void SetSpin(float spinAmount);
        void TriggerDelivery();
    }
}
