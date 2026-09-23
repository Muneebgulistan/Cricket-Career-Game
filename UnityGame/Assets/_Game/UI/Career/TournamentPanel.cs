using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Career.Tournaments;
using CricketGame.Tournaments;

namespace CricketGame.UI.Career
{
    public class TournamentPanel : MonoBehaviour
    {
        [Header("Tournament Standings UI")]
        [SerializeField] private Text tournamentTitleText;
        [SerializeField] private Text standingsTableText;
        [SerializeField] private Text fixturesListText;

        private void Start()
        {
            RefreshDisplay();
            if (TournamentManager.Instance != null)
            {
                TournamentManager.Instance.OnTournamentUpdated += HandleTournamentUpdated;
            }
        }

        private void OnDestroy()
        {
            if (TournamentManager.Instance != null)
            {
                TournamentManager.Instance.OnTournamentUpdated -= HandleTournamentUpdated;
            }
        }

        private void HandleTournamentUpdated(TournamentProgress tourney)
        {
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            TournamentProgress tourney = (TournamentManager.Instance != null) ? TournamentManager.Instance.ActiveTournament : null;
            if (tourney == null) return;

            if (tournamentTitleText != null)
            {
                tournamentTitleText.text = tourney.tournamentName;
            }

            if (standingsTableText != null && tourney.standings != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("POS  TEAM                        P   W   L   T   PTS   NRR");
                sb.AppendLine("----------------------------------------------------------");
                for (int i = 0; i < tourney.standings.Count; i++)
                {
                    var s = tourney.standings[i];
                    string row = string.Format("{0,-4} {1,-26} {2,2}  {3,2}  {4,2}  {5,2}   {6,2}  {7:+0.00;-0.00;0.00}",
                        i + 1, s.teamName, s.played, s.won, s.lost, s.tied, s.points, s.netRunRate);
                    sb.AppendLine(row);
                }
                standingsTableText.text = sb.ToString();
            }

            if (fixturesListText != null && tourney.fixtures != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("TOURNAMENT FIXTURES:");
                for (int i = 0; i < tourney.fixtures.Count; i++)
                {
                    var f = tourney.fixtures[i];
                    string status = f.isCompleted ? f.resultSummary : "Scheduled";
                    sb.AppendLine(string.Format("R{0}: {1} vs {2} — {3}", f.roundNumber, f.homeTeamName, f.awayTeamName, status));
                }
                fixturesListText.text = sb.ToString();
            }
        }
    }
}
