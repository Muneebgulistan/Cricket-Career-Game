using UnityEngine;

namespace CricketGame.Gameplay.Bowling
{
    public class BowlingInput : MonoBehaviour, IBowlingInput
    {
        [Header("State")]
        [SerializeField] private bool deliveryRequested = false;
        [SerializeField] private BowlingBaseType selectedType = BowlingBaseType.Fast;
        [SerializeField] private BowlingLength selectedLength = BowlingLength.GoodLength;
        [SerializeField] private BowlingLine selectedLine = BowlingLine.OffStump;
        [SerializeField] private float swingAmount = 0f;
        [SerializeField] private float spinAmount = 0f;
        [SerializeField] private float accuracyMeter = 0.85f;

        public bool IsDeliveryRequested { get { return deliveryRequested; } }
        public BowlingBaseType SelectedType { get { return selectedType; } }
        public BowlingLength SelectedLength { get { return selectedLength; } }
        public BowlingLine SelectedLine { get { return selectedLine; } }
        public float SwingAmount { get { return swingAmount; } }
        public float SpinAmount { get { return spinAmount; } }
        public float AccuracyMeter { get { return accuracyMeter; } }

        public void ConsumeDeliveryRequest()
        {
            deliveryRequested = false;
        }

        public void PollInput()
        {
            // Base types (1 - 4)
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1)) SelectDelivery(BowlingBaseType.Fast);
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2)) SelectDelivery(BowlingBaseType.Medium);
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3)) SelectDelivery(BowlingBaseType.OffSpin);
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4)) SelectDelivery(BowlingBaseType.LegSpin);

            // Length variations (Q, W, E, R, T)
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q)) SetTargetLength(BowlingLength.Yorker);
            if (UnityEngine.Input.GetKeyDown(KeyCode.W)) SetTargetLength(BowlingLength.Full);
            if (UnityEngine.Input.GetKeyDown(KeyCode.E)) SetTargetLength(BowlingLength.GoodLength);
            if (UnityEngine.Input.GetKeyDown(KeyCode.R)) SetTargetLength(BowlingLength.Short);
            if (UnityEngine.Input.GetKeyDown(KeyCode.T)) SetTargetLength(BowlingLength.Bouncer);

            // Line variations (Left/Right Arrows or A/D)
            if (UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                // Inswing / move line in
                swingAmount = Mathf.Clamp(swingAmount - 0.35f, -1.0f, 1.0f);
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                // Outswing / move line out
                swingAmount = Mathf.Clamp(swingAmount + 0.35f, -1.0f, 1.0f);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int lineIdx = Mathf.Max(0, (int)selectedLine - 1);
                selectedLine = (BowlingLine)lineIdx;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
            {
                int lineIdx = Mathf.Min(4, (int)selectedLine + 1);
                selectedLine = (BowlingLine)lineIdx;
            }

            // Start delivery
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                TriggerDelivery();
            }
        }

        // ----------------------------------------------------
        // Mobile / External API
        // ----------------------------------------------------
        public void SelectDelivery(BowlingBaseType baseType)
        {
            selectedType = baseType;
            if (baseType == BowlingBaseType.OffSpin)
            {
                spinAmount = -4.5f;
                swingAmount = -0.1f;
            }
            else if (baseType == BowlingBaseType.LegSpin)
            {
                spinAmount = 5.2f;
                swingAmount = 0.1f;
            }
            else
            {
                spinAmount = 0f;
            }
        }

        public void SetTargetLine(BowlingLine line)
        {
            selectedLine = line;
        }

        public void SetTargetLength(BowlingLength length)
        {
            selectedLength = length;
        }

        public void SetPower(float power01)
        {
            accuracyMeter = Mathf.Clamp01(power01);
        }

        public void SetSwing(float swing)
        {
            swingAmount = Mathf.Clamp(swing, -1.0f, 1.0f);
        }

        public void SetSpin(float spin)
        {
            spinAmount = Mathf.Clamp(spin, -8.0f, 8.0f);
        }

        public void TriggerDelivery()
        {
            deliveryRequested = true;
        }
    }
}
