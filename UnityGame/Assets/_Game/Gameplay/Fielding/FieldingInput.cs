using UnityEngine;

namespace CricketGame.Gameplay.Fielding
{
    public class FieldingInput : MonoBehaviour, IFieldingInput
    {
        [Header("State")]
        [SerializeField] private bool isFieldingActive = true;
        [SerializeField] private Vector2 moveDirection = Vector2.zero;
        [SerializeField] private bool wantsSprint = false;
        [SerializeField] private bool isPickupRequested = false;
        [SerializeField] private bool isCatchRequested = false;
        [SerializeField] private bool isThrowRequested = false;
        [SerializeField] private FieldingTarget selectedThrowTarget = FieldingTarget.WicketKeeper;

        public bool IsFieldingActive { get { return isFieldingActive; } }
        public Vector2 MoveDirection { get { return moveDirection; } }
        public bool WantsSprint { get { return wantsSprint; } }
        public bool IsPickupRequested { get { return isPickupRequested; } }
        public bool IsCatchRequested { get { return isCatchRequested; } }
        public bool IsThrowRequested { get { return isThrowRequested; } }
        public FieldingTarget SelectedThrowTarget { get { return selectedThrowTarget; } }

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            // F = toggle / activate fielding
            if (UnityEngine.Input.GetKeyDown(KeyCode.F))
            {
                isFieldingActive = !isFieldingActive;
            }

            // C = attempt catch
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
            {
                AttemptCatch();
            }

            // E = pickup
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                AttemptPickup();
            }

            // T = throw
            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                TriggerThrow();
            }

            // 1 = throw to wicketkeeper
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectThrowTarget(FieldingTarget.WicketKeeper);
            }
            // 2 = throw to bowler
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectThrowTarget(FieldingTarget.Bowler);
            }
            // 3 = throw to striker end
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectThrowTarget(FieldingTarget.StrikerEnd);
            }
            // 4 = throw to non-striker end
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
            {
                SelectThrowTarget(FieldingTarget.NonStrikerEnd);
            }

            // Manual movement keys (WASD / Arrows)
            float h = 0f;
            float v = 0f;
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow)) v += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow)) v -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) h += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)) h -= 1f;

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                bool sprint = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
                MoveFieldPlayer(new Vector2(h, v).normalized, sprint);
            }
        }

        // ----------------------------------------------------
        // Mobile / External API
        // ----------------------------------------------------
        public void MoveFieldPlayer(Vector2 direction, bool sprint)
        {
            moveDirection = direction;
            wantsSprint = sprint;
        }

        public void AttemptPickup()
        {
            isPickupRequested = true;
        }

        public void AttemptCatch()
        {
            isCatchRequested = true;
        }

        public void SelectThrowTarget(FieldingTarget target)
        {
            selectedThrowTarget = target;
        }

        public void TriggerThrow()
        {
            isThrowRequested = true;
        }

        public void ActivateFielding(bool activate)
        {
            isFieldingActive = activate;
        }

        public void ConsumePickupRequest()
        {
            isPickupRequested = false;
        }

        public void ConsumeCatchRequest()
        {
            isCatchRequested = false;
        }

        public void ConsumeThrowRequest()
        {
            isThrowRequested = false;
        }

        public void ResetInput()
        {
            moveDirection = Vector2.zero;
            wantsSprint = false;
            isPickupRequested = false;
            isCatchRequested = false;
            isThrowRequested = false;
        }
    }
}
