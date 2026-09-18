using UnityEngine;
using CricketGame.Core;
using CricketGame.Cricket;
using CricketGame.Career;

namespace CricketGame.Gameplay
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Header("Match Setup")]
        public string battingTeam = "Lahore Eagles U-16";
        public string bowlingTeam = "Karachi Kings U-16";
        public int totalRuns = 142;
        public int wicketsLost = 3;
        public float currentOvers = 14.2f;
        public int maxOvers = 20;

        [Header("Active Batters")]
        public string strikerName = "Muneeb Gulistan*";
        public int strikerRuns = 48;
        public int strikerBalls = 31;
        public string nonStrikerName = "Zayn Malik";
        public int nonStrikerRuns = 32;

        [Header("Active Bowler")]
        public string bowlerName = "Haris Rauf";
        public string bowlerFigures = "3.2-0-24-1";

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

        private void Start()
        {
            GameStateManager.Instance?.ChangeState(GameState.PlayingMatch);
            InitializeMatchSession();
        }

        private void InitializeMatchSession()
        {
            CareerProfile profile = CareerManager.Instance?.ActiveCareer;
            if (profile != null && profile.player != null)
            {
                strikerName = $"{profile.player.name}*";
                battingTeam = profile.player.currentTeam;
            }

            Debug.Log($"[MatchManager] Initialized 3D Match: {battingTeam} vs {bowlingTeam}");
        }

        public void PauseMatch()
        {
            GameStateManager.Instance?.ChangeState(GameState.MatchPaused);
            Time.timeScale = 0f;
            Debug.Log("[MatchManager] Match Paused.");
        }

        public void ResumeMatch()
        {
            GameStateManager.Instance?.ChangeState(GameState.PlayingMatch);
            Time.timeScale = 1f;
            Debug.Log("[MatchManager] Match Resumed.");
        }

        public void EndMatchAndReturnToHub(bool playerWon = true)
        {
            Time.timeScale = 1f;
            GameStateManager.Instance?.ChangeState(GameState.MatchFinished);

            // Record match result into career
            MatchResult result = new MatchResult
            {
                matchId = System.Guid.NewGuid().ToString(),
                homeTeamName = battingTeam,
                awayTeamName = bowlingTeam,
                winnerTeamName = playerWon ? battingTeam : bowlingTeam,
                isPlayerVictory = playerWon,
                manOfTheMatch = strikerName,
                userPerformance = new PlayerMatchPerformance
                {
                    playerName = strikerName.Replace("*", ""),
                    runs = strikerRuns,
                    balls = strikerBalls,
                    fours = 6,
                    sixes = 1,
                    isOut = false,
                    matchRating = 8.5f
                }
            };

            CareerManager.Instance?.RecordMatchOutcome(result);
            GameStateManager.Instance?.ChangeState(GameState.CareerHub);
            SceneController.Instance?.LoadScene(SceneController.SceneCareerHub);
        }
    }
}
