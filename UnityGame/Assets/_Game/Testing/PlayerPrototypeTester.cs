using UnityEngine;
using CricketGame.Players;

namespace CricketGame.Testing
{
    public class PlayerPrototypeTester : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private PlayerController player;
        [SerializeField] private UnityEngine.Camera testCamera;

        [Header("Camera Tracking")]
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 3.2f, -6.5f);
        [SerializeField] private float cameraSmoothSpeed = 8f;

        private void Start()
        {
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
            }

            if (testCamera == null)
            {
                testCamera = UnityEngine.Camera.main;
            }

            if (player != null)
            {
                PlayerProfile profile = new PlayerProfile("Career Star", 20, "Pakistan", Cricket.PlayingRole.Batsman, Cricket.BattingStyle.RightHand, Cricket.BowlingStyle.RightArmFast);
                profile.jerseyNumber = 56;
                player.Initialize(profile, PlayerMatchRole.Batsman, true, false);
            }
        }

        private void Update()
        {
            HandleRoleSwitchInput();
            UpdateCameraTracking();
        }

        private void HandleRoleSwitchInput()
        {
            if (player == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                player.RoleController.SetRole(PlayerMatchRole.Batsman);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                player.RoleController.SetRole(PlayerMatchRole.Bowler);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                player.RoleController.SetRole(PlayerMatchRole.WicketKeeper);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                player.RoleController.SetRole(PlayerMatchRole.Fielder);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                player.RoleController.SetRole(PlayerMatchRole.Umpire);
            }
        }

        private void UpdateCameraTracking()
        {
            if (testCamera == null || player == null) return;

            Vector3 targetPos = player.transform.position + cameraOffset;
            testCamera.transform.position = Vector3.Lerp(testCamera.transform.position, targetPos, Time.deltaTime * cameraSmoothSpeed);
            testCamera.transform.LookAt(player.transform.position + Vector3.up * 1.2f);
        }

        private void OnGUI()
        {
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;

            GUIStyle bodyStyle = new GUIStyle();
            bodyStyle.fontSize = 13;
            bodyStyle.normal.textColor = Color.yellow;

            GUI.Label(new Rect(20, 20, 400, 25), "=== 3D CRICKET PLAYER PROTOTYPE ===", headerStyle);
            GUI.Label(new Rect(20, 50, 500, 20), "Movement: WASD / Arrow Keys (Hold SHIFT to Sprint)", bodyStyle);
            GUI.Label(new Rect(20, 70, 500, 20), "Roles: [1] Batsman  [2] Bowler  [3] Keeper  [4] Fielder  [5] Umpire", bodyStyle);

            if (player != null)
            {
                GUIStyle statStyle = new GUIStyle();
                statStyle.fontSize = 13;
                statStyle.normal.textColor = Color.white;

                string roleText = string.Format("Current Role: {0} | Speed: {1:F1} m/s | State: {2}",
                    player.RoleController.CurrentRole,
                    player.Movement.CurrentSpeed,
                    player.AnimationController.CurrentState);

                GUI.Label(new Rect(20, 100, 500, 20), roleText, statStyle);
            }
        }
    }
}
