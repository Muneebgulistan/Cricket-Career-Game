using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Batting;

namespace CricketGame.UI.Mobile.Batting
{
    public class BattingTimingMeter : MonoBehaviour
    {
        [Header("Meter UI Elements")]
        [SerializeField] private RectTransform meterBar;
        [SerializeField] private RectTransform timingMarker;
        [SerializeField] private RectTransform perfectZone;
        [SerializeField] private RectTransform goodZone;
        [SerializeField] private Text feedbackLabel;

        [Header("Timing Thresholds")]
        [SerializeField] private float idealContactProgress = 0.5f; // Center of meter
        [SerializeField] private float perfectWindowProgress = 0.06f; // +/- 6%
        [SerializeField] private float goodWindowProgress = 0.16f;    // +/- 16%
        [SerializeField] private float validWindowProgress = 0.30f;   // +/- 30%

        private float currentProgress = 0f;
        private TimingQuality lastQuality = TimingQuality.Miss;
        private BattingTiming timingEvaluator;

        public float CurrentProgress { get { return currentProgress; } }
        public TimingQuality LastQuality { get { return lastQuality; } }

        public event Action<TimingQuality> OnTimingEvaluated;

        private void Awake()
        {
            timingEvaluator = new BattingTiming();
            ResetMeter();
        }

        public void SetThresholds(float ideal, float perfectWin, float goodWin, float validWin)
        {
            idealContactProgress = ideal;
            perfectWindowProgress = perfectWin;
            goodWindowProgress = goodWin;
            validWindowProgress = validWin;
        }

        public void UpdateProgress(float progress01)
        {
            currentProgress = Mathf.Clamp01(progress01);

            if (timingMarker != null && meterBar != null)
            {
                float barWidth = meterBar.rect.width > 0f ? meterBar.rect.width : 200f;
                float xPos = (currentProgress - 0.5f) * barWidth;
                timingMarker.anchoredPosition = new Vector2(xPos, timingMarker.anchoredPosition.y);
            }
        }

        public TimingQuality EvaluateSwingAtCurrentProgress()
        {
            return EvaluateSwing(currentProgress);
        }

        public TimingQuality EvaluateSwing(float progress01)
        {
            float delta = progress01 - idealContactProgress;
            float absDelta = Mathf.Abs(delta);

            if (absDelta <= perfectWindowProgress)
            {
                lastQuality = TimingQuality.Perfect;
            }
            else if (absDelta <= goodWindowProgress)
            {
                lastQuality = TimingQuality.Good;
            }
            else if (absDelta <= validWindowProgress)
            {
                lastQuality = delta < 0f ? TimingQuality.Early : TimingQuality.Late;
            }
            else
            {
                lastQuality = TimingQuality.Miss;
            }

            UpdateFeedbackDisplay(lastQuality);

            if (OnTimingEvaluated != null)
            {
                OnTimingEvaluated(lastQuality);
            }

            return lastQuality;
        }

        private void UpdateFeedbackDisplay(TimingQuality quality)
        {
            if (feedbackLabel == null) return;

            switch (quality)
            {
                case TimingQuality.Perfect:
                    feedbackLabel.text = "PERFECT!";
                    feedbackLabel.color = Color.green;
                    break;
                case TimingQuality.Good:
                    feedbackLabel.text = "GOOD";
                    feedbackLabel.color = Color.cyan;
                    break;
                case TimingQuality.Early:
                    feedbackLabel.text = "EARLY";
                    feedbackLabel.color = Color.yellow;
                    break;
                case TimingQuality.Late:
                    feedbackLabel.text = "LATE";
                    feedbackLabel.color = new Color(1f, 0.5f, 0f); // Orange
                    break;
                case TimingQuality.Miss:
                    feedbackLabel.text = "MISS";
                    feedbackLabel.color = Color.red;
                    break;
            }
        }

        public void ResetMeter()
        {
            currentProgress = 0f;
            lastQuality = TimingQuality.Miss;
            if (timingMarker != null && meterBar != null)
            {
                float barWidth = meterBar.rect.width > 0f ? meterBar.rect.width : 200f;
                timingMarker.anchoredPosition = new Vector2(-barWidth * 0.5f, timingMarker.anchoredPosition.y);
            }
            if (feedbackLabel != null)
            {
                feedbackLabel.text = "";
            }
        }
    }
}
