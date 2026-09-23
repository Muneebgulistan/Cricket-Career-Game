using System.Collections;
using UnityEngine;
using CricketGame.SaveSystem;
using CricketGame.Career;
using CricketGame.Audio;
using CricketGame.Input;

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
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (autoLoadMainMenu)
            {
                StartCoroutine(BootstrapSequence());
            }
        }

        private void InitializeCoreSubsystems()
        {
            Debug.Log("[GameBootstrap] Initializing Cricket Career Game Subsystems...");

            // Ensure GameStateManager exists
            if (GetComponent<GameStateManager>() == null)
            {
                gameObject.AddComponent<GameStateManager>();
            }

            // Ensure SceneController exists
            if (GetComponent<SceneController>() == null)
            {
                gameObject.AddComponent<SceneController>();
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

            Debug.Log("[GameBootstrap] All Core Subsystems successfully initialized.");
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

            Debug.Log("[GameBootstrap] Transitioning from Bootstrap to MainMenu scene.");
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.MainMenu);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneMainMenu);
        }
    }
}
