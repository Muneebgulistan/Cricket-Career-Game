using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Batting;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Batting
{
    public class MobileBattingHUD : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject hudContainer;

        [Header("Left Controls")]
        [SerializeField] private BattingDirectionControl directionControl;

        [Header("Right Shot Buttons")]
        [SerializeField] private Button defensiveButton;
        [SerializeField] private Button driveButton;
        [SerializeField] private Button cutButton;
        [SerializeField] private Button pullButton;
        [SerializeField] private Button hookButton;
        [SerializeField] private Button sweepButton;
        [SerializeField] private Button loftButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button swingButton;

        [Header("Timing Meter")]
        [SerializeField] private BattingTimingMeter timingMeter;

        [Header("Power Control")]
        [SerializeField] private BattingPowerControl powerControl;

        private bool isLoftedToggle = false;

        public BattingTimingMeter TimingMeter { get { return timingMeter; } }
        public BattingDirectionControl DirectionControl { get { return directionControl; } }

        private void Awake()
        {
            SetupButtonListeners();
        }

        private void SetupButtonListeners()
        {
            if (defensiveButton != null) defensiveButton.onClick.AddListener(OnDefensiveClicked);
            if (driveButton != null) driveButton.onClick.AddListener(OnDriveClicked);
            if (cutButton != null) cutButton.onClick.AddListener(OnCutClicked);
            if (pullButton != null) pullButton.onClick.AddListener(OnPullClicked);
            if (hookButton != null) hookButton.onClick.AddListener(OnHookClicked);
            if (sweepButton != null) sweepButton.onClick.AddListener(OnSweepClicked);
            if (loftButton != null) loftButton.onClick.AddListener(OnLoftToggleClicked);
            if (leaveButton != null) leaveButton.onClick.AddListener(OnLeaveClicked);
            if (swingButton != null) swingButton.onClick.AddListener(OnSwingClicked);
        }

        public void SetVisible(bool visible)
        {
            if (hudContainer != null)
            {
                hudContainer.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        public void OnDefensiveClicked()
        {
            SendShot(BattingShotType.Defensive, false);
        }

        public void OnDriveClicked()
        {
            BattingShotType type = isLoftedToggle ? BattingShotType.LoftedDrive : BattingShotType.StraightDrive;
            SendShot(type, isLoftedToggle);
        }

        public void OnCutClicked()
        {
            SendShot(BattingShotType.Cut, isLoftedToggle);
        }

        public void OnPullClicked()
        {
            SendShot(BattingShotType.Pull, isLoftedToggle);
        }

        public void OnHookClicked()
        {
            SendShot(BattingShotType.Hook, true);
        }

        public void OnSweepClicked()
        {
            SendShot(BattingShotType.Sweep, false);
        }

        public void OnLoftToggleClicked()
        {
            isLoftedToggle = !isLoftedToggle;
            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBattingShotSelection(
                    isLoftedToggle ? BattingShotType.StraightLoft : BattingShotType.StraightDrive,
                    isLoftedToggle);
            }
        }

        public void OnLeaveClicked()
        {
            // Do not swing / leave ball
            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBattingShotSelection(BattingShotType.Defensive, false);
            }
        }

        public void OnSwingClicked()
        {
            if (timingMeter != null)
            {
                timingMeter.EvaluateSwingAtCurrentProgress();
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBattingSwing();
            }
        }

        private void SendShot(BattingShotType type, bool lofted)
        {
            Vector2 dir = directionControl != null ? directionControl.CurrentDirection : Vector2.zero;
            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBattingShotSelection(type, lofted);
            }
        }
    }
}
