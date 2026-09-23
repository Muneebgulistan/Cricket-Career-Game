using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Career;
using CricketGame.Career.Tournaments;
using CricketGame.Tournaments;
using CricketGame.Core;

namespace CricketGame.UI.Career
{
    public class NextMatchPanel : MonoBehaviour
    {
        [Header("Next Match Preview Elements")]
        [SerializeField] private Text matchTitleText;
        [SerializeField] private Text opponentDetailsText;
        [SerializeField] private Text venueText;
        [SerializeField] private Text matchImportanceText;
        [SerializeField] private Button playMatchButton;

        private FixtureData nextFixture;

        private void Start()
        {
            if (playMatchButton != null)
            {
                playMatchButton.onClick.AddListener(HandlePlayMatchClicked);
            }
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            CareerProfile profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
            if (profile == null) return;

            string userTeam = profile.player.currentTeam;
            nextFixture = (TournamentManager.Instance != null) ? TournamentManager.Instance.GetNextUserFixture(userTeam) : null;

            if (nextFixture != null)
            {
                string oppName = nextFixture.homeTeamName == userTeam ? nextFixture.awayTeamName : nextFixture.homeTeamName;

                if (matchTitleText != null) matchTitleText.text = string.Format("Round {0} Fixture", nextFixture.roundNumber);
                if (opponentDetailsText != null) opponentDetailsText.text = string.Format("VS {0}", oppName);
                if (venueText != null) venueText.text = "Venue: National Cricket Stadium";
                if (matchImportanceText != null) matchImportanceText.text = "Tournament Stage: Group Match";

                if (playMatchButton != null)
                {
                    playMatchButton.interactable = profile.isSelectedInPlayingXI;
                }
            }
            else
            {
                if (matchTitleText != null) matchTitleText.text = "No Upcoming Matches";
                if (opponentDetailsText != null) opponentDetailsText.text = "Tournament stage complete!";
                if (playMatchButton != null) playMatchButton.interactable = false;
            }
        }

        private void HandlePlayMatchClicked()
        {
            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene(SceneController.SceneMatch);
            }
        }
    }
}
