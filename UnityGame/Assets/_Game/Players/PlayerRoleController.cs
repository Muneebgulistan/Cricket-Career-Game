using UnityEngine;

namespace CricketGame.Players
{
    public class PlayerRoleController : MonoBehaviour
    {
        [Header("Current Role")]
        [SerializeField] private PlayerMatchRole currentRole = PlayerMatchRole.Batsman;

        [Header("Dependencies")]
        [SerializeField] private PlayerVisual playerVisual;
        [SerializeField] private PlayerRuntimeData runtimeData;

        public PlayerMatchRole CurrentRole
        {
            get { return currentRole; }
        }

        private void Awake()
        {
            if (playerVisual == null) playerVisual = GetComponent<PlayerVisual>();
        }

        public void Initialize(PlayerMatchRole role, PlayerVisual visual, PlayerRuntimeData runtime)
        {
            playerVisual = visual;
            runtimeData = runtime;
            SetRole(role);
        }

        public void SetRole(PlayerMatchRole role)
        {
            currentRole = role;

            if (runtimeData != null)
            {
                runtimeData.UpdateRoleFlags(role);
            }

            ApplyRoleEquipment(role);
        }

        public void ApplyRoleEquipment(PlayerMatchRole role)
        {
            if (playerVisual == null) return;

            switch (role)
            {
                case PlayerMatchRole.Batsman:
                    playerVisual.SetEquipmentVisibility(true, true, true, true, true);
                    break;
                case PlayerMatchRole.Bowler:
                    playerVisual.SetEquipmentVisibility(false, false, false, false, true);
                    break;
                case PlayerMatchRole.AllRounder:
                    playerVisual.SetEquipmentVisibility(true, true, true, true, true);
                    break;
                case PlayerMatchRole.WicketKeeper:
                    playerVisual.SetEquipmentVisibility(false, true, true, true, true);
                    break;
                case PlayerMatchRole.Fielder:
                    playerVisual.SetEquipmentVisibility(false, false, false, false, true);
                    break;
                case PlayerMatchRole.Umpire:
                    playerVisual.SetEquipmentVisibility(false, false, false, false, true);
                    break;
            }
        }
    }
}
