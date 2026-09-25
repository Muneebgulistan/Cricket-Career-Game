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

            // Extended stats from actual career profile
            CareerProfile profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
            if (profile != null && profile.statistics != null)
            {
                int totalRuns = (profile.statistics.allTimeBatting != null) ? profile.statistics.allTimeBatting.runs : 0;
                int totalWickets = (profile.statistics.allTimeBowling != null) ? profile.statistics.allTimeBowling.wickets : 0;
                int totalMatches = (profile.statistics.allTimeBatting != null) ? profile.statistics.allTimeBatting.matches : 0;
                int totalCatches = (profile.statistics.allTimeFielding != null) ? profile.statistics.allTimeFielding.catches : 0;
                float batAvg = (totalMatches > 0) ? ((float)totalRuns / totalMatches) : 0f;
                float bowlAvg = (totalWickets > 0 && profile.statistics.allTimeBowling != null)
                    ? ((float)profile.statistics.allTimeBowling.runsConceded / totalWickets) : 0f;

                if (careerRunsText != null) careerRunsText.text = string.Format("Runs: {0}", totalRuns);
                if (battingAverageText != null) battingAverageText.text = string.Format("Avg: {0:F1}", batAvg);
                if (careerWicketsText != null) careerWicketsText.text = string.Format("Wickets: {0}", totalWickets);
                if (bowlingAverageText != null) bowlingAverageText.text = string.Format("Bowl Avg: {0:F1}", bowlAvg);
                if (careerMatchesText != null) careerMatchesText.text = string.Format("Matches: {0}", totalMatches);
                if (careerCatchesText != null) careerCatchesText.text = string.Format("Catches: {0}", totalCatches);
            }
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

        [Header("Extended Career Stats")]
        [SerializeField] private Text careerRunsText;
        [SerializeField] private Text battingAverageText;
        [SerializeField] private Text careerWicketsText;
        [SerializeField] private Text bowlingAverageText;
        [SerializeField] private Text careerMatchesText;
        [SerializeField] private Text careerCatchesText;

        public void OnPlayMatchClicked()
        {
            CricketGame.Audio.CareerAudioEvents.PlayButtonClick();
            Debug.Log("[CareerHubController] Navigating to Match Preview...");

            // Prepare match context first
            bool contextPrepared = false;
            if (CricketGame.Career.MatchIntegration.CareerMatchLauncher.Instance != null)
            {
                CareerProfile profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
                var tournament = (CricketGame.Career.Tournaments.TournamentManager.Instance != null)
                    ? CricketGame.Career.Tournaments.TournamentManager.Instance.ActiveTournament
                    : null;

                if (profile != null)
                {
                    // Prepare context without loading match scene yet
                    var launcher = CricketGame.Career.MatchIntegration.CareerMatchLauncher.Instance;
                    var context = launcher.CreateMatchContext(profile, tournament);
                    launcher.SetActiveContextForTesting(context);
                    context.isMatchInProgress = false;
                    context.isResultProcessed = false;
                    contextPrepared = true;
                }
            }

            // Navigate to match preview scene
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.MatchPreview);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneMatchPreview);
        }

        public void OnTrainingClicked()
        {
            CricketGame.Audio.CareerAudioEvents.PlayButtonClick();
            Debug.Log("[CareerHubController] Navigating to Training scene...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.Training);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneTraining);
        }

        public void OnStatisticsClicked()
        {
            CricketGame.Audio.CareerAudioEvents.PlayButtonClick();
            Debug.Log("[CareerHubController] Statistics modal / view opened.");
        }

        public void OnTournamentsClicked()
        {
            CricketGame.Audio.CareerAudioEvents.PlayButtonClick();
            Debug.Log("[CareerHubController] Navigating to Tournament scene...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.Tournament);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneTournament);
        }

        public void OnSaveClicked()
        {
            CricketGame.Audio.CareerAudioEvents.PlayButtonClick();
            if (CareerManager.Instance != null) CareerManager.Instance.SaveCurrentCareer();
            Debug.Log("[CareerHubController] Career manually saved successfully.");
        }

        public void OnBackClicked()
        {
            CricketGame.Audio.CareerAudioEvents.PlayButtonClick();
            Debug.Log("[CareerHubController] Returning to Main Menu...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.MainMenu);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneMainMenu);
        }
    }
}
