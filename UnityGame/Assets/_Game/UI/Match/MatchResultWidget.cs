using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Cricket;

namespace CricketGame.UI.Match
{
    public class MatchResultWidget : MonoBehaviour
    {
        [Header("Result Elements")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text winnerTitleText;
        [SerializeField] private Text resultDescriptionText;
        [SerializeField] private Text scorecardSummaryText;
        [SerializeField] private Button continueButton;

        public event Action OnContinueClicked;

        private void Start()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinueClicked);
            }
        }

        private void HandleContinueClicked()
        {
            if (OnContinueClicked != null)
            {
                OnContinueClicked();
            }
        }

        public void ShowResult(CricketGame.Cricket.MatchResult result)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (result == null) return;

            if (winnerTitleText != null)
            {
                winnerTitleText.text = string.Format("{0} WON!", result.winnerTeamName.ToUpper());
            }

            if (resultDescriptionText != null)
            {
                resultDescriptionText.text = result.resultDescription;
            }

            if (scorecardSummaryText != null)
            {
                scorecardSummaryText.text = string.Format("{0}: {1}/{2} ({3:F1} ov)\n{4}: {5}/{6} ({7:F1} ov)",
                    result.firstInnings.battingTeamName,
                    result.firstInnings.totalRuns,
                    result.firstInnings.wicketsLost,
                    result.firstInnings.oversCompleted,
                    result.secondInnings.battingTeamName,
                    result.secondInnings.totalRuns,
                    result.secondInnings.wicketsLost,
                    result.secondInnings.oversCompleted);
            }
        }

        public void HideResult()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
        }
    }
}
