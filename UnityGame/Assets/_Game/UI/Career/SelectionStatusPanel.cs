using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Career;
using CricketGame.Career.MatchIntegration;

namespace CricketGame.UI.Career
{
    public class SelectionStatusPanel : MonoBehaviour
    {
        [Header("Selection Status Elements")]
        [SerializeField] private Text selectionBadgeText;
        [SerializeField] private Text formValueText;
        [SerializeField] private Slider formSlider;
        [SerializeField] private Text fitnessValueText;
        [SerializeField] private Slider fitnessSlider;

        [Header("Promotion Status Elements")]
        [SerializeField] private Text promotionProgressText;
        [SerializeField] private Slider promotionSlider;
        [SerializeField] private Button promoteButton;

        private void Start()
        {
            if (promoteButton != null)
            {
                promoteButton.onClick.AddListener(HandlePromoteClicked);
            }

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

            if (selectionBadgeText != null)
            {
                if (profile.isSelectedInPlayingXI)
                {
                    selectionBadgeText.text = "SELECTED IN STARTING XI";
                }
                else
                {
                    selectionBadgeText.text = "DROPPED TO BENCH (SELECTION PRESSURE)";
                }
            }

            if (formValueText != null) formValueText.text = string.Format("Form: {0}/100", profile.player.form);
            if (formSlider != null) formSlider.value = profile.player.form / 100f;

            if (fitnessValueText != null) fitnessValueText.text = string.Format("Fitness: {0}/100", profile.player.fitness);
            if (fitnessSlider != null) fitnessSlider.value = profile.player.fitness / 100f;

            int progressPercent = profile.progression.levelProgressPercent;
            if (promotionProgressText != null)
            {
                promotionProgressText.text = string.Format("Promotion to {0}: {1}%",
                    CareerProgression.GetLevelDisplayName(profile.progression.GetNextLevel()),
                    progressPercent);
            }
            if (promotionSlider != null) promotionSlider.value = progressPercent / 100f;

            if (promoteButton != null)
            {
                promoteButton.interactable = profile.progression.isEligibleForPromotion;
            }
        }

        private void HandlePromoteClicked()
        {
            CareerProfile profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
            if (profile != null)
            {
                bool promoted = CareerProgressionEvaluator.PromotePlayerToNextLevel(profile);
                if (promoted)
                {
                    CareerManager.Instance.SaveCurrentCareer();
                    RefreshDisplay();
                }
            }
        }
    }
}
