using System;
using UnityEngine;
using CricketGame.Gameplay.Ball;
using CricketGame.Gameplay.Bowling;
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

    [Obsolete("Use BowlingController for delivery generation. TestDeliveryGenerator operates as an adapter.")]
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
        [SerializeField] private BowlingController bowlingController;

        private float timer = 0f;

        public TestDeliveryType SelectedDelivery { get { return selectedDelivery; } set { selectedDelivery = value; } }
        public float DeliverySpeedKph { get { return deliverySpeedKph; } set { deliverySpeedKph = value; } }
        public bool AutoDeliver { get { return autoDeliver; } set { autoDeliver = value; } }

        public event Action<TestDeliveryType, float> OnDeliveryBowled;

        private void Awake()
        {
            if (bowlingController == null)
            {
                bowlingController = FindFirstObjectByType<BowlingController>();
            }

            if (ball == null)
            {
                ball = FindFirstObjectByType<SimpleCricketBall>();
                if (ball == null)
                {
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
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                BowlDelivery(selectedDelivery);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                int next = ((int)selectedDelivery + 1) % 5;
                selectedDelivery = (TestDeliveryType)next;
                Debug.Log(string.Format("[TestDeliveryGenerator] Switched to {0} delivery", selectedDelivery));
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.F1)) selectedDelivery = TestDeliveryType.Straight;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F2)) selectedDelivery = TestDeliveryType.Inswing;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F3)) selectedDelivery = TestDeliveryType.Outswing;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F4)) selectedDelivery = TestDeliveryType.Short;
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) selectedDelivery = TestDeliveryType.Full;
        }

        public void BowlDelivery(TestDeliveryType type)
        {
            if (bowlingController != null)
            {
                // Delegate through Step 5 BowlingController
                BowlingLength length = BowlingLength.GoodLength;
                float swing = 0f;

                switch (type)
                {
                    case TestDeliveryType.Straight: length = BowlingLength.GoodLength; swing = 0f; break;
                    case TestDeliveryType.Inswing: length = BowlingLength.GoodLength; swing = -0.6f; break;
                    case TestDeliveryType.Outswing: length = BowlingLength.GoodLength; swing = 0.6f; break;
                    case TestDeliveryType.Short: length = BowlingLength.Short; swing = 0f; break;
                    case TestDeliveryType.Full: length = BowlingLength.Full; swing = 0f; break;
                }

                BowlingDelivery d = BowlingDelivery.CreateDefault(BowlingBaseType.Fast, length, BowlingLine.OffStump);
                d.releaseSpeedKph = deliverySpeedKph;
                d.swingAmount = swing;

                bowlingController.BowlDirectDelivery(d, 0.90f);

                if (OnDeliveryBowled != null)
                {
                    OnDeliveryBowled(type, deliverySpeedKph);
                }
                return;
            }

            // Fallback direct release if no bowling controller in scene
            if (ball == null) return;

            Vector3 pitchTarget = new Vector3(0.1f, 0.02f, 6.5f);
            float lateralCurve = 0f;
            float speed = deliverySpeedKph;

            switch (type)
            {
                case TestDeliveryType.Straight: pitchTarget = new Vector3(0.1f, 0.02f, 6.8f); lateralCurve = 0f; break;
                case TestDeliveryType.Inswing: pitchTarget = new Vector3(0.35f, 0.02f, 6.8f); lateralCurve = -0.6f; break;
                case TestDeliveryType.Outswing: pitchTarget = new Vector3(-0.15f, 0.02f, 6.8f); lateralCurve = 0.6f; break;
                case TestDeliveryType.Short: pitchTarget = new Vector3(0.1f, 0.02f, 3.2f); speed = deliverySpeedKph * 1.05f; break;
                case TestDeliveryType.Full: pitchTarget = new Vector3(0.0f, 0.02f, 8.8f); speed = deliverySpeedKph * 0.95f; break;
            }

            if (CricketCameraManager.Instance != null)
            {
                CricketCameraManager.Instance.SetFollowTarget(null);
                CricketCameraManager.Instance.SwitchCameraMode(CricketCameraMode.BattingCamera);
            }

            if (batsman != null)
            {
                batsman.ActiveBall = ball;
                batsman.RuntimeData.ResetForNextDelivery();
                batsman.SetState(BattingState.Ready);
            }

            ball.DeliverBall(releasePosition, pitchTarget, speed, lateralCurve);

            if (OnDeliveryBowled != null)
            {
                OnDeliveryBowled(type, speed);
            }
        }
    }
}
