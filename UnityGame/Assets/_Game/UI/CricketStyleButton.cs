using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.UI
{
    /// <summary>
    /// Reusable cricket-style button component.
    /// Handles pressed/hover/disabled visual states using color transitions.
    /// Attach alongside a Button component. Requires an Image on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class CricketStyleButton : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.12f, 0.58f, 0.25f);
        [SerializeField] private Color pressedColor = new Color(0.08f, 0.40f, 0.18f);
        [SerializeField] private Color disabledColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color highlightColor = new Color(0.18f, 0.70f, 0.32f);

        [Header("Transition")]
        [SerializeField] private float transitionDuration = 0.08f;

        [Header("Label")]
        [SerializeField] private Text labelText;
        [SerializeField] private Color labelNormalColor = Color.white;
        [SerializeField] private Color labelDisabledColor = new Color(0.7f, 0.7f, 0.7f);

        private Button button;
        private Image background;
        private Color currentTarget;
        private Coroutine colorRoutine;

        // --------------------------------------------------
        // Lifecycle
        // --------------------------------------------------

        private void Awake()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            currentTarget = normalColor;
            if (background != null) background.color = normalColor;
        }

        private void OnEnable()
        {
            RefreshButtonState();
        }

        private void Update()
        {
            // Re-check interactable state each frame (lightweight)
            RefreshButtonState();
        }

        // --------------------------------------------------
        // State Management
        // --------------------------------------------------

        private void RefreshButtonState()
        {
            if (button == null) return;

            Color target = button.interactable ? normalColor : disabledColor;
            Color textTarget = button.interactable ? labelNormalColor : labelDisabledColor;

            if (labelText != null && labelText.color != textTarget)
            {
                labelText.color = textTarget;
            }

            AnimateToColor(target);
        }

        public void OnPointerDown()
        {
            if (button != null && button.interactable)
                AnimateToColor(pressedColor);
        }

        public void OnPointerUp()
        {
            if (button != null && button.interactable)
                AnimateToColor(normalColor);
        }

        public void OnPointerEnter()
        {
            if (button != null && button.interactable)
                AnimateToColor(highlightColor);
        }

        public void OnPointerExit()
        {
            if (button != null && button.interactable)
                AnimateToColor(normalColor);
        }

        // --------------------------------------------------
        // Color Animation
        // --------------------------------------------------

        private void AnimateToColor(Color target)
        {
            if (background == null) return;
            if (background.color == target) return;

            if (colorRoutine != null)
                StopCoroutine(colorRoutine);

            colorRoutine = StartCoroutine(ColorTransitionRoutine(target));
        }

        private IEnumerator ColorTransitionRoutine(Color target)
        {
            Color start = background.color;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                background.color = Color.Lerp(start, target, t);
                yield return null;
            }

            background.color = target;
            colorRoutine = null;
        }

        // --------------------------------------------------
        // Public API
        // --------------------------------------------------

        public void SetLabel(string text)
        {
            if (labelText != null) labelText.text = text;
        }

        public void SetNormalColor(Color c)
        {
            normalColor = c;
        }
    }
}
