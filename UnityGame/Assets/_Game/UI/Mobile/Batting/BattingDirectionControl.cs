using System;
using UnityEngine;
using CricketGame.UI.Mobile.Common;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Batting
{
    public class BattingDirectionControl : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick joystick;
        private Vector2 currentDirection = Vector2.zero;

        public Vector2 CurrentDirection { get { return currentDirection; } }
        public event Action<Vector2> OnDirectionChanged;

        private void Awake()
        {
            if (joystick == null)
            {
                joystick = GetComponentInChildren<VirtualJoystick>();
            }

            if (joystick != null)
            {
                joystick.OnInputChanged += HandleJoystickInput;
            }
        }

        public void HandleJoystickInput(Vector2 dir)
        {
            currentDirection = dir;
            if (OnDirectionChanged != null)
            {
                OnDirectionChanged(currentDirection);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBattingDirection(currentDirection);
            }
        }

        public void SetDirectionDirect(Vector2 dir)
        {
            currentDirection = dir.normalized;
            if (OnDirectionChanged != null)
            {
                OnDirectionChanged(currentDirection);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendBattingDirection(currentDirection);
            }
        }
    }
}
