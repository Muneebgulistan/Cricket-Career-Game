using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Cricket;

namespace CricketGame.UI.Mobile.Match
{
    public class MobileMatchResult : MonoBehaviour
    {
        [Header("Result Dialog")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private Text winnerTitleText;
        [SerializeField] private Text resultMarginText;
        [SerializeField] private Text playerStatsSummaryText;
        [SerializeField] private Button continueButton;

        public event Action OnContinueClicked;

        private void Awake()
        {
            if (dialogPanel != null) dialogPanel.SetActive(false);
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinue);
            }
        }

        public void ShowResult(CricketGame.Cricket.MatchResult result)
        {
            if (dialogPanel != null) dialogPanel.SetActive(true);

            if (result == null) return;

            if (winnerTitleText != null)
            {
                winnerTitleText.text = string.Format("{0} VICTORY!", result.winnerTeamName.ToUpper());
            }

            if (resultMarginText != null)
            {
                string outcome = result.isPlayerVictory ? "Match Won!" : "Match Lost";
                resultMarginText.text = outcome;
            }

            if (playerStatsSummaryText != null && result.userPerformance != null)
            {
                playerStatsSummaryText.text = string.Format(
                    "Your Performance:\nRuns: {0} ({1})\n4s: {2} | 6s: {3}\nRating: {4:F1}/10.0",
                    result.userPerformance.runs,
                    result.userPerformance.balls,
                    result.userPerformance.fours,
                    result.userPerformance.sixes,
                    result.userPerformance.matchRating
                );
            }
        }

        private void HandleContinue()
        {
            if (dialogPanel != null) dialogPanel.SetActive(false);
            if (OnContinueClicked != null)
            {
                OnContinueClicked();
            }
        }
    }
}
