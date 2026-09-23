using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Career;

namespace CricketGame.UI.Career
{
    public class PlayerStatsPanel : MonoBehaviour
    {
        [Header("Batting Statistics")]
        [SerializeField] private Text battingMatchesText;
        [SerializeField] private Text battingRunsText;
        [SerializeField] private Text battingAverageText;
        [SerializeField] private Text battingStrikeRateText;
        [SerializeField] private Text battingHighestScoreText;
        [SerializeField] private Text battingMilestonesText; // 50s / 100s

        [Header("Bowling Statistics")]
        [SerializeField] private Text bowlingWicketsText;
        [SerializeField] private Text bowlingAverageText;
        [SerializeField] private Text bowlingEconomyText;
        [SerializeField] private Text bowlingMaidensText;

        private void Start()
        {
            RefreshDisplay();
            if (CareerManager.Instance != null)
            {
                CareerManager.Instance.OnCareerUpdated += HandleCareerUpdated;
            }
        }

        private void OnDestroy()
        {
            if (CareerManager.Instance != null)
            {
                CareerManager.Instance.OnCareerUpdated -= HandleCareerUpdated;
            }
        }

        private void HandleCareerUpdated(CareerProfile profile)
        {
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            CareerProfile profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
            if (profile == null) return;

            var bat = profile.statistics.allTimeBatting;
            var bowl = profile.statistics.allTimeBowling;

            if (battingMatchesText != null) battingMatchesText.text = string.Format("Matches: {0} ({1} inns)", bat.matches, bat.innings);
            if (battingRunsText != null) battingRunsText.text = string.Format("Runs: {0}", bat.runs);
            if (battingAverageText != null) battingAverageText.text = string.Format("Average: {0:F1}", bat.Average);
            if (battingStrikeRateText != null) battingStrikeRateText.text = string.Format("Strike Rate: {0:F1}", bat.StrikeRate);
            if (battingHighestScoreText != null) battingHighestScoreText.text = string.Format("Highest: {0}", bat.highestScore);
            if (battingMilestonesText != null) battingMilestonesText.text = string.Format("50s: {0} | 100s: {1} | 4s: {2} | 6s: {3}", bat.fifties, bat.hundreds, bat.fours, bat.sixes);

            if (bowlingWicketsText != null) bowlingWicketsText.text = string.Format("Wickets: {0}", bowl.wickets);
            if (bowlingAverageText != null) bowlingAverageText.text = string.Format("Average: {0:F1}", bowl.Average);
            if (bowlingEconomyText != null) bowlingEconomyText.text = string.Format("Economy: {0:F2}", bowl.Economy);
            if (bowlingMaidensText != null) bowlingMaidensText.text = string.Format("Maidens: {0} ({1:F1} overs)", bowl.maidens, bowl.overs);
        }
    }
}
