using UnityEngine;

namespace CricketGame.Gameplay.Batting
{
    public class BattingInput : MonoBehaviour, IBattingInput
    {
        [Header("Direct Shot Input State")]
        [SerializeField] private bool swingRequested = false;
        [SerializeField] private BattingShotType requestedShotType = BattingShotType.StraightDrive;
        [SerializeField] private Vector2 directionInput = Vector2.zero;
        [SerializeField] private bool isLofted = false;
        [SerializeField] private bool isDefensive = false;

        public bool IsSwingRequested { get { return swingRequested; } }
        public BattingShotType RequestedShotType { get { return requestedShotType; } }
        public Vector2 DirectionInput { get { return directionInput; } }
        public bool IsLofted { get { return isLofted; } }
        public bool IsDefensive { get { return isDefensive; } }

        public void ConsumeSwingRequest()
        {
            swingRequested = false;
        }

        public void PollInput()
        {
            // Modifiers
            bool wKey = UnityEngine.Input.GetKey(KeyCode.W);
            bool sKey = UnityEngine.Input.GetKey(KeyCode.S);
            bool aKey = UnityEngine.Input.GetKey(KeyCode.A);
            bool dKey = UnityEngine.Input.GetKey(KeyCode.D);

            // Compute direction vector from WASD
            float x = 0f;
            if (dKey) x += 1f;
            if (aKey) x -= 1f;

            float y = 0f;
            if (wKey) y += 1f;
            if (sKey) y -= 1f;

            if (x != 0f || y != 0f)
            {
                directionInput = new Vector2(x, y).normalized;
            }
            else
            {
                directionInput = Vector2.zero;
            }

            isLofted = wKey;
            isDefensive = sKey;

            // Direct Shot Keys (1 to 7)
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                TriggerDefensiveShot();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                TriggerShot(BattingShotType.StraightDrive, new Vector2(0f, 1f), false);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                TriggerShot(BattingShotType.CoverDrive, new Vector2(-0.7f, 0.7f), false);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
            {
                TriggerShot(BattingShotType.Pull, new Vector2(0.8f, 0.2f), false);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5))
            {
                TriggerShot(BattingShotType.Cut, new Vector2(-0.9f, 0.2f), false);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha6))
            {
                TriggerShot(BattingShotType.Sweep, new Vector2(0.9f, -0.3f), false);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha7))
            {
                TriggerShot(BattingShotType.LoftedDrive, new Vector2(0f, 1f), true);
            }

            // Swing key: Space
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                RequestSwing();
            }
        }

        // ----------------------------------------------------
        // Mobile / External API Hooks
        // ----------------------------------------------------
        public void TriggerShot(BattingShotType shotType, Vector2 direction, bool lofted)
        {
            requestedShotType = shotType;
            directionInput = direction;
            isLofted = lofted;
            isDefensive = (shotType == BattingShotType.Defensive);
            swingRequested = true;
        }

        public void SetShotDirection(Vector2 direction)
        {
            directionInput = direction;
        }

        public void SetShotModifier(bool lofted, bool defensive)
        {
            isLofted = lofted;
            isDefensive = defensive;
            if (isDefensive)
            {
                requestedShotType = BattingShotType.Defensive;
            }
        }

        public void TriggerDefensiveShot()
        {
            requestedShotType = BattingShotType.Defensive;
            isDefensive = true;
            isLofted = false;
            directionInput = Vector2.zero;
            swingRequested = true;
        }

        public void RequestSwing()
        {
            // If shot type was not explicitly picked, infer from direction
            if (isDefensive)
            {
                requestedShotType = BattingShotType.Defensive;
            }
            else if (directionInput.x < -0.4f)
            {
                requestedShotType = isLofted ? BattingShotType.CoverDrive : (directionInput.y < 0.3f ? BattingShotType.Cut : BattingShotType.CoverDrive);
            }
            else if (directionInput.x > 0.4f)
            {
                requestedShotType = isLofted ? BattingShotType.Hook : (directionInput.y < 0.3f ? BattingShotType.Sweep : BattingShotType.Pull);
            }
            else
            {
                requestedShotType = isLofted ? BattingShotType.StraightLoft : BattingShotType.StraightDrive;
            }

            swingRequested = true;
        }
    }
}
