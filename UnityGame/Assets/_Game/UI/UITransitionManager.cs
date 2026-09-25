using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI
{
    /// <summary>
    /// Lightweight screen-fade transition manager.
    /// Attach to a persistent Canvas in the Bootstrap scene, or spawn once per scene.
    /// </summary>
    public class UITransitionManager : MonoBehaviour
    {
        public static UITransitionManager Instance { get; private set; }

        [Header("Fade Overlay")]
        [SerializeField] private Canvas overlayCanvas;
        [SerializeField] private Image fadeImage;

        [Header("Default Durations (seconds)")]
        [SerializeField] private float defaultFadeDuration = 0.35f;

        private bool isFading = false;

        // --------------------------------------------------
        // Events
        // --------------------------------------------------
        public event Action OnFadeInComplete;
        public event Action OnFadeOutComplete;

        // --------------------------------------------------
        // Lifecycle
        // --------------------------------------------------
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                EnsureOverlayExists();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void EnsureOverlayExists()
        {
            if (overlayCanvas == null)
            {
                GameObject canvasGO = new GameObject("UIFadeCanvas");
                canvasGO.transform.SetParent(transform);
                overlayCanvas = canvasGO.AddComponent<Canvas>();
                overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                overlayCanvas.sortingOrder = 9999;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            if (fadeImage == null)
            {
                GameObject imgGO = new GameObject("FadeImage");
                imgGO.transform.SetParent(overlayCanvas.transform, false);
                fadeImage = imgGO.AddComponent<Image>();
                fadeImage.color = new Color(0f, 0f, 0f, 0f);

                RectTransform rt = imgGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        // --------------------------------------------------
        // Public API
        // --------------------------------------------------

        /// <summary>Fades the screen from black to clear.</summary>
        public void FadeIn(float duration = -1f)
        {
            float dur = (duration < 0f) ? defaultFadeDuration : duration;
            StartCoroutine(FadeRoutine(1f, 0f, dur, OnFadeInComplete));
        }

        /// <summary>Fades the screen from clear to black.</summary>
        public void FadeOut(float duration = -1f)
        {
            float dur = (duration < 0f) ? defaultFadeDuration : duration;
            StartCoroutine(FadeRoutine(0f, 1f, dur, OnFadeOutComplete));
        }

        /// <summary>Fade out, hold briefly, fade back in. Typical scene-change usage.</summary>
        public void FadeInOut(float holdSeconds = 0.1f, float duration = -1f, Action onBlack = null)
        {
            float dur = (duration < 0f) ? defaultFadeDuration : duration;
            StartCoroutine(FadeInOutRoutine(dur, holdSeconds, onBlack));
        }

        public bool IsFading
        {
            get { return isFading; }
        }

        // --------------------------------------------------
        // Coroutines
        // --------------------------------------------------

        private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, Action onComplete)
        {
            isFading = true;
            EnsureOverlayExists();

            float elapsed = 0f;
            Color col = fadeImage.color;
            col.a = fromAlpha;
            fadeImage.color = col;
            fadeImage.raycastTarget = (toAlpha >= 0.5f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                col.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                fadeImage.color = col;
                yield return null;
            }

            col.a = toAlpha;
            fadeImage.color = col;
            fadeImage.raycastTarget = (toAlpha >= 0.5f);
            isFading = false;

            if (onComplete != null)
            {
                onComplete();
            }
        }

        private IEnumerator FadeInOutRoutine(float duration, float holdSeconds, Action onBlack)
        {
            // Fade to black
            yield return StartCoroutine(FadeRoutine(0f, 1f, duration, null));

            if (onBlack != null)
            {
                onBlack();
            }

            yield return new WaitForSeconds(holdSeconds);

            // Fade from black
            yield return StartCoroutine(FadeRoutine(1f, 0f, duration, null));

            if (OnFadeInComplete != null)
            {
                OnFadeInComplete();
            }
        }
    }
}
