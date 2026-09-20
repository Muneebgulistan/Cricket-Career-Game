using UnityEngine;

namespace CricketGame.Players
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerAttributes playerAttributes;
        [SerializeField] private PlayerRoleController roleController;
        [SerializeField] private PlayerVisual playerVisual;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerAnimationController animationController;

        [Header("Runtime State")]
        [SerializeField] private PlayerRuntimeData runtimeData = new PlayerRuntimeData();

        public PlayerAttributes Attributes { get { return playerAttributes; } }
        public PlayerRoleController RoleController { get { return roleController; } }
        public PlayerVisual Visual { get { return playerVisual; } }
        public PlayerMovement Movement { get { return playerMovement; } }
        public PlayerAnimationController AnimationController { get { return animationController; } }
        public PlayerRuntimeData RuntimeData { get { return runtimeData; } }

        private void Awake()
        {
            if (playerAttributes == null) playerAttributes = GetComponent<PlayerAttributes>();
            if (roleController == null) roleController = GetComponent<PlayerRoleController>();
            if (playerVisual == null) playerVisual = GetComponent<PlayerVisual>();
            if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
            if (animationController == null) animationController = GetComponent<PlayerAnimationController>();
        }

        public void Initialize(PlayerProfile profile, PlayerMatchRole role, bool isUserControlled, bool isAIControlled)
        {
            if (playerAttributes == null) playerAttributes = GetComponent<PlayerAttributes>();
            if (roleController == null) roleController = GetComponent<PlayerRoleController>();
            if (playerVisual == null) playerVisual = GetComponent<PlayerVisual>();
            if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
            if (animationController == null) animationController = GetComponent<PlayerAnimationController>();

            if (playerAttributes != null && profile != null)
            {
                playerAttributes.Initialize(profile);
            }


            if (runtimeData == null)
            {
                runtimeData = new PlayerRuntimeData();
            }
            runtimeData.InitializeState(role, isUserControlled, isAIControlled);

            if (playerVisual != null)
            {
                string pName = profile != null ? profile.name : "Player";
                int number = profile != null ? profile.jerseyNumber : 10;
                playerVisual.SetPlayerIdentity(pName, number, role);

                if (animationController != null)
                {
                    animationController.InitializeLimbs(playerVisual.leftArm, playerVisual.rightArm, playerVisual.leftLeg, playerVisual.rightLeg);
                }
            }

            if (roleController != null)
            {
                roleController.Initialize(role, playerVisual, runtimeData);
            }

            SetUserControl(isUserControlled);
        }

        public void SetUserControl(bool enabled)
        {
            runtimeData.isControlledByUser = enabled;
            runtimeData.isAIControlled = !enabled;

            if (playerMovement != null)
            {
                playerMovement.CanMove = enabled;
            }
        }

        private void Update()
        {
            // Sync runtime position & rotation
            runtimeData.currentPosition = transform.position;
            runtimeData.currentRotation = transform.rotation;

            // Sync animation state with movement
            if (animationController != null && playerMovement != null)
            {
                animationController.UpdateAnimation(
                    playerMovement.CurrentSpeed,
                    playerMovement.IsGrounded,
                    playerMovement.IsRunning,
                    runtimeData.isBatting,
                    runtimeData.isBowling,
                    runtimeData.isFielding
                );
            }
        }
    }
}
