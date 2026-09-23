using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.UI.Match
{
    public class BatterScoreWidget : MonoBehaviour
    {
        [Header("Striker & Non-Striker Text Elements")]
        [SerializeField] private Text strikerText;
        [SerializeField] private Text nonStrikerText;

        public void UpdateDisplay(BatterScore striker, BatterScore nonStriker)
        {
            if (strikerText != null)
            {
                if (striker != null)
                {
                    strikerText.text = string.Format("{0}*  {1} ({2})  4s: {3}  6s: {4}  SR: {5:F1}",
                        striker.playerName,
                        striker.runs,
                        striker.balls,
                        striker.fours,
                        striker.sixes,
                        striker.StrikeRate);
                }
                else
                {
                    strikerText.text = "Striker -";
                }
            }

            if (nonStrikerText != null)
            {
                if (nonStriker != null)
                {
                    nonStrikerText.text = string.Format("{0}  {1} ({2})  4s: {3}  6s: {4}  SR: {5:F1}",
                        nonStriker.playerName,
                        nonStriker.runs,
                        nonStriker.balls,
                        nonStriker.fours,
                        nonStriker.sixes,
                        nonStriker.StrikeRate);
                }
                else
                {
                    nonStrikerText.text = "Non-striker -";
                }
            }
        }
    }
}
