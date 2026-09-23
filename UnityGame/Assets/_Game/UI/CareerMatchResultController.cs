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
    public class CareerMatchResultController : MonoBehaviour
    {
        [Header("Match Result Card")]
        [SerializeField] private Text matchResultTitleText;
        [SerializeField] private Text teamsScoreText;
        [SerializeField] private Text winnerSummaryText;

        [Header("Player Performance Card")]
        [SerializeField] private Text battingFiguresText;
        [SerializeField] private Text bowlingFiguresText;
        [SerializeField] private Text fieldingFiguresText;
        [SerializeField] private Text matchRatingText;
        [SerializeField] private Text performanceGradeText;
        [SerializeField] private GameObject potmBadge;

        [Header("Career Delta Card")]
        [SerializeField] private Text careerRunsDeltaText;
        [SerializeField] private Text careerWicketsDeltaText;
        [SerializeField] private Text rewardsGainedText;
        [SerializeField] private Text objectivesSummaryText;

        [Header("Tournament Delta Card")]
        [SerializeField] private Text tournamentPositionDeltaText;
        [SerializeField] private Text tournamentRecordText;
        [SerializeField] private Text qualificationStatusText;

        [Header("Promotion Card")]
        [SerializeField] private GameObject promotionBanner;
        [SerializeField] private Text promotionTitleText;
        [SerializeField] private Text promotionDetailsText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button nextMatchButton;
        [SerializeField] private Button tournamentButton;
        [SerializeField] private Button careerHubButton;

        private void Start()
        {
            SetupListeners();
            PopulateData();
        }

        private void SetupListeners()
        {
            if (nextMatchButton != null) nextMatchButton.onClick.AddListener(OnNextMatchClicked);
            if (tournamentButton != null) tournamentButton.onClick.AddListener(OnTournamentClicked);
            if (careerHubButton != null) careerHubButton.onClick.AddListener(OnCareerHubClicked);
        }

        public void PopulateData()
        {
            CareerMatchContext context = (CareerMatchLauncher.Instance != null) 
                ? CareerMatchLauncher.Instance.ActiveContext 
                : null;
            CareerProfile profile = (CareerManager.Instance != null) 
                ? CareerManager.Instance.ActiveCareer 
                : null;
            var tournament = (TournamentManager.Instance != null) 
                ? TournamentManager.Instance.ActiveTournament 
                : null;

            CareerMatchResultViewModel vm = CareerMatchResultViewModel.FromContext(context, profile, tournament);
            BindViewModel(vm);

            // Play audio cue
            CareerAudioEvents.PlayMatchResultSound(vm.isPlayerVictory);
            if (vm.isPromoted)
            {
                CareerAudioEvents.PlayPromotionSound();
            }
        }

        public void BindViewModel(CareerMatchResultViewModel vm)
        {
            if (vm == null) return;

            // 1. Match Result
            if (matchResultTitleText != null) matchResultTitleText.text = vm.resultText;
            if (teamsScoreText != null) teamsScoreText.text = string.Format("{0} vs {1}", vm.homeTeamName, vm.awayTeamName);
            if (winnerSummaryText != null) winnerSummaryText.text = string.Format("Winner: {0}", vm.winnerTeamName);

            // 2. Player Performance
            if (battingFiguresText != null)
            {
                battingFiguresText.text = string.Format("Batting: {0} ({1}b) • SR: {2:F1}", vm.runs, vm.balls, vm.strikeRate);
            }
            if (bowlingFiguresText != null)
            {
                bowlingFiguresText.text = string.Format("Bowling: {0}/{1} ({2:F1} ov) • Econ: {3:F1}", 
                    vm.wickets, vm.runsConceded, vm.oversBowled, vm.economy);
            }
            if (fieldingFiguresText != null)
            {
                fieldingFiguresText.text = string.Format("Fielding: {0} Catch(es) | {1} Run-Out(s)", vm.catches, vm.runOuts);
            }
            if (matchRatingText != null)
            {
                matchRatingText.text = string.Format("Match Rating: {0:F1}/10.0", vm.matchRating);
            }
            if (performanceGradeText != null)
            {
                performanceGradeText.text = string.Format("Grade: {0}", vm.performanceGrade);
            }
            if (potmBadge != null)
            {
                potmBadge.SetActive(vm.isPlayerOfTheMatch);
            }

            // 3. Career Delta
            if (careerRunsDeltaText != null)
            {
                careerRunsDeltaText.text = string.Format("Career Runs: {0} -> {1}", vm.careerRunsBefore, vm.careerRunsAfter);
            }
            if (careerWicketsDeltaText != null)
            {
                careerWicketsDeltaText.text = string.Format("Career Wickets: {0} -> {1}", vm.careerWicketsBefore, vm.careerWicketsAfter);
            }
            if (rewardsGainedText != null)
            {
                rewardsGainedText.text = string.Format("+{0} Skill Points | +{1:F0} Reputation", vm.skillPointsGained, vm.reputationDelta);
            }
            if (objectivesSummaryText != null)
            {
                objectivesSummaryText.text = vm.completedObjectivesText;
            }

            // 4. Tournament Delta
            if (tournamentPositionDeltaText != null)
            {
                tournamentPositionDeltaText.text = string.Format("Standings: #{0} -> #{1}", vm.tournamentPositionBefore, vm.tournamentPositionAfter);
            }
            if (tournamentRecordText != null)
            {
                tournamentRecordText.text = string.Format("Played: {0} (W: {1}, L: {2})", vm.tournamentMatches, vm.tournamentWins, vm.tournamentLosses);
            }
            if (qualificationStatusText != null)
            {
                qualificationStatusText.text = string.Format("Status: {0}", vm.qualificationStatus);
            }

            // 5. Promotion Banner
            if (promotionBanner != null)
            {
                promotionBanner.SetActive(vm.isPromoted);
            }
            if (promotionTitleText != null)
            {
                promotionTitleText.text = "NEW CAREER STAGE UNLOCKED!";
            }
            if (promotionDetailsText != null && vm.isPromoted)
            {
                promotionDetailsText.text = string.Format("Promoted from {0} to {1}!\n{2}", 
                    vm.previousStage, vm.newStage, vm.unlockedOpportunity);
            }
        }

        public void OnNextMatchClicked()
        {
            CareerAudioEvents.PlayButtonClick();
            Debug.Log("[CareerMatchResultController] Navigating to next match...");

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

        public void OnTournamentClicked()
        {
            CareerAudioEvents.PlayButtonClick();
            Debug.Log("[CareerMatchResultController] Navigating to Tournament standings...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.Tournament);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneTournament);
        }

        public void OnCareerHubClicked()
        {
            CareerAudioEvents.PlayButtonClick();
            Debug.Log("[CareerMatchResultController] Returning to Career Hub...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.CareerHub);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneCareerHub);
        }
    }
}
