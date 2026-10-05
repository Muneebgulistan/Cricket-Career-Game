using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Core;

namespace CricketGame.UI.Match
{
    /// <summary>Reusable score popup that returns itself to the floating-text pool.</summary>
    public sealed class PooledFloatingText : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField, Min(0.1f)] private float lifetime = 1.1f;
        [SerializeField] private float riseSpeed = 36f;

        private CanvasGroup canvasGroup;
        private Coroutine animation;

        private void Awake()
        {
            if (label == null) label = GetComponentInChildren<Text>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Show(string message, Vector3 screenPosition, Color color)
        {
            if (label != null)
            {
                label.text = message;
                label.color = color;
            }

            transform.position = screenPosition;
            canvasGroup.alpha = 1f;
            if (animation != null) StopCoroutine(animation);
            animation = StartCoroutine(AnimateAndReturn());
        }

        private IEnumerator AnimateAndReturn()
        {
            float elapsed = 0f;
            RectTransform rect = transform as RectTransform;
            while (elapsed < lifetime)
            {
                float delta = Time.unscaledDeltaTime;
                elapsed += delta;
                if (rect != null) rect.anchoredPosition += Vector2.up * (riseSpeed * delta);
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / lifetime);
                yield return null;
            }

            animation = null;
            if (ObjectPoolManager.Instance != null) ObjectPoolManager.Instance.Release(gameObject);
        }
    }
}
