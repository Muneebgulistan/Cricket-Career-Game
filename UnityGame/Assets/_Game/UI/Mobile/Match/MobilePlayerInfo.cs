using System;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI.Mobile.Match
{
    public class MobilePlayerInfo : MonoBehaviour
    {
        [Header("Striker")]
        [SerializeField] private Text strikerNameText;
        [SerializeField] private Text strikerScoreText;

        [Header("Non-Striker")]
        [SerializeField] private Text nonStrikerNameText;
        [SerializeField] private Text nonStrikerScoreText;

        [Header("Bowler")]
        [SerializeField] private Text bowlerNameText;
        [SerializeField] private Text bowlerFiguresText;

        public void UpdateStriker(string name, int runs, int balls, int fours, int sixes, float sr)
        {
            if (strikerNameText != null) strikerNameText.text = string.Format("{0}*", name);
            if (strikerScoreText != null) strikerScoreText.text = string.Format("{0} ({1}) [4s:{2} 6s:{3}] SR:{4:F1}", runs, balls, fours, sixes, sr);
        }

        public void UpdateNonStriker(string name, int runs, int balls)
        {
            if (nonStrikerNameText != null) nonStrikerNameText.text = name;
            if (nonStrikerScoreText != null) nonStrikerScoreText.text = string.Format("{0} ({1})", runs, balls);
        }

        public void UpdateBowler(string name, string figures, float economy)
        {
            if (bowlerNameText != null) bowlerNameText.text = name;
            if (bowlerFiguresText != null) bowlerFiguresText.text = string.Format("{0} (Econ: {1:F2})", figures, economy);
        }
    }
}
