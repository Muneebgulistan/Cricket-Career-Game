using UnityEngine;

namespace CricketGame.Players
{
    public class PlayerVisual : MonoBehaviour
    {
        [Header("Humanoid Body Hierarchy")]
        public Transform playerBody;
        public Transform head;
        public Transform torso;
        public Transform leftArm;
        public Transform rightArm;
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform feet;

        [Header("Equipment Hierarchy")]
        public GameObject bat;
        public GameObject helmet;
        public GameObject gloves;
        public GameObject pads;
        public GameObject shoes;

        [Header("Visual Identity & Customization")]
        public string displayName = "Player";
        public int jerseyNumber = 10;
        public PlayerMatchRole matchRole = PlayerMatchRole.Batsman;
        public Color primaryColor = new Color(0.1f, 0.5f, 0.2f); // Default team green
        public Color secondaryColor = new Color(0.9f, 0.8f, 0.2f); // Default gold accent

        [Header("Debug World Space Identity")]
        public bool showWorldSpaceLabel = true;

        public void SetEquipmentVisibility(bool showBat, bool showHelmet, bool showGloves, bool showPads, bool showShoes)
        {
            if (bat != null) bat.SetActive(showBat);
            if (helmet != null) helmet.SetActive(showHelmet);
            if (gloves != null) gloves.SetActive(showGloves);
            if (pads != null) pads.SetActive(showPads);
            if (shoes != null) shoes.SetActive(showShoes);
        }

        public void SetPlayerIdentity(string name, int number, PlayerMatchRole role)
        {
            displayName = name;
            jerseyNumber = number;
            matchRole = role;
            gameObject.name = string.Format("Player_{0}_{1}_{2}", number, name.Replace(" ", "_"), role);
        }

        public void SetTeamColors(Color primary, Color secondary)
        {
            primaryColor = primary;
            secondaryColor = secondary;

            // Apply color tinting to torso / shirt if MeshRenderer exists
            if (torso != null)
            {
                MeshRenderer mr = torso.GetComponent<MeshRenderer>();
                if (mr != null && mr.material != null)
                {
                    mr.material.color = primary;
                }
            }
        }

        private void OnGUI()
        {
            if (!showWorldSpaceLabel) return;
            if (UnityEngine.Camera.main == null) return;

            Vector3 worldPos = transform.position + Vector3.up * 2.1f;
            Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(worldPos);

            // Only render if in front of camera
            if (screenPos.z > 0 && screenPos.z < 60f)
            {
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;
                style.fontSize = 12;
                style.fontStyle = FontStyle.Bold;

                string label = string.Format("#{0} {1} ({2})", jerseyNumber, displayName, matchRole);
                GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y - 20, 200, 20), label, style);
            }
        }
    }
}
