using System.Collections;
using UnityEngine;
using CricketGame.SaveSystem;
using CricketGame.Career;
using CricketGame.Audio;
using CricketGame.Input;
using UnityEngine.SceneManagement;

namespace CricketGame.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [Header("Initial Configuration")]
        [SerializeField] private bool autoLoadMainMenu = true;
        [SerializeField] private float splashDelaySeconds = 1.0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCoreSubsystems();
                SceneManager.sceneLoaded += HandleSceneLoaded;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != SceneController.SceneMatch) return;
            if (FindAnyObjectByType<CricketGame.Gameplay.Match.PlayableCareerMatch>() == null)
                new GameObject("Playable Career Match").AddComponent<CricketGame.Gameplay.Match.PlayableCareerMatch>();
        }

        private void Start()
        {
            // Keep mobile frame pacing stable; Unity ignores vSyncCount on mobile.
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = GameConstants.TargetFrameRate;

            if (autoLoadMainMenu)
            {
                StartCoroutine(BootstrapSequence());
            }
        }

        private void InitializeCoreSubsystems()
        {
            CricketGame.Core.CricketLogger.Log("[GameBootstrap] Initializing Cricket Career Game Subsystems...");

            // Ensure GameStateManager exists
            if (GetComponent<GameStateManager>() == null)
            {
                gameObject.AddComponent<GameStateManager>();
            }

            if (GetComponent<GameLifecycleManager>() == null)
            {
                gameObject.AddComponent<GameLifecycleManager>();
            }

            // Ensure SceneController exists
            if (GetComponent<SceneController>() == null)
            {
                gameObject.AddComponent<SceneController>();
            }

            if (GetComponent<ObjectPoolManager>() == null)
            {
                gameObject.AddComponent<ObjectPoolManager>();
            }

            // Ensure SaveManager exists
            if (GetComponent<SaveManager>() == null)
            {
                gameObject.AddComponent<SaveManager>();
            }

            // Ensure CareerManager exists
            if (GetComponent<CareerManager>() == null)
            {
                gameObject.AddComponent<CareerManager>();
            }

            // Ensure AudioManager exists
            if (GetComponent<AudioManager>() == null)
            {
                gameObject.AddComponent<AudioManager>();
            }

            // Ensure TournamentManager exists
            if (GetComponent<CricketGame.Career.Tournaments.TournamentManager>() == null)
            {
                gameObject.AddComponent<CricketGame.Career.Tournaments.TournamentManager>();
            }

            // Ensure CareerMatchLauncher exists
            if (GetComponent<CricketGame.Career.MatchIntegration.CareerMatchLauncher>() == null)
            {
                gameObject.AddComponent<CricketGame.Career.MatchIntegration.CareerMatchLauncher>();
            }

            // Ensure CareerMatchCompletionPipeline exists
            if (GetComponent<CricketGame.Career.MatchIntegration.CareerMatchCompletionPipeline>() == null)
            {
                gameObject.AddComponent<CricketGame.Career.MatchIntegration.CareerMatchCompletionPipeline>();
            }

            CricketGame.Core.CricketLogger.Log("[GameBootstrap] All Core Subsystems successfully initialized.");
        }

        private IEnumerator BootstrapSequence()
        {
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.Booting);

            // Attempt to load existing career if available
            SaveManager saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.HasSaveFile())
            {
                if (CareerManager.Instance != null) CareerManager.Instance.LoadExistingCareer();
            }

            yield return new WaitForSeconds(splashDelaySeconds);

            CricketGame.Core.CricketLogger.Log("[GameBootstrap] Transitioning from Bootstrap to MainMenu scene.");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.MainMenu);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneMainMenu);
        }
    }
}
