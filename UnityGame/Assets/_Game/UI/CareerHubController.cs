using UnityEngine;
using UnityEngine.UI;
using CricketGame.Core;
using CricketGame.Career;
using CricketGame.Career.ViewModels;
using CricketGame.Career.Tournaments;

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

        [Header("Objective & Recent Match")]
        [SerializeField] private Text nextObjectiveText;
        [SerializeField] private Text recentResultText;

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
            CareerProfile profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
            if (profile == null)
            {
                Debug.LogWarning("[CareerHubController] No active career found, displaying placeholder data.");
                SetPlaceholderData();
                return;
            }

            var tournament = (TournamentManager.Instance != null) ? TournamentManager.Instance.ActiveTournament : null;
            CareerHubViewModel vm = CareerHubViewModel.FromCareerProfile(profile, tournament, profile.recentMatchResultSummary);
            BindViewModel(vm);
        }

        public void BindViewModel(CareerHubViewModel vm)
        {
            if (vm == null) return;

            if (playerNameText != null) playerNameText.text = vm.playerName;
            if (playerAgeText != null) playerAgeText.text = string.Format("Age: {0}", vm.playerAge);
            if (playerTeamText != null) playerTeamText.text = vm.currentTeam;
            if (careerLevelText != null) careerLevelText.text = vm.careerStage;
            if (overallRatingText != null) overallRatingText.text = string.Format("OVR: {0}", vm.overallRating);
            
            if (formText != null) formText.text = string.Format("Form: {0:F0}%", vm.formPercent);
            if (fitnessText != null) fitnessText.text = string.Format("Fitness: {0:F0}%", vm.fitnessPercent);

            if (matchesCountText != null) matchesCountText.text = string.Format("Stage: {0}", vm.currentTournament);
            if (runsCountText != null) runsCountText.text = string.Format("Reputation: {0:F0}", vm.reputation);
            if (wicketsCountText != null) wicketsCountText.text = string.Format("Role: {0}", vm.position);
            if (achievementsText != null) achievementsText.text = string.Format("Skill Points: {0}", vm.skillPoints);

            if (nextObjectiveText != null) nextObjectiveText.text = string.Format("Objective: {0}", vm.nextObjective);
            if (recentResultText != null) recentResultText.text = string.Format("Recent: {0}", vm.recentResult);
        }

        private void SetPlaceholderData()
        {
            CareerHubViewModel vm = new CareerHubViewModel();
            vm.playerName = "Muneeb Gulistan";
            vm.playerAge = 16;
            vm.currentTeam = "Lahore Eagles U-16";
            vm.careerStage = "Under-16 Championship";
            vm.overallRating = 58;
            vm.formPercent = 75f;
            vm.fitnessPercent = 100f;
            vm.reputation = 50f;
            vm.skillPoints = 0;
            vm.nextObjective = "Score 50 Runs in Tournament (0/50)";
            vm.recentResult = "Scheduled vs Karachi Kings";
            BindViewModel(vm);
        }

        public void OnPlayMatchClicked()
        {
            Debug.Log("[CareerHubController] Navigating to Match scene...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.MatchLoading);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneMatch);
        }

        public void OnTrainingClicked()
        {
            Debug.Log("[CareerHubController] Navigating to Training scene...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.Training);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneTraining);
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
            if (CareerManager.Instance != null) CareerManager.Instance.SaveCurrentCareer();
            Debug.Log("[CareerHubController] Career manually saved successfully.");
        }

        public void OnBackClicked()
        {
            Debug.Log("[CareerHubController] Returning to Main Menu...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.MainMenu);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneMainMenu);
        }
    }
}
