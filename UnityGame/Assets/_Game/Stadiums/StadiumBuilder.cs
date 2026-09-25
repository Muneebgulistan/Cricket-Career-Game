using UnityEngine;

namespace CricketGame.Stadiums
{
    /// <summary>
    /// Procedurally builds a cricket stadium from Unity primitives at runtime.
    /// Creates: ground, pitch strip, crease lines, 6 stumps + 2 bails per end,
    /// boundary rope, 4 simplified stands, atmospheric sky sphere.
    /// 
    /// All geometry uses primitive meshes — no external asset dependency.
    /// Mobile-optimised: static batching friendly, low triangle counts.
    /// </summary>
    public class StadiumBuilder : MonoBehaviour
    {
        [Header("Build Settings")]
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool builtAlready = false;

        [Header("Ground")]
        [SerializeField] private float groundRadius = 70f;
        [SerializeField] private Color outfieldColor = new Color(0.18f, 0.52f, 0.14f);
        [SerializeField] private Color pitchColor = new Color(0.85f, 0.78f, 0.55f);

        [Header("Pitch")]
        [SerializeField] private float pitchLength = 20.12f;
        [SerializeField] private float pitchWidth = 3.05f;

        [Header("Stumps")]
        [SerializeField] private float stumpHeight = 0.71f;
        [SerializeField] private float stumpRadius = 0.02f;
        [SerializeField] private Color stumpColor = new Color(0.85f, 0.75f, 0.5f);

        [Header("Stands")]
        [SerializeField] private Color standColor = new Color(0.3f, 0.3f, 0.35f);
        [SerializeField] private Color seatColor = new Color(0.15f, 0.35f, 0.7f);

        [Header("Boundary")]
        [SerializeField] private Color boundaryColor = new Color(0.9f, 0.9f, 0.9f);

        [Header("Sky")]
        [SerializeField] private Color skyColorTop = new Color(0.25f, 0.55f, 0.88f);

        // Container hierarchy
        private Transform stadiumRoot;

        // --------------------------------------------------
        // Lifecycle
        // --------------------------------------------------

        private void Start()
        {
            if (buildOnStart && !builtAlready)
            {
                BuildStadium();
            }
        }

        // --------------------------------------------------
        // Main Build
        // --------------------------------------------------

        public void BuildStadium()
        {
            if (builtAlready) return;
            builtAlready = true;

            stadiumRoot = new GameObject("[Stadium]").transform;
            stadiumRoot.SetParent(transform);

            BuildGround();
            BuildPitch();
            BuildStumps();
            BuildBoundaryRope();
            BuildStands();
            BuildSkyDome();
            BuildFloodlightTowers();

            Debug.Log("[StadiumBuilder] Stadium built successfully.");
        }

        // --------------------------------------------------
        // Ground
        // --------------------------------------------------

        private void BuildGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Ground_Outfield";
            ground.transform.SetParent(stadiumRoot);
            ground.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(groundRadius * 2f, 0.05f, groundRadius * 2f);
            SetMaterialColor(ground, outfieldColor);

            // Remove collider — stadium geometry is visual only
            Destroy(ground.GetComponent<CapsuleCollider>());
        }

        // --------------------------------------------------
        // Pitch
        // --------------------------------------------------

        private void BuildPitch()
        {
            GameObject pitch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pitch.name = "Pitch_Strip";
            pitch.transform.SetParent(stadiumRoot);
            pitch.transform.localPosition = new Vector3(0f, 0.001f, 0f);
            pitch.transform.localScale = new Vector3(pitchWidth, 0.01f, pitchLength);
            SetMaterialColor(pitch, pitchColor);
            Destroy(pitch.GetComponent<BoxCollider>());

            // Crease lines: popping creases at ±8.84m, bowling creases at ±10.06m
            BuildCrease("Crease_BatterEnd_Popping", new Vector3(0f, 0.002f, 8.84f), new Vector3(pitchWidth + 0.5f, 0.005f, 0.04f));
            BuildCrease("Crease_BatterEnd_Bowling", new Vector3(0f, 0.002f, 10.06f), new Vector3(pitchWidth + 0.5f, 0.005f, 0.03f));
            BuildCrease("Crease_BowlerEnd_Popping", new Vector3(0f, 0.002f, -8.84f), new Vector3(pitchWidth + 0.5f, 0.005f, 0.04f));
            BuildCrease("Crease_BowlerEnd_Bowling", new Vector3(0f, 0.002f, -10.06f), new Vector3(pitchWidth + 0.5f, 0.005f, 0.03f));
        }

        private void BuildCrease(string name, Vector3 position, Vector3 scale)
        {
            GameObject crease = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crease.name = name;
            crease.transform.SetParent(stadiumRoot);
            crease.transform.localPosition = position;
            crease.transform.localScale = scale;
            SetMaterialColor(crease, Color.white);
            Destroy(crease.GetComponent<BoxCollider>());
        }

        // --------------------------------------------------
        // Stumps & Bails
        // --------------------------------------------------

        private void BuildStumps()
        {
            // Batter end stumps at z = +10.06m
            BuildStumpSet("BatterEnd", 10.06f);
            // Bowler end stumps at z = -10.06m
            BuildStumpSet("BowlerEnd", -10.06f);
        }

        private void BuildStumpSet(string label, float z)
        {
            // 3 stumps spaced 0.11m apart
            float[] xPositions = new float[] { -0.11f, 0f, 0.11f };
            string[] stumpNames = new string[] { "LegStump", "MiddleStump", "OffStump" };

            for (int i = 0; i < 3; i++)
            {
                GameObject stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stump.name = string.Format("Stump_{0}_{1}", label, stumpNames[i]);
                stump.transform.SetParent(stadiumRoot);
                stump.transform.localPosition = new Vector3(xPositions[i], stumpHeight * 0.5f, z);
                stump.transform.localScale = new Vector3(stumpRadius * 2f, stumpHeight * 0.5f, stumpRadius * 2f);
                SetMaterialColor(stump, stumpColor);
                Destroy(stump.GetComponent<CapsuleCollider>());
            }

            // Bails: 2 small cubes bridging the stumps
            BuildBail(string.Format("Bail_{0}_Left", label), new Vector3(-0.055f, stumpHeight + 0.01f, z));
            BuildBail(string.Format("Bail_{0}_Right", label), new Vector3(0.055f, stumpHeight + 0.01f, z));
        }

        private void BuildBail(string name, Vector3 position)
        {
            GameObject bail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bail.name = name;
            bail.transform.SetParent(stadiumRoot);
            bail.transform.localPosition = position;
            bail.transform.localScale = new Vector3(0.11f, 0.025f, 0.025f);
            SetMaterialColor(bail, stumpColor);
            Destroy(bail.GetComponent<BoxCollider>());
        }

        // --------------------------------------------------
        // Boundary Rope
        // --------------------------------------------------

        private void BuildBoundaryRope()
        {
            // Approximate boundary rope with 32 thin segments arranged in a circle
            int segments = 32;
            float radius = groundRadius - 1f;
            float segAngle = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * segAngle * Mathf.Deg2Rad;
                float angle2 = (i + 1) * segAngle * Mathf.Deg2Rad;

                Vector3 p1 = new Vector3(Mathf.Sin(angle1) * radius, 0.03f, Mathf.Cos(angle1) * radius);
                Vector3 p2 = new Vector3(Mathf.Sin(angle2) * radius, 0.03f, Mathf.Cos(angle2) * radius);
                Vector3 mid = (p1 + p2) * 0.5f;
                float segLength = Vector3.Distance(p1, p2);
                Vector3 dir = (p2 - p1).normalized;

                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = string.Format("BoundaryRope_{0}", i);
                seg.transform.SetParent(stadiumRoot);
                seg.transform.position = mid;
                seg.transform.rotation = Quaternion.LookRotation(dir);
                seg.transform.localScale = new Vector3(0.08f, 0.08f, segLength);
                SetMaterialColor(seg, boundaryColor);
                Destroy(seg.GetComponent<BoxCollider>());
            }
        }

        // --------------------------------------------------
        // Stands
        // --------------------------------------------------

        private void BuildStands()
        {
            // 4 simple tiered stand blocks at cardinal positions
            float standDist = groundRadius + 5f;

            BuildSingleStand("Stand_North", new Vector3(0f, 2.5f, standDist),
                             new Vector3(90f, 0f, 0f), new Vector3(60f, 5f, 10f));
            BuildSingleStand("Stand_South", new Vector3(0f, 2.5f, -standDist),
                             new Vector3(-90f, 0f, 0f), new Vector3(60f, 5f, 10f));
            BuildSingleStand("Stand_East", new Vector3(standDist, 2.5f, 0f),
                             new Vector3(0f, -90f, 0f), new Vector3(60f, 5f, 10f));
            BuildSingleStand("Stand_West", new Vector3(-standDist, 2.5f, 0f),
                             new Vector3(0f, 90f, 0f), new Vector3(60f, 5f, 10f));
        }

        private void BuildSingleStand(string name, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            // Base concrete structure
            GameObject standBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            standBase.name = name + "_Base";
            standBase.transform.SetParent(stadiumRoot);
            standBase.transform.localPosition = position;
            standBase.transform.localEulerAngles = eulerAngles;
            standBase.transform.localScale = scale;
            SetMaterialColor(standBase, standColor);
            Destroy(standBase.GetComponent<BoxCollider>());

            // Seat rows overlay
            GameObject seats = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seats.name = name + "_Seats";
            seats.transform.SetParent(stadiumRoot);
            seats.transform.localPosition = position + Vector3.up * 0.2f;
            seats.transform.localEulerAngles = eulerAngles;
            seats.transform.localScale = new Vector3(scale.x * 0.95f, 0.3f, scale.z * 0.9f);
            SetMaterialColor(seats, seatColor);
            Destroy(seats.GetComponent<BoxCollider>());
        }

        // --------------------------------------------------
        // Sky Dome
        // --------------------------------------------------

        private void BuildSkyDome()
        {
            // Inverted large sphere as simple sky background
            GameObject sky = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sky.name = "SkyDome";
            sky.transform.SetParent(stadiumRoot);
            sky.transform.localPosition = new Vector3(0f, 0f, 0f);
            sky.transform.localScale = new Vector3(400f, 200f, 400f);

            MeshRenderer mr = sky.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Color"));
                if (mat != null)
                {
                    mat.color = skyColorTop;
                    mat.SetFloat("_Cull", 1f); // Front culling to render inside
                }
                mr.material = mat;
            }

            Destroy(sky.GetComponent<SphereCollider>());
        }

        // --------------------------------------------------
        // Floodlight Towers
        // --------------------------------------------------

        private void BuildFloodlightTowers()
        {
            BuildTower("Tower_NE", new Vector3(60f, 0f, 60f));
            BuildTower("Tower_NW", new Vector3(-60f, 0f, 60f));
            BuildTower("Tower_SE", new Vector3(60f, 0f, -60f));
            BuildTower("Tower_SW", new Vector3(-60f, 0f, -60f));
        }

        private void BuildTower(string name, Vector3 basePos)
        {
            // Pole
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = name + "_Pole";
            pole.transform.SetParent(stadiumRoot);
            pole.transform.localPosition = basePos + Vector3.up * 12.5f;
            pole.transform.localScale = new Vector3(0.6f, 12.5f, 0.6f);
            SetMaterialColor(pole, new Color(0.55f, 0.55f, 0.55f));
            Destroy(pole.GetComponent<CapsuleCollider>());

            // Light housing
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = name + "_Head";
            head.transform.SetParent(stadiumRoot);
            head.transform.localPosition = basePos + Vector3.up * 25.5f;
            head.transform.localScale = new Vector3(4f, 0.5f, 3f);
            SetMaterialColor(head, new Color(0.85f, 0.85f, 0.7f));
            Destroy(head.GetComponent<BoxCollider>());
        }

        // --------------------------------------------------
        // Helpers
        // --------------------------------------------------

        private void SetMaterialColor(GameObject go, Color color)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend == null) return;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
            }

            if (mat != null)
            {
                mat.color = color;
                rend.material = mat;
            }
        }
    }
}
