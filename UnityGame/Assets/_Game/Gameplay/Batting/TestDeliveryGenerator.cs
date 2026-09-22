using System;
using UnityEngine;
using CricketGame.Gameplay.Ball;
using CricketGame.Camera;

namespace CricketGame.Gameplay.Batting
{
    public enum TestDeliveryType
    {
        Straight,
        Inswing,
        Outswing,
        Short,
        Full
    }

    public class TestDeliveryGenerator : MonoBehaviour
    {
        [Header("Delivery Settings")]
        [SerializeField] private TestDeliveryType selectedDelivery = TestDeliveryType.Straight;
        [SerializeField] private float deliverySpeedKph = 135f;
        [SerializeField] private bool autoDeliver = false;
        [SerializeField] private float autoDeliverInterval = 4.0f;

        [Header("Positions")]
        [SerializeField] private Vector3 releasePosition = new Vector3(0f, 2.1f, -10.5f);
        [SerializeField] private Vector3 strikerStumpPosition = new Vector3(0f, 0f, 10.06f);

        [Header("References")]
        [SerializeField] private SimpleCricketBall ball;
        [SerializeField] private BattingController batsman;

        private float timer = 0f;

        public TestDeliveryType SelectedDelivery { get { return selectedDelivery; } set { selectedDelivery = value; } }
        public float DeliverySpeedKph { get { return deliverySpeedKph; } set { deliverySpeedKph = value; } }
        public bool AutoDeliver { get { return autoDeliver; } set { autoDeliver = value; } }

        public event Action<TestDeliveryType, float> OnDeliveryBowled;

        private void Awake()
        {
            if (ball == null)
            {
                ball = FindFirstObjectByType<SimpleCricketBall>();
                if (ball == null)
                {
                    // Create a simple ball GameObject if none exists
                    GameObject ballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    ballObj.name = "CricketBall";
                    ballObj.transform.localScale = new Vector3(0.072f, 0.072f, 0.072f);
                    ball = ballObj.AddComponent<SimpleCricketBall>();
                }
            }

            if (batsman == null)
            {
                batsman = FindFirstObjectByType<BattingController>();
            }
        }

        private void Start()
        {
            if (batsman != null && ball != null)
            {
                batsman.ActiveBall = ball;
            }
        }

        private void Update()
        {
            HandleKeyboardControls();

            if (autoDeliver)
            {
                timer += Time.deltaTime;
                if (timer >= autoDeliverInterval)
                {
                    timer = 0f;
                    BowlDelivery(selectedDelivery);
                }
            }
        }

        private void HandleKeyboardControls()
        {
            // 'R' bowls the current delivery
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                BowlDelivery(selectedDelivery);
            }

            // 'T' cycles through delivery types
            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                int next = ((int)selectedDelivery + 1) % 5;
                selectedDelivery = (TestDeliveryType)next;
                Debug.Log(string.Format("[TestDeliveryGenerator] Switched to {0} delivery", selectedDelivery));
            }

            // F1 - F5 direct delivery selection
            if (UnityEngine.Input.GetKeyDown(KeyCode.F1)) selectedDelivery = TestDeliveryType.Straight;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F2)) selectedDelivery = TestDeliveryType.Inswing;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F3)) selectedDelivery = TestDeliveryType.Outswing;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F4)) selectedDelivery = TestDeliveryType.Short;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) selectedDelivery = TestDeliveryType.Full;
        }

        public void BowlDelivery(TestDeliveryType type)
        {
            if (ball == null) return;

            Vector3 pitchTarget = new Vector3(0.1f, 0.02f, 6.5f); // Good length default
            float lateralCurve = 0f;
            float speed = deliverySpeedKph;

            switch (type)
            {
                case TestDeliveryType.Straight:
                    pitchTarget = new Vector3(0.1f, 0.02f, 6.8f);
                    lateralCurve = 0f;
                    break;
                case TestDeliveryType.Inswing:
                    pitchTarget = new Vector3(0.35f, 0.02f, 6.8f);
                    lateralCurve = -0.6f; // Curves inward towards right-hander
                    break;
                case TestDeliveryType.Outswing:
                    pitchTarget = new Vector3(-0.15f, 0.02f, 6.8f);
                    lateralCurve = 0.6f; // Curves away from right-hander
                    break;
                case TestDeliveryType.Short:
                    pitchTarget = new Vector3(0.1f, 0.02f, 3.2f); // Pitches halfway down the pitch
                    speed = deliverySpeedKph * 1.05f;
                    break;
                case TestDeliveryType.Full:
                    pitchTarget = new Vector3(0.0f, 0.02f, 8.8f); // Full length yorker/half-volley
                    speed = deliverySpeedKph * 0.95f;
                    break;
            }

            // Reset camera to batting view if currently tracking previous hit
            if (CricketCameraManager.Instance != null)
            {
                CricketCameraManager.Instance.SetFollowTarget(null);
                CricketCameraManager.Instance.SwitchCameraMode(CricketCameraMode.BattingCamera);
            }

            // Reset batsman runtime state for delivery
            if (batsman != null)
            {
                batsman.ActiveBall = ball;
                batsman.RuntimeData.ResetForNextDelivery();
                batsman.SetState(BattingState.Ready);
            }

            // Deliver ball
            ball.DeliverBall(releasePosition, pitchTarget, speed, lateralCurve);

            if (OnDeliveryBowled != null)
            {
                OnDeliveryBowled(type, speed);
            }

            Debug.Log(string.Format("[TestDeliveryGenerator] Bowled {0} delivery at {1:F1} km/h", type, speed));
        }
    }
}
