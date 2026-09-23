using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Career;

namespace CricketGame.UI.Career
{
    public class CareerOverviewPanel : MonoBehaviour
    {
        [Header("Overview UI Elements")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerRoleText;
        [SerializeField] private Text overallRatingText;
        [SerializeField] private Text careerLevelText;
        [SerializeField] private Text seasonWeekText;
        [SerializeField] private Text skillPointsText;

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

            if (playerNameText != null) playerNameText.text = profile.player.name;
            if (playerRoleText != null) playerRoleText.text = profile.player.playingRole.ToString();
            if (overallRatingText != null) overallRatingText.text = string.Format("OVR: {0}", profile.player.overallRating);
            if (careerLevelText != null) careerLevelText.text = CareerProgression.GetLevelDisplayName(profile.progression.currentLevel);
            if (seasonWeekText != null) seasonWeekText.text = string.Format("Season {0} • Week {1}", profile.currentSeason, profile.currentWeek);
            if (skillPointsText != null) skillPointsText.text = string.Format("Skill Points: {0}", profile.skillPoints);
        }
    }
}
