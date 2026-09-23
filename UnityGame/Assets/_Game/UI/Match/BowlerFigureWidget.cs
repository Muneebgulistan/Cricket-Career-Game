using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.UI.Match
{
    public class BowlerFigureWidget : MonoBehaviour
    {
        [Header("Bowler Stats Elements")]
        [SerializeField] private Text bowlerNameText;
        [SerializeField] private Text bowlerFiguresText;
        [SerializeField] private Text bowlerEconomyText;

        public void UpdateDisplay(BowlerFigures bowler)
        {
            if (bowler == null)
            {
                if (bowlerNameText != null) bowlerNameText.text = "Bowler -";
                if (bowlerFiguresText != null) bowlerFiguresText.text = "0.0-0-0-0";
                if (bowlerEconomyText != null) bowlerEconomyText.text = "Econ: 0.00";
                return;
            }

            if (bowlerNameText != null)
            {
                bowlerNameText.text = bowler.playerName;
            }

            if (bowlerFiguresText != null)
            {
                bowlerFiguresText.text = bowler.FiguresFormatted;
            }

            if (bowlerEconomyText != null)
            {
                bowlerEconomyText.text = string.Format("Econ: {0:F2}", bowler.Economy);
            }
        }
    }
}
