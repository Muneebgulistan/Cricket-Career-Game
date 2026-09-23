using System;
using UnityEngine;

namespace CricketGame.Input
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public event Action<Vector2> OnTap;
        public event Action<Vector2, Vector2> OnSwipe; // startPos, direction

        private Vector2 touchStartPos;
        private bool isSwiping = false;
        private const float MinSwipeDistance = 40f;

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

        private void Update()
        {
            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                touchStartPos = UnityEngine.Input.mousePosition;
                isSwiping = true;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && isSwiping)
            {
                isSwiping = false;
                Vector2 endPos = UnityEngine.Input.mousePosition;
                Vector2 delta = endPos - touchStartPos;

                if (delta.magnitude >= MinSwipeDistance)
                {
                    if (OnSwipe != null) OnSwipe(touchStartPos, delta.normalized);
                }
                else
                {
                    if (OnTap != null) OnTap(endPos);
                }
            }
#else
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 delta = touch.position - touchStartPos;
                    if (delta.magnitude >= MinSwipeDistance)
                    {
                        if (OnSwipe != null) OnSwipe(touchStartPos, delta.normalized);
                    }
                    else
                    {
                        if (OnTap != null) OnTap(touch.position);
                    }
                }
            }
#endif
        }
    }
}
