using System;
using UnityEngine;
using CricketGame.UI.Mobile.Common;
using CricketGame.MobileInput;

namespace CricketGame.UI.Mobile.Fielding
{
    public class FieldingJoystick : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private bool autoSprint = true;

        private Vector2 moveDirection = Vector2.zero;
        public Vector2 MoveDirection { get { return moveDirection; } }

        public event Action<Vector2, bool> OnFieldingMove;

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

        private void HandleJoystickInput(Vector2 dir)
        {
            moveDirection = dir;
            bool sprint = autoSprint && dir.sqrMagnitude > 0.5f;

            if (OnFieldingMove != null)
            {
                OnFieldingMove(moveDirection, sprint);
            }

            if (MobileInputManager.Instance != null && MobileInputManager.Instance.Router != null)
            {
                MobileInputManager.Instance.Router.SendFieldingMove(moveDirection, sprint);
            }
        }
    }
}
