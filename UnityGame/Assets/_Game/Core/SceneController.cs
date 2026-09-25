using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CricketGame.Core
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }

        public const string SceneBootstrap = "Bootstrap";
        public const string SceneMainMenu = "MainMenu";
        public const string SceneCareerHub = "CareerHub";
        public const string SceneTournament = "Tournament";
        public const string SceneMatchPreview = "MatchPreview";
        public const string SceneTraining = "Training";
        public const string SceneMatch = "Match";
        public const string SceneCareerMatchResult = "CareerMatchResult";

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

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

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            Debug.Log(string.Format("[SceneController] Loading scene: {0}", sceneName));
            if (OnSceneLoadStarted != null) OnSceneLoadStarted(sceneName);

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncOp.isDone)
            {
                yield return null;
            }

            Debug.Log(string.Format("[SceneController] Scene loaded successfully: {0}", sceneName));
            if (OnSceneLoadCompleted != null) OnSceneLoadCompleted(sceneName);
        }
    }
}
