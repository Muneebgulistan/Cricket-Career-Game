using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI.Mobile.Match
{
    public class MobileOverDisplay : MonoBehaviour
    {
        [SerializeField] private Text overSummaryText;
        private List<string> currentOverBalls = new List<string>();

        public void AddBallOutcome(string outcome)
        {
            currentOverBalls.Add(outcome);
            RefreshDisplay();
        }

        public void ResetOver()
        {
            currentOverBalls.Clear();
            RefreshDisplay();
        }

        public void SetBalls(List<string> balls)
        {
            currentOverBalls = balls != null ? new List<string>(balls) : new List<string>();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (overSummaryText == null) return;

            if (currentOverBalls.Count == 0)
            {
                overSummaryText.text = "This Over: -";
                return;
            }

            string result = "This Over: ";
            for (int i = 0; i < currentOverBalls.Count; i++)
            {
                result += string.Format("[{0}] ", currentOverBalls[i]);
            }
            overSummaryText.text = result.TrimEnd();
        }
    }
}
