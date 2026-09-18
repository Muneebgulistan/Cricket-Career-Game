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

            // Ensure InputManager exists
            if (GetComponent<InputManager>() == null)
            {
                gameObject.AddComponent<InputManager>();
            }

            Debug.Log("[GameBootstrap] All Core Subsystems successfully initialized.");
        }

        private IEnumerator BootstrapSequence()
        {
            GameStateManager.Instance?.ChangeState(GameState.Booting);

            // Attempt to load existing career if available
            SaveManager saveMgr = SaveManager.Instance;
            if (saveMgr != null && saveMgr.HasSaveFile())
            {
                CareerManager.Instance?.LoadExistingCareer();
            }

            yield return new WaitForSeconds(splashDelaySeconds);

            Debug.Log("[GameBootstrap] Transitioning from Bootstrap to MainMenu scene.");
            GameStateManager.Instance?.ChangeState(GameState.MainMenu);
            SceneController.Instance?.LoadScene(SceneController.SceneMainMenu);
        }
    }
}
