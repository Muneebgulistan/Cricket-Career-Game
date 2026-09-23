using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI.Match
{
    public class OverSummaryWidget : MonoBehaviour
    {
        [Header("Over Summary Ball Text")]
        [SerializeField] private Text overBallsText;

        public void UpdateDisplay(List<string> balls)
        {
            if (overBallsText == null) return;

            if (balls == null || balls.Count == 0)
            {
                overBallsText.text = "This Over: -";
                return;
            }

            string result = "This Over: " + string.Join(" ", balls.ToArray());
            overBallsText.text = result;
        }
    }
}
