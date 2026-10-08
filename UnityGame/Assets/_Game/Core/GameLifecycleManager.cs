using UnityEngine;
using CricketGame.UI;

namespace CricketGame.Core
{
    /// <summary>
    /// Manages mobile application lifecycle events including pause/resume,
    /// backgrounding, focus changes, and frame rate constraints.
    /// Ensures active matches enter a paused state when the user backgrounds the app.
    /// </summary>
    public class GameLifecycleManager : MonoBehaviour
    {
        public static GameLifecycleManager Instance { get; private set; }

        private bool isPaused = false;
        private bool pausedLiveMatch = false;
        public bool IsPaused { get { return isPaused; } }

        public static void SetInstanceForTesting(GameLifecycleManager instance)
        {
            Instance = instance;
        }

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

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            ApplyPerformanceSettings();
        }

        public void ApplyPerformanceSettings()
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = GameConstants.TargetFrameRate;
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) HandleApplicationSuspension();
            else HandleApplicationResumed();
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                HandleApplicationSuspension();
            }
            else
            {
                HandleApplicationResumed();
            }
        }

        public void HandleApplicationSuspension()
        {
            if (pausedLiveMatch) return;

            GameStateManager stateManager = GameStateManager.Instance;
            CricketGame.Gameplay.Match.MatchManager matchController =
                FindAnyObjectByType<CricketGame.Gameplay.Match.MatchManager>();
            bool controllerInLiveState = matchController != null && matchController.Controller != null &&
                (matchController.Controller.State == CricketGame.Gameplay.Match.MatchState.Innings1 ||
                 matchController.Controller.State == CricketGame.Gameplay.Match.MatchState.Innings2);
            bool gameStateInLiveMatch = stateManager != null && stateManager.CurrentState == GameState.PlayingMatch;
            if (!controllerInLiveState && !gameStateInLiveMatch) return;

            pausedLiveMatch = true;
            isPaused = true;
            CricketLogger.Log("[GameLifecycleManager] Application backgrounded/suspended.");

            if (controllerInLiveState)
            {
                matchController.PauseMatch();
            }

            if (gameStateInLiveMatch)
            {
                if (CricketGame.Gameplay.MatchManager.Instance != null)
                    CricketGame.Gameplay.MatchManager.Instance.PauseMatch();
                else
                    stateManager.ChangeState(GameState.MatchPaused);
            }

            // Also freezes matches that use the runtime-built playable match scene.
            Time.timeScale = 0f;
            CricketLogger.Log("[GameLifecycleManager] Match automatically paused on suspension.");
        }

        public void HandleApplicationResumed()
        {
            if (!pausedLiveMatch) return;
            pausedLiveMatch = false;
            isPaused = false;
            CricketLogger.Log("[GameLifecycleManager] Application resumed.");

            CricketGame.Gameplay.Match.MatchManager matchController =
                FindAnyObjectByType<CricketGame.Gameplay.Match.MatchManager>();
            if (matchController != null && matchController.Controller != null &&
                matchController.Controller.State == CricketGame.Gameplay.Match.MatchState.Paused)
                matchController.ResumeMatch();

            GameStateManager stateManager = GameStateManager.Instance;
            if (stateManager != null && stateManager.CurrentState == GameState.MatchPaused)
            {
                if (CricketGame.Gameplay.MatchManager.Instance != null)
                    CricketGame.Gameplay.MatchManager.Instance.ResumeMatch();
                else
                    stateManager.ChangeState(GameState.PlayingMatch);
            }
            Time.timeScale = 1f;
        }

        public void ResumeMatch()
        {
            HandleApplicationResumed();
            CricketLogger.Log("[GameLifecycleManager] Match manually resumed.");
        }
    }
}
