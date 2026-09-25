using System;
using UnityEngine;
using CricketGame.Core;
using CricketGame.Cricket;
using CricketGame.Career.Tournaments;
using CricketGame.Tournaments;

namespace CricketGame.Career.MatchIntegration
{
    public class CareerMatchLauncher : MonoBehaviour
    {
        public static CareerMatchLauncher Instance { get; private set; }

        public CareerMatchContext ActiveContext { get; private set; }

        public event Action<CareerMatchContext> OnMatchLaunched;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public CareerMatchContext CreateMatchContext(CareerProfile profile, TournamentProgress tournament)
        {
            CareerMatchContext context = new CareerMatchContext();
            if (profile == null) return context;

            context.careerId = profile.careerId;
            context.playerProfile = profile.player;
            context.playerTeamName = (profile.player != null && !string.IsNullOrEmpty(profile.player.currentTeam)) 
                ? profile.player.currentTeam 
                : "Lahore Eagles U-16";

            // Record pre-match baseline stats for delta calculation
            if (profile.statistics != null && profile.statistics.allTimeBatting != null)
            {
                context.preMatchCareerRuns = profile.statistics.allTimeBatting.runs;
            }
            if (profile.statistics != null && profile.statistics.allTimeBowling != null)
            {
                context.preMatchCareerWickets = profile.statistics.allTimeBowling.wickets;
            }
            context.preMatchSkillPoints = profile.skillPoints;
            context.preMatchReputation = profile.reputation;

            if (tournament != null)
            {
                context.tournamentId = tournament.tournamentId;
                context.tournamentName = tournament.tournamentName;
                context.format = tournament.format;
                context.totalOvers = tournament.totalOvers > 0 ? tournament.totalOvers : 20;
                context.preMatchTournamentPosition = tournament.tournamentPosition;

                // Obtain next fixture
                TournamentFixture fix = null;
                if (TournamentManager.Instance != null)
                {
                    fix = TournamentManager.Instance.GetNextUserTournamentFixture(context.playerTeamName);
                }

                if (fix != null)
                {
                    context.fixture = fix;
                    context.homeTeamName = fix.homeTeamName;
                    context.awayTeamName = fix.awayTeamName;
                    context.opponentTeamName = fix.GetOpponent(context.playerTeamName);
                    context.venue = fix.venue;
                }
                else
                {
                    // Generate fallback fixture against tournament opposition
                    context.homeTeamName = context.playerTeamName;
                    context.awayTeamName = "Karachi Kings U-16";
                    context.opponentTeamName = context.awayTeamName;
                    context.venue = "Gaddafi Stadium";
                    context.fixture = new TournamentFixture(
                        1, 
                        "FIX_FALLBACK", 
                        context.homeTeamName, 
                        context.awayTeamName, 
                        context.venue, 
                        tournament.currentRound, 
                        context.format
                    );
                }
            }
            else
            {
                context.tournamentName = "Youth Exhibition Match";
                context.homeTeamName = context.playerTeamName;
                context.awayTeamName = "Islamabad United U-16";
                context.opponentTeamName = context.awayTeamName;
                context.venue = "Rawalpindi Stadium";
                context.format = MatchFormat.T20;
                context.totalOvers = 20;
                context.fixture = new TournamentFixture(
                    1, 
                    "FIX_EXHIBITION", 
                    context.homeTeamName, 
                    context.awayTeamName, 
                    context.venue, 
                    1, 
                    context.format
                );
            }

            return context;
        }

        public bool LaunchCareerMatch()
        {
            CareerProfile profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
            TournamentProgress tournament = (TournamentManager.Instance != null) ? TournamentManager.Instance.ActiveTournament : null;

            return LaunchCareerMatch(profile, tournament);
        }

        public bool LaunchCareerMatch(CareerProfile profile, TournamentProgress tournament)
        {
            if (profile == null)
            {
                Debug.LogWarning("[CareerMatchLauncher] Cannot launch match without an active CareerProfile.");
                return false;
            }

            // Verify tournament is not locked
            if (tournament != null && !string.IsNullOrEmpty(tournament.tournamentId))
            {
                if (profile.unlockedTournaments != null && !profile.unlockedTournaments.Contains(tournament.tournamentId))
                {
                    Debug.LogWarning(string.Format("[CareerMatchLauncher] Tournament '{0}' is locked.", tournament.tournamentId));
                    return false;
                }
            }

            ActiveContext = CreateMatchContext(profile, tournament);
            ActiveContext.isMatchInProgress = true;
            ActiveContext.isResultProcessed = false;

            Debug.Log(string.Format("[CareerMatchLauncher] Launching Match: {0} vs {1} at {2}", 
                ActiveContext.homeTeamName, 
                ActiveContext.awayTeamName, 
                ActiveContext.venue));

            if (tournament != null)
            {
                tournament.UpdateState(TournamentState.InProgress);
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.MatchLoading);
            }

            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene(SceneController.SceneMatch);
            }

            if (OnMatchLaunched != null)
            {
                OnMatchLaunched(ActiveContext);
            }

            return true;
        }

        public void SetActiveContextForTesting(CareerMatchContext context)
        {
            ActiveContext = context;
        }

        /// <summary>
        /// Called from the Match Preview screen to actually start the match.
        /// The context was already prepared by LaunchCareerMatch (which navigated to preview first).
        /// This method simply transitions state and loads the match scene.
        /// Returns false if no context is available (safety guard).
        /// </summary>
        public bool LaunchMatchFromPreview()
        {
            if (ActiveContext == null)
            {
                Debug.LogWarning("[CareerMatchLauncher] LaunchMatchFromPreview: No active context. Preparing fallback context.");
                return LaunchCareerMatch();
            }

            Debug.Log(string.Format("[CareerMatchLauncher] LaunchMatchFromPreview: Starting {0} vs {1}",
                ActiveContext.homeTeamName, ActiveContext.awayTeamName));

            ActiveContext.isMatchInProgress = true;
            ActiveContext.isResultProcessed = false;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.PlayingMatch);
            }

            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene(SceneController.SceneMatch);
            }

            if (OnMatchLaunched != null)
            {
                OnMatchLaunched(ActiveContext);
            }

            return true;
        }
    }
}
