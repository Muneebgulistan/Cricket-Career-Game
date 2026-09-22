using System;
using UnityEngine;
using CricketGame.Players;
using CricketGame.Gameplay.Ball;
using CricketGame.Gameplay.Batting;
using CricketGame.Camera;

namespace CricketGame.Gameplay.Bowling
{
    public class BowlingController : MonoBehaviour
    {
        [Header("Linked Player Components")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerAttributes playerAttributes;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerAnimationController playerAnimation;
        [SerializeField] private BowlingInput bowlingInput;

        [Header("Configuration & State")]
        [SerializeField] private BowlingSettings settings = new BowlingSettings();
        [SerializeField] private BowlingRuntimeData runtimeData = new BowlingRuntimeData();

        [Header("Stadium Positions")]
        [SerializeField] private Vector3 bowlerStartPosition = new Vector3(0f, 0f, -22.0f);
        [SerializeField] private Vector3 bowlerReleasePosition = new Vector3(0f, 2.1f, -10.5f);

        [Header("Ball & Batsman References")]
        [SerializeField] private SimpleCricketBall activeBall;
        [SerializeField] private BattingController activeBatsman;

        private float stateTimer = 0f;

        public BowlingState CurrentState { get { return runtimeData.currentState; } }
        public BowlingRuntimeData RuntimeData { get { return runtimeData; } }
        public BowlingSettings Settings { get { return settings; } }
        public BowlingResult LastResult { get { return runtimeData.lastResult; } }
        public SimpleCricketBall ActiveBall { get { return activeBall; } set { activeBall = value; } }
        public BattingController ActiveBatsman { get { return activeBatsman; } set { activeBatsman = value; } }

        public event Action<BowlingState> OnStateChanged;
        public event Action<BowlingReleaseData> OnDeliveryReleased;
        public event Action<BowlingResult> OnDeliveryCompleted;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (playerAttributes == null) playerAttributes = GetComponent<PlayerAttributes>();
            if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
            if (playerAnimation == null) playerAnimation = GetComponent<PlayerAnimationController>();
            if (bowlingInput == null) bowlingInput = GetComponent<BowlingInput>();
            if (settings == null) settings = BowlingSettings.CreateDefault();
            if (runtimeData == null) runtimeData = new BowlingRuntimeData();
        }

        private void Start()
        {
            if (activeBall == null) activeBall = FindFirstObjectByType<SimpleCricketBall>();
            if (activeBatsman == null) activeBatsman = FindFirstObjectByType<BattingController>();

            if (activeBall != null)
            {
                activeBall.OnPitchBounce += HandlePitchBounce;
                activeBall.OnBatContact += HandleBatContact;
            }

            ResetBowlerPosition();
            SetState(BowlingState.Idle);
        }

        private void OnDestroy()
        {
            if (activeBall != null)
            {
                activeBall.OnPitchBounce -= HandlePitchBounce;
                activeBall.OnBatContact -= HandleBatContact;
            }
        }

        private void Update()
        {
            if (bowlingInput != null)
            {
                bowlingInput.PollInput();

                if (bowlingInput.IsDeliveryRequested && runtimeData.canBowl)
                {
                    StartDeliverySequence();
                    bowlingInput.ConsumeDeliveryRequest();
                }
            }

            UpdateStateExecution();
        }

        public void SetState(BowlingState newState)
        {
            runtimeData.currentState = newState;
            stateTimer = 0f;

            if (OnStateChanged != null)
            {
                OnStateChanged(newState);
            }
        }

        public void StartDeliverySequence()
        {
            if (!runtimeData.canBowl) return;

            runtimeData.canBowl = false;
            runtimeData.isBowling = true;

            // Configure delivery from input
            if (bowlingInput != null)
            {
                runtimeData.activeDelivery = BowlingDelivery.CreateDefault(
                    bowlingInput.SelectedType,
                    bowlingInput.SelectedLength,
                    bowlingInput.SelectedLine
                );
                if (Mathf.Abs(bowlingInput.SwingAmount) > 0.01f)
                {
                    runtimeData.activeDelivery.swingAmount = bowlingInput.SwingAmount;
                }
                if (Mathf.Abs(bowlingInput.SpinAmount) > 0.01f)
                {
                    runtimeData.activeDelivery.spinAmount = bowlingInput.SpinAmount;
                }
                runtimeData.accuracyMeterValue = bowlingInput.AccuracyMeter;
            }

            // Switch camera to Bowling view
            if (CricketCameraManager.Instance != null)
            {
                CricketCameraManager.Instance.SetFollowTarget(null);
                CricketCameraManager.Instance.SwitchCameraMode(CricketCameraMode.BowlingCamera);
            }

            // Reset batsman for delivery
            if (activeBatsman != null)
            {
                activeBatsman.ActiveBall = activeBall;
                activeBatsman.RuntimeData.ResetForNextDelivery();
                activeBatsman.SetState(BattingState.Ready);
            }

            SetState(BowlingState.RunUp);
        }

        private void UpdateStateExecution()
        {
            stateTimer += Time.deltaTime;

            switch (runtimeData.currentState)
            {
                case BowlingState.Idle:
                    break;

                case BowlingState.RunUp:
                    float runProgress = Mathf.Clamp01(stateTimer / settings.runUpDuration);
                    Vector3 runPos = Vector3.Lerp(bowlerStartPosition, bowlerReleasePosition - new Vector3(0f, 2.1f, 1.2f), runProgress);
                    transform.position = runPos;

                    if (playerAnimation != null)
                    {
                        playerAnimation.UpdateAnimation(5.5f, true, true, false, true, false);
                    }

                    if (stateTimer >= settings.runUpDuration)
                    {
                        SetState(BowlingState.DeliveryStride);
                    }
                    break;

                case BowlingState.DeliveryStride:
                    float strideProgress = Mathf.Clamp01(stateTimer / settings.deliveryStrideDuration);
                    Vector3 stridePos = Vector3.Lerp(bowlerReleasePosition - new Vector3(0f, 2.1f, 1.2f), bowlerReleasePosition - new Vector3(0f, 2.1f, 0f), strideProgress);
                    transform.position = stridePos;

                    if (stateTimer >= settings.deliveryStrideDuration)
                    {
                        ExecuteBallRelease();
                        SetState(BowlingState.Release);
                    }
                    break;

                case BowlingState.Release:
                    if (stateTimer >= 0.15f)
                    {
                        SetState(BowlingState.Recovery);
                    }
                    break;

                case BowlingState.Recovery:
                    float recoveryProgress = Mathf.Clamp01(stateTimer / settings.recoveryDuration);

                    if (stateTimer >= settings.recoveryDuration)
                    {
                        CompleteDelivery();
                    }
                    break;
            }
        }

        public void ExecuteBallRelease()
        {
            if (activeBall == null) return;

            PlayerProfile profile = playerAttributes != null ? playerAttributes.Profile : null;
            BowlingReleaseData releaseData = BowlingEvaluator.CalculateReleaseData(
                runtimeData.activeDelivery,
                bowlerReleasePosition,
                profile,
                settings,
                runtimeData.accuracyMeterValue
            );

            runtimeData.lastReleaseData = releaseData;

            // Launch ball
            activeBall.LaunchDelivery(releaseData);

            // Camera tracks ball
            if (CricketCameraManager.Instance != null)
            {
                CricketCameraManager.Instance.SetFollowTarget(activeBall.transform);
            }

            if (OnDeliveryReleased != null)
            {
                OnDeliveryReleased(releaseData);
            }

            Debug.Log(string.Format("[BowlingController] Released {0} at {1:F1} km/h", releaseData.deliveryName, releaseData.speedKph));
        }

        private void HandlePitchBounce(Vector3 bouncePos)
        {
            if (runtimeData.isBowling && runtimeData.lastReleaseData != null)
            {
                BowlingResult result = BowlingEvaluator.EvaluateResult(
                    runtimeData.activeDelivery,
                    runtimeData.lastReleaseData,
                    bouncePos,
                    false,
                    true
                );
                runtimeData.lastResult = result;
            }
        }

        private void HandleBatContact(BattingResult battingResult)
        {
            if (runtimeData.lastResult != null)
            {
                runtimeData.lastResult.wasHitByBatsman = true;
            }
        }

        private void CompleteDelivery()
        {
            if (runtimeData.lastResult == null && runtimeData.lastReleaseData != null)
            {
                runtimeData.lastResult = BowlingEvaluator.EvaluateResult(
                    runtimeData.activeDelivery,
                    runtimeData.lastReleaseData,
                    Vector3.zero,
                    false,
                    false
                );
            }

            if (OnDeliveryCompleted != null && runtimeData.lastResult != null)
            {
                OnDeliveryCompleted(runtimeData.lastResult);
            }

            ResetBowlerPosition();
            runtimeData.ResetForNextDelivery();
            SetState(BowlingState.Idle);
        }

        public void ResetBowlerPosition()
        {
            transform.position = bowlerStartPosition;
            transform.rotation = Quaternion.identity; // Facing down pitch toward batsman (positive Z)
            if (playerAnimation != null)
            {
                playerAnimation.UpdateAnimation(0f, true, false, false, false, false);
            }
        }

        // Direct method for testing or AI bowling
        public void BowlDirectDelivery(BowlingDelivery delivery, float accuracy = 0.85f)
        {
            runtimeData.activeDelivery = delivery;
            runtimeData.accuracyMeterValue = accuracy;
            StartDeliverySequence();
        }
    }
}
