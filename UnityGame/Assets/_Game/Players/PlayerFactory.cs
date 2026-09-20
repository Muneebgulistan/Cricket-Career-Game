using UnityEngine;

namespace CricketGame.Players
{
    public class PlayerFactory : MonoBehaviour
    {
        public static PlayerFactory Instance { get; private set; }

        [Header("Prefab Reference")]
        [SerializeField] private GameObject playerPrefab;

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

        public PlayerController CreatePlayer(PlayerProfile profile, Vector3 position, Quaternion rotation, PlayerMatchRole role, bool isUserControlled, bool isAI = true)
        {
            GameObject playerObj = null;

            if (playerPrefab != null)
            {
                playerObj = Instantiate(playerPrefab, position, rotation);
            }
            else
            {
                // Fallback procedural instantiation if prefab reference not linked in scene
                playerObj = CreateProceduralPlayerObject(position, rotation);
            }

            PlayerController controller = playerObj.GetComponent<PlayerController>();
            if (controller == null)
            {
                controller = playerObj.AddComponent<PlayerController>();
            }

            controller.Initialize(profile, role, isUserControlled, isAI);
            return controller;
        }

        public static GameObject CreateProceduralPlayerObject(Vector3 position, Quaternion rotation)
        {
            GameObject root = new GameObject("CricketPlayer");
            root.transform.position = position;
            root.transform.rotation = rotation;

            CharacterController cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            PlayerAttributes attributes = root.AddComponent<PlayerAttributes>();
            PlayerVisual visual = root.AddComponent<PlayerVisual>();
            PlayerRoleController roleController = root.AddComponent<PlayerRoleController>();
            PlayerMovement movement = root.AddComponent<PlayerMovement>();
            PlayerAnimationController anim = root.AddComponent<PlayerAnimationController>();
            PlayerController controller = root.AddComponent<PlayerController>();

            // Humanoid Body
            GameObject body = new GameObject("PlayerBody");
            body.transform.SetParent(root.transform, false);
            visual.playerBody = body.transform;

            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(body.transform, false);
            torso.transform.localPosition = new Vector3(0, 1.15f, 0);
            torso.transform.localScale = new Vector3(0.45f, 0.6f, 0.25f);
            visual.torso = torso.transform;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(body.transform, false);
            head.transform.localPosition = new Vector3(0, 1.62f, 0);
            head.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
            visual.head = head.transform;

            GameObject lArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lArm.name = "LeftArm";
            lArm.transform.SetParent(body.transform, false);
            lArm.transform.localPosition = new Vector3(-0.32f, 1.15f, 0);
            lArm.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
            visual.leftArm = lArm.transform;

            GameObject rArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rArm.name = "RightArm";
            rArm.transform.SetParent(body.transform, false);
            rArm.transform.localPosition = new Vector3(0.32f, 1.15f, 0);
            rArm.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
            visual.rightArm = rArm.transform;

            GameObject lLeg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lLeg.name = "LeftLeg";
            lLeg.transform.SetParent(body.transform, false);
            lLeg.transform.localPosition = new Vector3(-0.14f, 0.45f, 0);
            lLeg.transform.localScale = new Vector3(0.14f, 0.45f, 0.14f);
            visual.leftLeg = lLeg.transform;

            GameObject rLeg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rLeg.name = "RightLeg";
            rLeg.transform.SetParent(body.transform, false);
            rLeg.transform.localPosition = new Vector3(0.14f, 0.45f, 0);
            rLeg.transform.localScale = new Vector3(0.14f, 0.45f, 0.14f);
            visual.rightLeg = rLeg.transform;

            // Equipment Container
            GameObject equip = new GameObject("Equipment");
            equip.transform.SetParent(root.transform, false);

            GameObject bat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bat.name = "Bat";
            bat.transform.SetParent(equip.transform, false);
            bat.transform.localPosition = new Vector3(0.38f, 0.65f, 0.1f);
            bat.transform.localScale = new Vector3(0.12f, 0.85f, 0.05f);
            visual.bat = bat;

            GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            helmet.name = "Helmet";
            helmet.transform.SetParent(equip.transform, false);
            helmet.transform.localPosition = new Vector3(0, 1.65f, 0);
            helmet.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
            visual.helmet = helmet;

            GameObject gloves = new GameObject("Gloves");
            gloves.transform.SetParent(equip.transform, false);
            visual.gloves = gloves;

            GameObject pads = new GameObject("Pads");
            pads.transform.SetParent(equip.transform, false);
            visual.pads = pads;

            GameObject shoes = new GameObject("Shoes");
            shoes.transform.SetParent(equip.transform, false);
            visual.shoes = shoes;

            anim.InitializeLimbs(visual.leftArm, visual.rightArm, visual.leftLeg, visual.rightLeg);
            return root;
        }
    }
}
