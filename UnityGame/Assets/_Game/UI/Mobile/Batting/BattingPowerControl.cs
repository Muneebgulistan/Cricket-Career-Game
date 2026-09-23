using System;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI.Mobile.Batting
{
    public class BattingPowerControl : MonoBehaviour
    {
        [SerializeField] private Slider powerSlider;
        [SerializeField] private float powerValue = 1.0f;

        public float PowerValue { get { return powerValue; } }
        public event Action<float> OnPowerChanged;

        private void Awake()
        {
            if (powerSlider == null) powerSlider = GetComponent<Slider>();
            if (powerSlider != null)
            {
                powerSlider.minValue = 0.2f;
                powerSlider.maxValue = 1.0f;
                powerSlider.value = powerValue;
            }
        }

        public void SetPower(float power01)
        {
            powerValue = Mathf.Clamp(power01, 0.2f, 1.0f);
            if (powerSlider != null)
            {
                powerSlider.value = powerValue;
            }
            if (OnPowerChanged != null)
            {
                OnPowerChanged(powerValue);
            }
        }
    }
}
