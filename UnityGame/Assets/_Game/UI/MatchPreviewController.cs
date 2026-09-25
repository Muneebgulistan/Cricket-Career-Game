using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Core;
using CricketGame.Career;
using CricketGame.Career.MatchIntegration;
using CricketGame.Audio;

namespace CricketGame.UI
{
    /// <summary>
    /// Pre-match preview screen. Displays teams, venue, format, match number.
    /// Start Match uses the existing CareerMatchLauncher flow.
    /// Bind this to the MatchPreview scene Canvas.
    /// </summary>
    public class MatchPreviewController : MonoBehaviour
    {
        [Header("Match Header")]
        [SerializeField] private Text playerTeamNameText;
        [SerializeField] private Text vsText;
        [SerializeField] private Text opponentTeamNameText;

        [Header("Match Info")]
        [SerializeField] private Text tournamentNameText;
        [SerializeField] private Text matchFormatText;
        [SerializeField] private Text venueText;
        [SerializeField] private Text matchNumberText;
        [SerializeField] private Text homeAwayText;

        [Header("Player Info")]
        [SerializeField] private Text playerRoleText;
        [SerializeField] private Text playerNameText;

        [Header("Action Buttons")]
        [SerializeField] private Button startMatchButton;
        [SerializeField] private Button backButton;

        // --------------------------------------------------
        // Lifecycle
        // --------------------------------------------------

        private void Start()
        {
            SetupListeners();
            PopulateData();
        }

        private void SetupListeners()
        {
            if (startMatchButton != null) startMatchButton.onClick.AddListener(OnStartMatchClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        // --------------------------------------------------
        // Data Population
        // --------------------------------------------------

        public void PopulateData()
        {
            CareerMatchContext ctx = GetActiveContext();

            if (ctx == null)
            {
                SetPlaceholderData();
                return;
            }

            string playerTeam = ctx.playerTeamName;
            string opponent = ctx.opponentTeamName;
            // Determine home/away by comparing player team to home team
            bool isHome = (ctx.homeTeamName == playerTeam);

            if (playerTeamNameText != null) playerTeamNameText.text = playerTeam;
            if (vsText != null) vsText.text = "VS";
            if (opponentTeamNameText != null) opponentTeamNameText.text = opponent;

            if (tournamentNameText != null)
                tournamentNameText.text = string.Format("Tournament: {0}", ctx.tournamentName);

            if (matchFormatText != null)
                matchFormatText.text = string.Format("Format: {0}", ctx.format.ToString());

            if (venueText != null)
                venueText.text = string.Format("Venue: {0}", ctx.venue);

            if (matchNumberText != null)
            {
                int num = (ctx.fixture != null) ? ctx.fixture.matchNumber : 1;
                matchNumberText.text = string.Format("Match #{0}", num);
            }

            if (homeAwayText != null)
                homeAwayText.text = isHome ? "HOME" : "AWAY";

            // Player info
            var profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
            if (profile != null)
            {
                if (playerNameText != null)
                    playerNameText.text = profile.player.name;

                if (playerRoleText != null)
                    playerRoleText.text = string.Format("Role: {0}", profile.player.playingRole.ToString());
            }
        }

        private void SetPlaceholderData()
        {
            if (playerTeamNameText != null) playerTeamNameText.text = "Lahore Eagles";
            if (vsText != null) vsText.text = "VS";
            if (opponentTeamNameText != null) opponentTeamNameText.text = "Karachi Kings";
            if (tournamentNameText != null) tournamentNameText.text = "Tournament: U16 National Cup";
            if (matchFormatText != null) matchFormatText.text = "Format: T20";
            if (venueText != null) venueText.text = "Venue: Gaddafi Stadium, Lahore";
            if (matchNumberText != null) matchNumberText.text = "Match #1";
            if (homeAwayText != null) homeAwayText.text = "HOME";
            if (playerNameText != null) playerNameText.text = "Muneeb Gulistan";
            if (playerRoleText != null) playerRoleText.text = "Role: All-Rounder";
        }

        private CareerMatchContext GetActiveContext()
        {
            if (CareerMatchLauncher.Instance == null) return null;
            return CareerMatchLauncher.Instance.ActiveContext;
        }

        // --------------------------------------------------
        // Button Handlers
        // --------------------------------------------------

        public void OnStartMatchClicked()
        {
            CareerAudioEvents.PlayButtonClick();
            Debug.Log("[MatchPreviewController] Starting match from preview...");

            // Use existing CareerMatchLauncher flow — already prepared
            bool launched = false;
            if (CareerMatchLauncher.Instance != null)
            {
                launched = CareerMatchLauncher.Instance.LaunchMatchFromPreview();
            }

            if (!launched)
            {
                // Fallback: directly load match scene
                if (GameStateManager.Instance != null)
                    GameStateManager.Instance.ChangeState(GameState.PlayingMatch);
                if (SceneController.Instance != null)
                    SceneController.Instance.LoadScene(SceneController.SceneMatch);
            }
        }

        public void OnBackClicked()
        {
            CareerAudioEvents.PlayButtonClick();
            Debug.Log("[MatchPreviewController] Returning to Career Hub from preview...");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.CareerHub);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneCareerHub);
        }
    }
}
