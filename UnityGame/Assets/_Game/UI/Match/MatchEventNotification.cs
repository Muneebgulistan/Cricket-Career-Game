using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI.Match
{
    /// <summary>
    /// Displays timed overlay notifications for key match events:
    /// FOUR, SIX, WICKET, OUT, DOT BALL, CATCH, etc.
    /// Hooks into ScoringManager events and GameplayAudioEvents.
    /// Uses simple scale + alpha animation — no third-party framework needed.
    /// </summary>
    public class MatchEventNotification : MonoBehaviour
    {
        [Header("Notification Panel")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private Text notificationText;
        [SerializeField] private Image notificationBackground;

        [Header("Display Settings")]
        [SerializeField] private float displayDuration = 1.8f;
        [SerializeField] private float fadeSpeed = 3.5f;

        [Header("Event Colors")]
        [SerializeField] private Color colorSix = new Color(1.0f, 0.85f, 0.0f, 1f);    // Gold
        [SerializeField] private Color colorFour = new Color(0.2f, 0.8f, 0.2f, 1f);   // Green
        [SerializeField] private Color colorWicket = new Color(0.9f, 0.2f, 0.1f, 1f); // Red
        [SerializeField] private Color colorCatch = new Color(0.2f, 0.6f, 0.9f, 1f);  // Blue
        [SerializeField] private Color colorDot = new Color(0.6f, 0.6f, 0.6f, 1f);    // Grey
        [SerializeField] private Color colorDefault = new Color(1f, 1f, 1f, 1f);       // White

        private Coroutine activeRoutine;

        // --------------------------------------------------
        // Lifecycle
        // --------------------------------------------------

        private void Awake()
        {
            if (notificationPanel != null) notificationPanel.SetActive(false);
        }

        private void Start()
        {
            SubscribeToGameplayEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromGameplayEvents();
        }

        private void SubscribeToGameplayEvents()
        {
            CricketGame.Audio.GameplayAudioEvents.OnBoundaryFour += OnFour;
            CricketGame.Audio.GameplayAudioEvents.OnBoundarySix += OnSix;
            CricketGame.Audio.GameplayAudioEvents.OnWicketFall += OnWicket;
            CricketGame.Audio.GameplayAudioEvents.OnCatch += OnCatch;
            CricketGame.Audio.GameplayAudioEvents.OnDotBall += OnDot;
        }

        private void UnsubscribeFromGameplayEvents()
        {
            CricketGame.Audio.GameplayAudioEvents.OnBoundaryFour -= OnFour;
            CricketGame.Audio.GameplayAudioEvents.OnBoundarySix -= OnSix;
            CricketGame.Audio.GameplayAudioEvents.OnWicketFall -= OnWicket;
            CricketGame.Audio.GameplayAudioEvents.OnCatch -= OnCatch;
            CricketGame.Audio.GameplayAudioEvents.OnDotBall -= OnDot;
        }

        // --------------------------------------------------
        // Event Handlers
        // --------------------------------------------------

        private void OnFour()
        {
            ShowNotification("FOUR!", colorFour);
        }

        private void OnSix()
        {
            ShowNotification("SIX!", colorSix);
        }

        private void OnWicket()
        {
            ShowNotification("WICKET!", colorWicket);
        }

        private void OnCatch()
        {
            ShowNotification("CAUGHT!", colorCatch);
        }

        private void OnDot()
        {
            ShowNotification("DOT", colorDot);
        }

        // --------------------------------------------------
        // Public API
        // --------------------------------------------------

        /// <summary>Manually trigger a named notification with a given color.</summary>
        public void ShowNotification(string message, Color bgColor)
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }
            activeRoutine = StartCoroutine(ShowRoutine(message, bgColor));
        }

        // --------------------------------------------------
        // Coroutine
        // --------------------------------------------------

        private IEnumerator ShowRoutine(string message, Color bgColor)
        {
            if (notificationPanel == null) yield break;

            // Set content
            if (notificationText != null) notificationText.text = message;

            // Snap to visible state
            if (notificationBackground != null)
            {
                Color c = bgColor;
                c.a = 0.92f;
                notificationBackground.color = c;
            }

            // Scale up pop
            notificationPanel.SetActive(true);
            notificationPanel.transform.localScale = Vector3.one * 0.7f;

            float elapsed = 0f;
            float scaleInTime = 0.15f;

            while (elapsed < scaleInTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / scaleInTime);
                notificationPanel.transform.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one * 1.05f, t);
                yield return null;
            }

            notificationPanel.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            float fadeElapsed = 0f;
            float fadeDuration = 1f / fadeSpeed;

            if (notificationBackground != null && notificationText != null)
            {
                Color startBg = notificationBackground.color;
                Color startTxt = notificationText.color;

                while (fadeElapsed < fadeDuration)
                {
                    fadeElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(fadeElapsed / fadeDuration);

                    Color bg = startBg;
                    bg.a = Mathf.Lerp(startBg.a, 0f, t);
                    notificationBackground.color = bg;

                    Color txt = startTxt;
                    txt.a = Mathf.Lerp(startTxt.a, 0f, t);
                    notificationText.color = txt;

                    yield return null;
                }

                // Reset alpha for next use
                startBg.a = 0.92f;
                notificationBackground.color = startBg;
                startTxt.a = 1f;
                notificationText.color = startTxt;
            }

            notificationPanel.SetActive(false);
            activeRoutine = null;
        }
    }
}
