using System;
using UnityEngine;

namespace CricketGame.Gameplay.Batting
{
    public enum TimingQuality
    {
        Perfect,
        Good,
        Early,
        Late,
        Miss
    }

    [Serializable]
    public class BattingTiming
    {
        [Header("Timing Thresholds (seconds)")]
        public float perfectWindow = 0.05f; // within +/- 0.05s is Perfect
        public float goodWindow = 0.12f;    // within +/- 0.12s is Good
        public float validWindow = 0.25f;   // within +/- 0.25s is Early or Late; beyond is Miss

        public TimingQuality EvaluateTiming(float actualSwingTime, float idealContactTime, out float timeDelta)
        {
            timeDelta = actualSwingTime - idealContactTime;
            float absDelta = Mathf.Abs(timeDelta);

            if (absDelta <= perfectWindow)
            {
                return TimingQuality.Perfect;
            }
            if (absDelta <= goodWindow)
            {
                return TimingQuality.Good;
            }
            if (absDelta <= validWindow)
            {
                return timeDelta < 0f ? TimingQuality.Early : TimingQuality.Late;
            }

            return TimingQuality.Miss;
        }

        public float GetTimingMultiplier(TimingQuality quality)
        {
            switch (quality)
            {
                case TimingQuality.Perfect:
                    return 1.0f;
                case TimingQuality.Good:
                    return 0.85f;
                case TimingQuality.Early:
                    return 0.60f;
                case TimingQuality.Late:
                    return 0.55f;
                default:
                    return 0.1f;
            }
        }
    }
}
