using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Bowling
{
    public class BowlingSwingControl : MonoBehaviour
    {
        [SerializeField] private Slider swingSlider;
        [SerializeField] private Text swingLabel;
        private float currentSwing = 0f;

        public float CurrentSwing { get { return currentSwing; } }
        public event Action<float> OnSwingChanged;

        private void Awake()
        {
            if (swingSlider == null) swingSlider = GetComponent<Slider>();
            if (swingSlider != null)
            {
                swingSlider.minValue = -1.0f;
                swingSlider.maxValue = 1.0f;
                swingSlider.value = currentSwing;
            }
        }

        public void SetSwingValue(float value)
        {
            currentSwing = Mathf.Clamp(value, -1.0f, 1.0f);
            if (swingSlider != null)
            {
                swingSlider.value = currentSwing;
            }
            if (swingLabel != null)
            {
                if (currentSwing < -0.1f) swingLabel.text = string.Format("In-Swing: {0:F1}", -currentSwing);
                else if (currentSwing > 0.1f) swingLabel.text = string.Format("Out-Swing: {0:F1}", currentSwing);
                else swingLabel.text = "Straight";
            }
            if (OnSwingChanged != null)
            {
                OnSwingChanged(currentSwing);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBowlingSwing(currentSwing);
            }
        }
    }
}
