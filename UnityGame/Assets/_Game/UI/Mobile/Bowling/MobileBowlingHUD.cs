using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Bowling
{
    public class MobileBowlingHUD : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject hudContainer;

        [Header("Controls")]
        [SerializeField] private BowlingDeliverySelector deliverySelector;
        [SerializeField] private BowlingLineControl lineControl;
        [SerializeField] private BowlingLengthControl lengthControl;
        [SerializeField] private BowlingSwingControl swingControl;
        [SerializeField] private BowlingPowerMeter powerMeter;
        [SerializeField] private Button bowlButton;

        public BowlingDeliverySelector DeliverySelector { get { return deliverySelector; } }
        public BowlingLineControl LineControl { get { return lineControl; } }
        public BowlingLengthControl LengthControl { get { return lengthControl; } }
        public BowlingSwingControl SwingControl { get { return swingControl; } }
        public BowlingPowerMeter PowerMeter { get { return powerMeter; } }

        private void Awake()
        {
            if (bowlButton != null)
            {
                bowlButton.onClick.AddListener(OnBowlClicked);
            }
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

        public void OnBowlClicked()
        {
            if (powerMeter != null && powerMeter.IsCharging)
            {
                powerMeter.StopChargingAndLock();
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBowlingTriggerDelivery();
            }
        }
    }
}
