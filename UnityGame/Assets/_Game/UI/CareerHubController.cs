using UnityEngine;
using UnityEngine.UI;
using CricketGame.Core;
using CricketGame.Career;

namespace CricketGame.UI
{
    public class CareerHubController : MonoBehaviour
    {
        [Header("Player Details Text")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text playerAgeText;
        [SerializeField] private Text playerTeamText;
        [SerializeField] private Text careerLevelText;
        [SerializeField] private Text overallRatingText;
        
        [Header("Condition Sliders / Bars")]
        [SerializeField] private Text formText;
        [SerializeField] private Text fitnessText;

        [Header("Career Stats Text")]
        [SerializeField] private Text matchesCountText;
        [SerializeField] private Text runsCountText;
        [SerializeField] private Text wicketsCountText;
        [SerializeField] private Text achievementsText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button playMatchButton;
        [SerializeField] private Button trainingButton;
        [SerializeField] private Button statisticsButton;
        [SerializeField] private Button tournamentsButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button backButton;

        private void Start()
        {
            SetupListeners();
            PopulateCareerData();
        }

        private void SetupListeners()
        {
            if (playMatchButton != null) playMatchButton.onClick.AddListener(OnPlayMatchClicked);
            if (trainingButton != null) trainingButton.onClick.AddListener(OnTrainingClicked);
            if (statisticsButton != null) statisticsButton.onClick.AddListener(OnStatisticsClicked);
            if (tournamentsButton != null) tournamentsButton.onClick.AddListener(OnTournamentsClicked);
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        public void PopulateCareerData()
        {
            CareerProfile profile = CareerManager.Instance?.ActiveCareer;
            if (profile == null)
            {
                Debug.LogWarning("[CareerHubController] No active career found, displaying placeholder data.");
                SetPlaceholderData();
                return;
            }

            var player = profile.player;
            var stats = profile.statistics;

            if (playerNameText != null) playerNameText.text = player.name;
            if (playerAgeText != null) playerAgeText.text = $"Age: {player.age}";
            if (playerTeamText != null) playerTeamText.text = player.currentTeam;
            if (careerLevelText != null) careerLevelText.text = CareerProgression.GetLevelDisplayName(player.currentLevel);
            if (overallRatingText != null) overallRatingText.text = $"OVR: {player.overallRating}";
            
            if (formText != null) formText.text = $"Form: {player.form}%";
            if (fitnessText != null) fitnessText.text = $"Fitness: {player.fitness}%";

            if (matchesCountText != null) matchesCountText.text = $"Matches: {stats.allTimeBatting.matches}";
            if (runsCountText != null) runsCountText.text = $"Runs: {stats.allTimeBatting.runs}";
            if (wicketsCountText != null) wicketsCountText.text = $"Wickets: {stats.allTimeBowling.wickets}";
            if (achievementsText != null) achievementsText.text = $"Skill Points: {profile.skillPoints}";
        }

        private void SetPlaceholderData()
        {
            if (playerNameText != null) playerNameText.text = "Muneeb Gulistan";
            if (playerAgeText != null) playerAgeText.text = "Age: 16";
            if (playerTeamText != null) playerTeamText.text = "Lahore Eagles U-16";
            if (careerLevelText != null) careerLevelText.text = "Under-16 Cup";
            if (overallRatingText != null) overallRatingText.text = "OVR: 58";
            if (formText != null) formText.text = "Form: 75%";
            if (fitnessText != null) fitnessText.text = "Fitness: 100%";
            if (matchesCountText != null) matchesCountText.text = "Matches: 0";
            if (runsCountText != null) runsCountText.text = "Runs: 0";
            if (wicketsCountText != null) wicketsCountText.text = "Wickets: 0";
            if (achievementsText != null) achievementsText.text = "Skill Points: 0";
        }

        public void OnPlayMatchClicked()
        {
            Debug.Log("[CareerHubController] Navigating to Match scene...");
            GameStateManager.Instance?.ChangeState(GameState.MatchLoading);
            SceneController.Instance?.LoadScene(SceneController.SceneMatch);
        }

        public void OnTrainingClicked()
        {
            Debug.Log("[CareerHubController] Navigating to Training scene...");
            GameStateManager.Instance?.ChangeState(GameState.Training);
            SceneController.Instance?.LoadScene(SceneController.SceneTraining);
        }

        public void OnStatisticsClicked()
        {
            Debug.Log("[CareerHubController] Statistics modal / view opened.");
        }

        public void OnTournamentsClicked()
        {
            Debug.Log("[CareerHubController] Tournament standings view opened.");
        }

        public void OnSaveClicked()
        {
            CareerManager.Instance?.SaveCurrentCareer();
            Debug.Log("[CareerHubController] Career manually saved successfully.");
        }

        public void OnBackClicked()
        {
            Debug.Log("[CareerHubController] Returning to Main Menu...");
            GameStateManager.Instance?.ChangeState(GameState.MainMenu);
            SceneController.Instance?.LoadScene(SceneController.SceneMainMenu);
        }
    }
}
