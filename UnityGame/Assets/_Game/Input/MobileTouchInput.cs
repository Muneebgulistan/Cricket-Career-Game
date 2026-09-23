using System;
using UnityEngine;
using CricketGame.Gameplay.Batting;

namespace CricketGame.MobileInput
{
    public enum SwipeDirection
    {
        None,
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    public class MobileTouchInput : MonoBehaviour
    {
        private MobileInputSettings settings;
        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isTrackingTouch;

        public event Action<SwipeDirection, Vector2> OnSwipeDetected;
        public event Action<Vector2> OnTapDetected;
        public event Action<Vector2> OnLongPressDetected;

        public void Initialize(MobileInputSettings inputSettings)
        {
            settings = inputSettings != null ? inputSettings : MobileInputSettings.Default();
        }

        private void Awake()
        {
            if (settings == null)
            {
                settings = MobileInputSettings.Default();
            }
        }

        private void Update()
        {
            HandleTouches();
        }

        private void HandleTouches()
        {
            // Handle Unity touches if available
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    BeginTouch(touch.position, Time.time);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    EndTouch(touch.position, Time.time);
                }
            }
            // Mouse fallback for Editor / Testing
            else
            {
                if (UnityEngine.Input.GetMouseButtonDown(0))
                {
                    BeginTouch(new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y), Time.time);
                }
                else if (UnityEngine.Input.GetMouseButtonUp(0))
                {
                    EndTouch(new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y), Time.time);
                }
            }
        }

        public void BeginTouch(Vector2 position, float time)
        {
            touchStartPosition = position;
            touchStartTime = time;
            isTrackingTouch = true;
        }

        public void EndTouch(Vector2 endPosition, float time)
        {
            if (!isTrackingTouch) return;
            isTrackingTouch = false;

            float duration = time - touchStartTime;
            Vector2 delta = endPosition - touchStartPosition;
            float distance = delta.magnitude;

            if (distance >= settings.minSwipeDistance && duration <= settings.maxSwipeDuration)
            {
                SwipeDirection dir = CalculateSwipeDirection(delta.normalized);
                if (OnSwipeDetected != null)
                {
                    OnSwipeDetected(dir, delta.normalized);
                }
            }
            else if (distance < settings.minSwipeDistance)
            {
                if (duration > settings.maxSwipeDuration)
                {
                    if (OnLongPressDetected != null)
                    {
                        OnLongPressDetected(endPosition);
                    }
                }
                else
                {
                    if (OnTapDetected != null)
                    {
                        OnTapDetected(endPosition);
                    }
                }
            }
        }

        public SwipeDirection CalculateSwipeDirection(Vector2 directionNormalized)
        {
            float x = directionNormalized.x;
            float y = directionNormalized.y;

            // Diagonal thresholds
            float diagThreshold = 0.38f;

            if (y > diagThreshold && x > diagThreshold)
            {
                return SwipeDirection.UpRight;
            }
            if (y > diagThreshold && x < -diagThreshold)
            {
                return SwipeDirection.UpLeft;
            }
            if (y < -diagThreshold && x > diagThreshold)
            {
                return SwipeDirection.DownRight;
            }
            if (y < -diagThreshold && x < -diagThreshold)
            {
                return SwipeDirection.DownLeft;
            }

            // Cardinal directions
            if (Math.Abs(x) > Math.Abs(y))
            {
                return x > 0f ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                return y > 0f ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }

        public BattingShotType MapSwipeToBattingShot(SwipeDirection swipe, out bool isLofted, out bool isDefensive)
        {
            isLofted = false;
            isDefensive = false;

            switch (swipe)
            {
                case SwipeDirection.Up:
                    isLofted = true;
                    return BattingShotType.StraightLoft;
                case SwipeDirection.Down:
                    isDefensive = true;
                    return BattingShotType.Defensive;
                case SwipeDirection.Left:
                    return BattingShotType.Cut;
                case SwipeDirection.Right:
                    return BattingShotType.Pull;
                case SwipeDirection.UpLeft:
                    isLofted = true;
                    return BattingShotType.CoverDrive;
                case SwipeDirection.UpRight:
                    isLofted = true;
                    return BattingShotType.Hook;
                case SwipeDirection.DownLeft:
                    return BattingShotType.CoverDrive;
                case SwipeDirection.DownRight:
                    return BattingShotType.Sweep;
                default:
                    return BattingShotType.StraightDrive;
            }
        }
    }
}
