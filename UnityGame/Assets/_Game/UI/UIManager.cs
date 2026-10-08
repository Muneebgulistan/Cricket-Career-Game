using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CricketGame.UI
{
    /// <summary>
    /// Master UI Architecture manager providing stack-based panel navigation,
    /// mobile CanvasScaler configuration, safe-area notch adaptation,
    /// and asynchronous scene transitions with loading progress display.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Canvas & Mobile Scaler Defaults")]
        [SerializeField] private Vector2 defaultReferenceResolution = new Vector2(1920, 1080);
        [SerializeField] [Range(0f, 1f)] private float defaultMatchWidthOrHeight = 0.5f;

        [Header("Loading Screen Overlay")]
        [SerializeField] private GameObject loadingScreenPanel;
        [SerializeField] private Slider loadingProgressBar;
        [SerializeField] private Text loadingStatusText;
        [SerializeField] private Text loadingTipText;

        private readonly Stack<GameObject> panelStack = new Stack<GameObject>();
        private bool isLoadingScene = false;
        private float loadingProgress = 0f;

        private static readonly string[] CricketTips = new string[]
        {
            "Watch the bowler's hand release to predict swing direction.",
            "Use defensive blocks against sharp bouncers to preserve your wicket.",
            "Lofted shots over the infield are highest percentage during Powerplay overs.",
            "Maintain high player fitness to avoid stamina drops in death overs.",
            "Green pitches assist seam movement early on; flat pitches favor batsmen."
        };

        public int StackDepth { get { return panelStack.Count; } }
        public bool IsLoadingScene { get { return isLoadingScene; } }
        public float LoadingProgress { get { return loadingProgress; } }
        public GameObject LoadingScreenPanel { get { return loadingScreenPanel; } set { loadingScreenPanel = value; } }

        public event Action<GameObject> OnPanelPushed;
        public event Action<GameObject> OnPanelPopped;
        public event Action<float> OnLoadingProgressUpdated;
        public event Action<string> OnSceneLoaded;

        public static void SetInstanceForTesting(UIManager inst)
        {
            Instance = inst;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            ConfigureAllCanvases();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ==========================================
        // 1. STACK-BASED PANEL NAVIGATION
        // ==========================================

        public void PushPanel(GameObject panel)
        {
            if (panel == null) return;

            if (panelStack.Count > 0)
            {
                GameObject currentTop = panelStack.Peek();
                if (currentTop != null)
                {
                    currentTop.SetActive(false);
                }
            }

            panelStack.Push(panel);
            panel.SetActive(true);

            if (OnPanelPushed != null)
            {
                OnPanelPushed(panel);
            }
        }

        public GameObject PopPanel()
        {
            if (panelStack.Count == 0) return null;

            GameObject popping = panelStack.Pop();
            if (popping != null)
            {
                popping.SetActive(false);
            }

            if (panelStack.Count > 0)
            {
                GameObject newTop = panelStack.Peek();
                if (newTop != null)
                {
                    newTop.SetActive(true);
                }
            }

            if (OnPanelPopped != null)
            {
                OnPanelPopped(popping);
            }

            return popping;
        }

        public GameObject PeekPanel()
        {
            if (panelStack.Count == 0) return null;
            return panelStack.Peek();
        }

        public void ClearStack()
        {
            while (panelStack.Count > 0)
            {
                GameObject p = panelStack.Pop();
                if (p != null) p.SetActive(false);
            }
        }

        // ==========================================
        // 2. MOBILE CANVAS SCALER & SAFE-AREA NOTCH
        // ==========================================

        public void ConfigureCanvasScaler(CanvasScaler scaler, Vector2 referenceResolution, float matchWidthOrHeight = 0.5f)
        {
            if (scaler == null) return;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
        }

        public void ConfigureAllCanvases()
        {
            CanvasScaler[] scalers = FindAllObjectsUnsorted<CanvasScaler>();
            if (scalers != null)
            {
                foreach (CanvasScaler s in scalers)
                {
                    ConfigureCanvasScaler(s, defaultReferenceResolution, defaultMatchWidthOrHeight);
                }
            }
        }

        public void ApplySafeArea(RectTransform targetRect)
        {
            if (targetRect == null) return;
            ApplySafeArea(targetRect, Screen.safeArea, Screen.width, Screen.height, true, true);
        }

        public void ApplySafeArea(RectTransform targetRect, Rect safeArea, int screenWidth, int screenHeight, bool conformX = true, bool conformY = true)
        {
            if (targetRect == null) return;
            if (screenWidth <= 0 || screenHeight <= 0) return;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= screenWidth;
            anchorMin.y /= screenHeight;
            anchorMax.x /= screenWidth;
            anchorMax.y /= screenHeight;

            if (!conformX)
            {
                anchorMin.x = 0f;
                anchorMax.x = 1f;
            }

            if (!conformY)
            {
                anchorMin.y = 0f;
                anchorMax.y = 1f;
            }

            targetRect.anchorMin = anchorMin;
            targetRect.anchorMax = anchorMax;
        }

        // ==========================================
        // 3. ASYNCHRONOUS SCENE LOADING & TRANSITION
        // ==========================================

        public void LoadSceneAsync(string sceneName, Action onComplete = null)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName, onComplete));
        }

        public IEnumerator LoadSceneCoroutine(string sceneName, Action onComplete = null)
        {
            isLoadingScene = true;
            loadingProgress = 0f;

            if (loadingScreenPanel != null)
            {
                loadingScreenPanel.SetActive(true);
            }

            if (loadingTipText != null && CricketTips.Length > 0)
            {
                loadingTipText.text = CricketTips[UnityEngine.Random.Range(0, CricketTips.Length)];
            }

            UpdateLoadingUI(0.1f, "Initializing Stadium & Assets...");

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOp != null)
            {
                while (!asyncOp.isDone)
                {
                    float p = Mathf.Clamp01(asyncOp.progress / 0.9f);
                    UpdateLoadingUI(p, string.Format("Loading {0}... {1}%", sceneName, Mathf.RoundToInt(p * 100f)));
                    yield return null;
                }
            }

            UpdateLoadingUI(1.0f, "Match Environment Ready!");

            if (loadingScreenPanel != null)
            {
                loadingScreenPanel.SetActive(false);
            }

            isLoadingScene = false;

            if (OnSceneLoaded != null)
            {
                OnSceneLoaded(sceneName);
            }

            if (onComplete != null)
            {
                onComplete();
            }
        }

        public void UpdateLoadingUI(float progress, string status)
        {
            loadingProgress = progress;

            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = progress;
            }

            if (loadingStatusText != null)
            {
                loadingStatusText.text = status;
            }

            if (OnLoadingProgressUpdated != null)
            {
                OnLoadingProgressUpdated(progress);
            }
        }

        // Helper to find objects in scene without deprecated methods
        private T[] FindAllObjectsUnsorted<T>() where T : Component
        {
            return UnityEngine.Object.FindObjectsByType<T>();
        }
    }
}
