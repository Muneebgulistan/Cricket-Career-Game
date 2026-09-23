using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Core;
using CricketGame.Career;
using CricketGame.Career.Tournaments;
using CricketGame.Career.ViewModels;
using CricketGame.Career.MatchIntegration;
using CricketGame.Audio;

namespace CricketGame.UI
{
    public class TournamentScreenController : MonoBehaviour
    {
        [Header("Tournament Info")]
        [SerializeField] private Text tournamentTitleText;
        [SerializeField] private Text stageText;
        [SerializeField] private Text teamText;
        [SerializeField] private Text positionText;

        [Header("Record & Standings")]
        [SerializeField] private Text matchesText;
        [SerializeField] private Text winsLossesText;
        [SerializeField] private Text roundText;
        [SerializeField] private Text qualificationStatusText;
        [SerializeField] private Text progressText;

        [Header("Player Tournament Figures")]
        [SerializeField] private Text playerRunsText;
        [SerializeField] private Text playerWicketsText;

        [Header("Action Buttons")]
        [SerializeField] private Button playNextMatchButton;
        [SerializeField] private Button backButton;

        private void Start()
        {
            SetupListeners();
            PopulateData();
        }

        private void SetupListeners()
        {
            if (playNextMatchButton != null) playNextMatchButton.onClick.AddListener(OnPlayNextMatchClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        public void PopulateData()
        {
            var tournament = (TournamentManager.Instance != null) ? TournamentManager.Instance.ActiveTournament : null;
            var profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;

            TournamentScreenViewModel vm = TournamentScreenViewModel.FromTournament(tournament, profile);
            BindViewModel(vm);
        }

        public void BindViewModel(TournamentScreenViewModel vm)
        {
            if (vm == null) return;

            if (tournamentTitleText != null) tournamentTitleText.text = vm.tournamentName;
            if (stageText != null) stageText.text = string.Format("Tier: {0}", vm.stage);
            if (teamText != null) teamText.text = string.Format("Team: {0}", vm.team);
            if (positionText != null) positionText.text = string.Format("Standing: #{0}", vm.position);

            if (matchesText != null) matchesText.text = string.Format("Matches: {0}", vm.matches);
            if (winsLossesText != null) winsLossesText.text = string.Format("W: {0} | L: {1}", vm.wins, vm.losses);
            if (roundText != null) roundText.text = string.Format("Round: {0}", vm.currentRound);
            if (qualificationStatusText != null) qualificationStatusText.text = string.Format("Status: {0}", vm.qualificationStatus);
            if (progressText != null) progressText.text = string.Format("Progress: {0:F0}%", vm.progress);

            if (playerRunsText != null) playerRunsText.text = string.Format("Runs: {0}", vm.playerRuns);
            if (playerWicketsText != null) playerWicketsText.text = string.Format("Wickets: {0}", vm.playerWickets);
        }

        public void OnPlayNextMatchClicked()
        {
            CareerAudioEvents.PlayButtonClick();
            Debug.Log("[TournamentScreenController] Launching next tournament match...");

            bool launched = false;
            if (CareerMatchLauncher.Instance != null)
            {
                launched = CareerMatchLauncher.Instance.LaunchCareerMatch();
            }

            if (!launched)
            {
                if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.MatchLoading);
                if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneMatch);
            }
        }

        public void OnBackClicked()
        {
            CareerAudioEvents.PlayButtonClick();
            Debug.Log("[TournamentScreenController] Returning to Career Hub...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.CareerHub);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneCareerHub);
        }
    }
}
