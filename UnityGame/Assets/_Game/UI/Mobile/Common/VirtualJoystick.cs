using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CricketGame.UI.Mobile.Common
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick Visuals")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;

        [Header("Settings")]
        [SerializeField] private float maxRadius = 75f;
        [SerializeField] private float deadZone = 0.1f;
        [SerializeField] private bool dynamicPosition = false;

        private Vector2 inputVector = Vector2.zero;
        private Vector2 defaultBackgroundPosition;
        private bool isInteracting = false;

        public Vector2 Input { get { return inputVector; } }
        public bool IsInteracting { get { return isInteracting; } }

        public event Action<Vector2> OnInputChanged;

        private void Awake()
        {
            if (background != null)
            {
                defaultBackgroundPosition = background.anchoredPosition;
            }
        }

        public void SetInputVector(Vector2 input)
        {
            float mag = input.magnitude;
            if (mag < deadZone)
            {
                inputVector = Vector2.zero;
            }
            else
            {
                inputVector = input.normalized * Mathf.Clamp01((mag - deadZone) / (1f - deadZone));
            }

            UpdateHandleVisual();

            if (OnInputChanged != null)
            {
                OnInputChanged(inputVector);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isInteracting = true;
            if (dynamicPosition && background != null)
            {
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background.parent as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPos);
                background.anchoredPosition = localPos;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null) return;

            Vector2 position;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                eventData.pressEventCamera,
                out position))
            {
                float radius = maxRadius > 0f ? maxRadius : 75f;
                Vector2 clamped = Vector2.ClampMagnitude(position, radius);
                Vector2 normalized = clamped / radius;

                SetInputVector(normalized);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isInteracting = false;
            inputVector = Vector2.zero;
            UpdateHandleVisual();

            if (dynamicPosition && background != null)
            {
                background.anchoredPosition = defaultBackgroundPosition;
            }

            if (OnInputChanged != null)
            {
                OnInputChanged(Vector2.zero);
            }
        }

        private void UpdateHandleVisual()
        {
            if (handle != null)
            {
                float radius = maxRadius > 0f ? maxRadius : 75f;
                handle.anchoredPosition = inputVector * radius;
            }
        }

        public void ResetJoystick()
        {
            isInteracting = false;
            inputVector = Vector2.zero;
            UpdateHandleVisual();
            if (dynamicPosition && background != null)
            {
                background.anchoredPosition = defaultBackgroundPosition;
            }
        }
    }
}
