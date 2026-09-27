using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Career;
using CricketGame.Career.Tournaments;
using CricketGame.Players;
using CricketGame.Cricket;
using CricketGame.SaveSystem;

namespace CricketGame.UI
{
    /// <summary>
    /// Comprehensive Career Hub Dashboard displaying upcoming fixtures,
    /// player statistics, form/fatigue condition meters, and attribute upgrade panels.
    /// </summary>
    public class CareerHubDashboard : MonoBehaviour
    {
        public static CareerHubDashboard Instance { get; private set; }

        [Header("Player Overview Header")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerRoleText;
        [SerializeField] private Text teamNameText;
        [SerializeField] private Text overallRatingText;
        [SerializeField] private Text skillPointsText;

        [Header("Condition & Fatigue")]
        [SerializeField] private Slider formSlider;
        [SerializeField] private Slider fitnessSlider;
        [SerializeField] private Text formLabel;
        [SerializeField] private Text fitnessLabel;

        [Header("Fixture Information")]
        [SerializeField] private Text upcomingTournamentText;
        [SerializeField] private Text nextOpponentText;
        [SerializeField] private Text matchFormatText;
        [SerializeField] private Text matchVenueText;

        [Header("Career Statistics")]
        [SerializeField] private Text totalMatchesText;
        [SerializeField] private Text totalRunsText;
        [SerializeField] private Text battingAvgText;
        [SerializeField] private Text totalWicketsText;
        [SerializeField] private Text bowlingAvgText;
        [SerializeField] private Text totalCatchesText;

        [Header("Attribute Upgrade Panel")]
        [SerializeField] private Text battingRatingText;
        [SerializeField] private Text bowlingRatingText;
        [SerializeField] private Text fitnessRatingText;
        [SerializeField] private Button upgradeBattingButton;
        [SerializeField] private Button upgradeBowlingButton;
        [SerializeField] private Button upgradeFitnessButton;
        [SerializeField] private Button restButton;

        [Header("Action Controls")]
        [SerializeField] private Button playMatchButton;
        [SerializeField] private Button saveCareerButton;
        [SerializeField] private Button backToMenuButton;

        private const int UPGRADE_COST_PER_POINT = 100;
        private const int MAX_ATTRIBUTE_VALUE = 99;

        private CareerProfile activeProfile;

        public CareerProfile ActiveProfile { get { return activeProfile; } }

        public event Action<string, int> OnAttributeUpgraded;
        public event Action OnPlayerRested;
        public event Action OnMatchLaunchRequested;

        public static void SetInstanceForTesting(CareerHubDashboard inst)
        {
            Instance = inst;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            SetupListeners();
            RefreshDashboard();
        }

        private void SetupListeners()
        {
            if (upgradeBattingButton != null) upgradeBattingButton.onClick.AddListener(OnUpgradeBattingClicked);
            if (upgradeBowlingButton != null) upgradeBowlingButton.onClick.AddListener(OnUpgradeBowlingClicked);
            if (upgradeFitnessButton != null) upgradeFitnessButton.onClick.AddListener(OnUpgradeFitnessClicked);
            if (restButton != null) restButton.onClick.AddListener(RestPlayer);
            if (playMatchButton != null) playMatchButton.onClick.AddListener(OnPlayMatchClicked);
            if (saveCareerButton != null) saveCareerButton.onClick.AddListener(OnSaveClicked);
            if (backToMenuButton != null) backToMenuButton.onClick.AddListener(OnBackClicked);
        }

        public void OnUpgradeBattingClicked() { UpgradeBatting(); }
        public void OnUpgradeBowlingClicked() { UpgradeBowling(); }
        public void OnUpgradeFitnessClicked() { UpgradeFitness(); }

        public void BindProfile(CareerProfile profile)
        {
            activeProfile = profile;
            RefreshDashboard();
        }

        public void RefreshDashboard()
        {
            if (activeProfile == null)
            {
                if (CareerManager.Instance != null && CareerManager.Instance.ActiveCareer != null)
                {
                    activeProfile = CareerManager.Instance.ActiveCareer;
                }
                else if (SaveProfileManager.Instance != null && SaveProfileManager.Instance.CurrentProfile != null)
                {
                    activeProfile = SaveProfileManager.Instance.CurrentProfile;
                }
            }

            if (activeProfile == null)
            {
                SetDefaultMockDashboard();
                return;
            }

            PlayerProfile player = activeProfile.player;

            // Header Info
            if (playerNameText != null) playerNameText.text = player != null ? player.name : "Player";
            if (playerRoleText != null) playerRoleText.text = player != null ? player.playingRole.ToString() : "All-Rounder";
            if (teamNameText != null) teamNameText.text = player != null ? player.currentTeam : "Lahore Eagles";
            if (overallRatingText != null) overallRatingText.text = string.Format("OVR: {0}", player != null ? player.overallRating : 60);
            if (skillPointsText != null) skillPointsText.text = string.Format("Skill Points: {0}", activeProfile.skillPoints);

            // Condition & Fatigue
            int form = player != null ? player.form : 75;
            int fitness = player != null ? player.fitness : 100;
            if (formSlider != null) formSlider.value = form / 100f;
            if (fitnessSlider != null) fitnessSlider.value = fitness / 100f;
            if (formLabel != null) formLabel.text = string.Format("Form: {0}%", form);
            if (fitnessLabel != null) fitnessLabel.text = string.Format("Fitness: {0}%", fitness);

            // Fixture Info
            if (upcomingTournamentText != null) upcomingTournamentText.text = "Under-16 National Cup";
            if (nextOpponentText != null) nextOpponentText.text = "Karachi Kings U-16";
            if (matchFormatText != null) matchFormatText.text = "Format: T20 (20 Overs)";
            if (matchVenueText != null) matchVenueText.text = "Venue: Gaddafi Stadium Lahore";

            // Career Stats
            if (activeProfile.statistics != null)
            {
                var stats = activeProfile.statistics;
                int matches = stats.allTimeBatting != null ? stats.allTimeBatting.matches : 0;
                int runs = stats.allTimeBatting != null ? stats.allTimeBatting.runs : 0;
                int wickets = stats.allTimeBowling != null ? stats.allTimeBowling.wickets : 0;
                int catches = stats.allTimeFielding != null ? stats.allTimeFielding.catches : 0;
                float batAvg = matches > 0 ? (float)runs / matches : 0f;
                float bowlAvg = wickets > 0 && stats.allTimeBowling != null ? (float)stats.allTimeBowling.runsConceded / wickets : 0f;

                if (totalMatchesText != null) totalMatchesText.text = string.Format("Matches: {0}", matches);
                if (totalRunsText != null) totalRunsText.text = string.Format("Runs: {0}", runs);
                if (battingAvgText != null) battingAvgText.text = string.Format("Bat Avg: {0:F1}", batAvg);
                if (totalWicketsText != null) totalWicketsText.text = string.Format("Wickets: {0}", wickets);
                if (bowlingAvgText != null) bowlingAvgText.text = string.Format("Bowl Avg: {0:F1}", bowlAvg);
                if (totalCatchesText != null) totalCatchesText.text = string.Format("Catches: {0}", catches);
            }

            // Attribute Upgrade Panel
            if (battingRatingText != null && player != null) battingRatingText.text = string.Format("Batting: {0}", player.battingRating);
            if (bowlingRatingText != null && player != null) bowlingRatingText.text = string.Format("Bowling: {0}", player.bowlingRating);
            if (fitnessRatingText != null && player != null) fitnessRatingText.text = string.Format("Fitness: {0}", player.fitness);

            RefreshUpgradeButtons();
        }

        private void SetDefaultMockDashboard()
        {
            if (playerNameText != null) playerNameText.text = "Muneeb Gulistan";
            if (playerRoleText != null) playerRoleText.text = "Batsman";
            if (teamNameText != null) teamNameText.text = "Lahore Eagles U-16";
            if (overallRatingText != null) overallRatingText.text = "OVR: 62";
            if (skillPointsText != null) skillPointsText.text = "Skill Points: 500";
            if (formLabel != null) formLabel.text = "Form: 80%";
            if (fitnessLabel != null) fitnessLabel.text = "Fitness: 95%";
            if (upcomingTournamentText != null) upcomingTournamentText.text = "Under-16 National Cup";
            if (nextOpponentText != null) nextOpponentText.text = "Karachi Kings U-16";
        }

        private void RefreshUpgradeButtons()
        {
            bool canAfford = activeProfile != null && activeProfile.skillPoints >= UPGRADE_COST_PER_POINT;
            if (upgradeBattingButton != null) upgradeBattingButton.interactable = canAfford && (activeProfile.player.battingRating < MAX_ATTRIBUTE_VALUE);
            if (upgradeBowlingButton != null) upgradeBowlingButton.interactable = canAfford && (activeProfile.player.bowlingRating < MAX_ATTRIBUTE_VALUE);
            if (upgradeFitnessButton != null) upgradeFitnessButton.interactable = canAfford && (activeProfile.player.fitness < MAX_ATTRIBUTE_VALUE);
        }

        // ==========================================
        // ATTRIBUTE UPGRADE SYSTEM
        // ==========================================

        public bool UpgradeBatting()
        {
            if (activeProfile == null || activeProfile.player == null) return false;
            if (activeProfile.skillPoints < UPGRADE_COST_PER_POINT || activeProfile.player.battingRating >= MAX_ATTRIBUTE_VALUE) return false;

            activeProfile.skillPoints -= UPGRADE_COST_PER_POINT;
            activeProfile.player.battingRating = Mathf.Clamp(activeProfile.player.battingRating + 1, 1, MAX_ATTRIBUTE_VALUE);
            RecalculateOverallRating();

            if (OnAttributeUpgraded != null) OnAttributeUpgraded("Batting", activeProfile.player.battingRating);
            RefreshDashboard();
            return true;
        }

        public bool UpgradeBowling()
        {
            if (activeProfile == null || activeProfile.player == null) return false;
            if (activeProfile.skillPoints < UPGRADE_COST_PER_POINT || activeProfile.player.bowlingRating >= MAX_ATTRIBUTE_VALUE) return false;

            activeProfile.skillPoints -= UPGRADE_COST_PER_POINT;
            activeProfile.player.bowlingRating = Mathf.Clamp(activeProfile.player.bowlingRating + 1, 1, MAX_ATTRIBUTE_VALUE);
            RecalculateOverallRating();

            if (OnAttributeUpgraded != null) OnAttributeUpgraded("Bowling", activeProfile.player.bowlingRating);
            RefreshDashboard();
            return true;
        }

        public bool UpgradeFitness()
        {
            if (activeProfile == null || activeProfile.player == null) return false;
            if (activeProfile.skillPoints < UPGRADE_COST_PER_POINT || activeProfile.player.fitness >= MAX_ATTRIBUTE_VALUE) return false;

            activeProfile.skillPoints -= UPGRADE_COST_PER_POINT;
            activeProfile.player.fitness = Mathf.Clamp(activeProfile.player.fitness + 2, 1, 100);
            RecalculateOverallRating();

            if (OnAttributeUpgraded != null) OnAttributeUpgraded("Fitness", activeProfile.player.fitness);
            RefreshDashboard();
            return true;
        }

        public void RestPlayer()
        {
            if (activeProfile == null || activeProfile.player == null) return;

            // Rest recovers fitness to 100 and stabilizes form
            activeProfile.player.fitness = Mathf.Min(activeProfile.player.fitness + 15, 100);
            activeProfile.player.form = Mathf.Clamp(activeProfile.player.form + 5, 50, 100);

            if (OnPlayerRested != null) OnPlayerRested();
            RefreshDashboard();
        }

        private void RecalculateOverallRating()
        {
            if (activeProfile == null || activeProfile.player == null) return;
            var p = activeProfile.player;
            p.overallRating = Mathf.RoundToInt((p.battingRating * 0.4f) + (p.bowlingRating * 0.4f) + (p.fieldingRating * 0.2f));
        }

        // ==========================================
        // ACTION CONTROLS
        // ==========================================

        public void OnPlayMatchClicked()
        {
            if (OnMatchLaunchRequested != null) OnMatchLaunchRequested();

            if (AppFlowManager.Instance != null)
            {
                AppFlowManager.Instance.LaunchMatch();
            }
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.LoadSceneAsync("SceneMatch");
            }
        }

        public void OnSaveClicked()
        {
            if (activeProfile != null)
            {
                if (SaveProfileManager.Instance != null)
                {
                    SaveProfileManager.Instance.SaveProfile(activeProfile);
                }
                if (CareerManager.Instance != null)
                {
                    CareerManager.Instance.SaveCurrentCareer();
                }
            }
        }

        public void OnBackClicked()
        {
            if (AppFlowManager.Instance != null)
            {
                AppFlowManager.Instance.TransitionTo(AppFlowState.MainMenu);
            }
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.PopPanel();
            }
        }
    }
}
