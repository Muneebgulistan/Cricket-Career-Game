using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Bowling
{
    public class BowlingPowerMeter : MonoBehaviour
    {
        [SerializeField] private Slider meterSlider;
        [SerializeField] private RectTransform idealReleaseMarker;
        [SerializeField] private float speed = 2.0f;

        private float currentPower = 0.5f;
        private bool isCharging = false;
        private float direction = 1.0f;

        public float CurrentPower { get { return currentPower; } }
        public bool IsCharging { get { return isCharging; } }

        public event Action<float> OnPowerLocked;

        private void Awake()
        {
            if (meterSlider == null) meterSlider = GetComponent<Slider>();
            if (meterSlider != null)
            {
                meterSlider.minValue = 0f;
                meterSlider.maxValue = 1f;
                meterSlider.value = currentPower;
            }
        }

        private void Update()
        {
            if (isCharging)
            {
                currentPower += direction * speed * Time.deltaTime;
                if (currentPower >= 1f)
                {
                    currentPower = 1f;
                    direction = -1.0f;
                }
                else if (currentPower <= 0f)
                {
                    currentPower = 0f;
                    direction = 1.0f;
                }

                if (meterSlider != null)
                {
                    meterSlider.value = currentPower;
                }
            }
        }

        public void StartCharging()
        {
            isCharging = true;
            currentPower = 0f;
            direction = 1.0f;
        }

        public float StopChargingAndLock()
        {
            isCharging = false;
            if (OnPowerLocked != null)
            {
                OnPowerLocked(currentPower);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBowlingPower(currentPower);
            }

            return currentPower;
        }

        public void SetPowerDirect(float power01)
        {
            currentPower = Mathf.Clamp01(power01);
            if (meterSlider != null)
            {
                meterSlider.value = currentPower;
            }
            if (OnPowerLocked != null)
            {
                OnPowerLocked(currentPower);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBowlingPower(currentPower);
            }
        }
    }
}
