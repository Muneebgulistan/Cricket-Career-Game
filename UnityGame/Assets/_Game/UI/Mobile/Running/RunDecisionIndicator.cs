using System;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI.Mobile.Running
{
    public enum RunSafetyLevel
    {
        Safe,
        Tight,
        Danger
    }

    public class RunDecisionIndicator : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Image statusIcon;

        private RunSafetyLevel currentLevel = RunSafetyLevel.Safe;

        public RunSafetyLevel CurrentLevel { get { return currentLevel; } }

        public void UpdateSafetyMargin(float marginSeconds)
        {
            if (marginSeconds > 0.8f)
            {
                SetStatus(RunSafetyLevel.Safe, "SAFE RUN", Color.green);
            }
            else if (marginSeconds > 0.0f)
            {
                SetStatus(RunSafetyLevel.Tight, "TIGHT RUN!", Color.yellow);
            }
            else
            {
                SetStatus(RunSafetyLevel.Danger, "RUN OUT RISK!", Color.red);
            }
        }

        public void SetStatus(RunSafetyLevel level, string message, Color color)
        {
            currentLevel = level;
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
            if (statusIcon != null)
            {
                statusIcon.color = color;
            }
        }
    }
}
