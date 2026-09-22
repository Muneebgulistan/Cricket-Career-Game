using System;
using UnityEngine;
using CricketGame.Players;
using CricketGame.Gameplay.Ball;
using CricketGame.Camera;

namespace CricketGame.Gameplay.Batting
{
    public class BattingController : MonoBehaviour
    {
        [Header("Linked Player Components")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerAttributes playerAttributes;
        [SerializeField] private BatController batController;
        [SerializeField] private BattingInput battingInput;

        [Header("Configuration & State")]
        [SerializeField] private BattingSettings settings = new BattingSettings();
        [SerializeField] private BattingRuntimeData runtimeData = new BattingRuntimeData();

        [Header("Ball Interaction")]
        [SerializeField] private SimpleCricketBall activeBall;
        [SerializeField] private float contactZThreshold = 9.5f; // Striker crease Z position

        private float stateTimer = 0f;
        private BattingShot currentShot = null;

        public BattingState CurrentState { get { return runtimeData.currentState; } }
        public BattingRuntimeData RuntimeData { get { return runtimeData; } }
        public BattingSettings Settings { get { return settings; } }
        public BattingResult LastResult { get { return runtimeData.lastResult; } }
        public SimpleCricketBall ActiveBall { get { return activeBall; } set { activeBall = value; } }

        public event Action<BattingState> OnStateChanged;
        public event Action<BattingResult> OnShotExecuted;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (playerAttributes == null) playerAttributes = GetComponent<PlayerAttributes>();
            if (batController == null) batController = GetComponentInChildren<BatController>();
            if (battingInput == null) battingInput = GetComponent<BattingInput>();
            if (settings == null) settings = BattingSettings.CreateDefault();
            if (runtimeData == null) runtimeData = new BattingRuntimeData();
        }

        private void Start()
        {
            if (batController != null)
            {
                batController.OnBallContact += HandleBallContact;
            }

            SetState(BattingState.Ready);
        }

        private void OnDestroy()
        {
            if (batController != null)
            {
                batController.OnBallContact -= HandleBallContact;
            }
        }

        private void Update()
        {
            // Update input if available
            if (battingInput != null)
            {
                battingInput.PollInput();

                if (battingInput.IsSwingRequested && runtimeData.canSwing)
                {
                    InitiateSwing(battingInput.RequestedShotType);
                    battingInput.ConsumeSwingRequest();
                }
            }

            UpdateStateExecution();
            UpdateBallProximity();
        }

        public void SetState(BattingState newState)
        {
            runtimeData.currentState = newState;
            stateTimer = 0f;

            if (OnStateChanged != null)
            {
                OnStateChanged(newState);
            }
        }

        public void InitiateSwing(BattingShotType shotType)
        {
            if (!runtimeData.canSwing) return;

            runtimeData.currentShotType = shotType;
            currentShot = BattingShot.CreateDefault(shotType);
            runtimeData.isSwinging = true;
            runtimeData.canSwing = false;
            runtimeData.actualSwingTime = Time.time;
            runtimeData.hasMadeContactThisSwing = false;

            SetState(BattingState.Backlift);
        }

        private void UpdateStateExecution()
        {
            stateTimer += Time.deltaTime;

            switch (runtimeData.currentState)
            {
                case BattingState.Ready:
                    if (batController != null)
                    {
                        batController.ApplyProceduralPose(BattingState.Ready, 1f);
                    }
                    break;

                case BattingState.Backlift:
                    float backliftProgress = Mathf.Clamp01(stateTimer / settings.backliftDuration);
                    if (batController != null)
                    {
                        batController.ApplyProceduralPose(BattingState.Backlift, backliftProgress);
                    }

                    if (stateTimer >= settings.backliftDuration)
                    {
                        SetState(BattingState.Swing);
                    }
                    break;

                case BattingState.Swing:
                    float swingProgress = Mathf.Clamp01(stateTimer / settings.swingDuration);
                    if (batController != null)
                    {
                        batController.ApplyProceduralPose(BattingState.Swing, swingProgress);
                    }

                    // Check if ball is in hitting proximity during swing if trigger didn't fire
                    CheckProximityContact();

                    if (stateTimer >= settings.swingDuration)
                    {
                        if (!runtimeData.hasMadeContactThisSwing)
                        {
                            RegisterMiss();
                        }
                        else
                        {
                            SetState(BattingState.FollowThrough);
                        }
                    }
                    break;

                case BattingState.Contact:
                    if (batController != null)
                    {
                        batController.ApplyProceduralPose(BattingState.Contact, 1f);
                    }
                    if (stateTimer >= 0.08f)
                    {
                        SetState(BattingState.FollowThrough);
                    }
                    break;

                case BattingState.FollowThrough:
                    float followProgress = Mathf.Clamp01(stateTimer / settings.followThroughDuration);
                    if (batController != null)
                    {
                        batController.ApplyProceduralPose(BattingState.FollowThrough, followProgress);
                    }

                    if (stateTimer >= settings.followThroughDuration)
                    {
                        SetState(BattingState.Recovery);
                    }
                    break;

                case BattingState.Recovery:
                    float recoveryProgress = Mathf.Clamp01(stateTimer / settings.recoveryDuration);
                    if (batController != null)
                    {
                        batController.ApplyProceduralPose(BattingState.Recovery, recoveryProgress);
                    }

                    if (stateTimer >= settings.recoveryDuration)
                    {
                        runtimeData.ResetForNextDelivery();
                        SetState(BattingState.Ready);
                    }
                    break;

                case BattingState.Miss:
                    if (stateTimer >= settings.recoveryDuration)
                    {
                        runtimeData.ResetForNextDelivery();
                        SetState(BattingState.Ready);
                    }
                    break;
            }
        }

        private void UpdateBallProximity()
        {
            if (activeBall == null || !activeBall.IsInPlay) return;

            // Calculate ideal contact time when ball crosses striker crease Z = 9.5m
            if (activeBall.Velocity.z > 0.1f) // Traveling towards positive Z (batsman)
            {
                float distZ = contactZThreshold - activeBall.Position.z;
                if (distZ > 0f)
                {
                    float timeToArrival = distZ / activeBall.Velocity.z;
                    runtimeData.idealContactTime = Time.time + timeToArrival;
                }
            }
        }

        private void CheckProximityContact()
        {
            if (runtimeData.hasMadeContactThisSwing || activeBall == null || !activeBall.IsInPlay) return;

            // If ball is within bat contact distance (~0.6m) and close to Z crease
            Vector3 sweetSpotPos = batController != null ? batController.SweetSpotPosition : transform.position + transform.forward * 0.5f;
            float dist = Vector3.Distance(activeBall.Position, sweetSpotPos);

            if (dist <= settings.edgeContactRadius + 0.15f)
            {
                ExecuteContact(activeBall, activeBall.Position);
            }
        }

        private void HandleBallContact(ICricketBall ball, Vector3 contactPoint)
        {
            if (runtimeData.hasMadeContactThisSwing) return;
            if (runtimeData.currentState == BattingState.Swing || runtimeData.currentState == BattingState.Backlift)
            {
                ExecuteContact(ball, contactPoint);
            }
        }

        private void ExecuteContact(ICricketBall ball, Vector3 contactPoint)
        {
            runtimeData.hasMadeContactThisSwing = true;
            runtimeData.lastContactPosition = contactPoint;

            if (currentShot == null)
            {
                currentShot = BattingShot.CreateDefault(runtimeData.currentShotType);
            }

            // 1. Evaluate Timing
            float timingDelta = 0f;
            BattingTiming timingSystem = new BattingTiming();
            timingSystem.perfectWindow = settings.perfectTimingWindow;
            timingSystem.goodWindow = settings.goodTimingWindow;
            timingSystem.validWindow = settings.validTimingWindow;

            TimingQuality timingQuality = timingSystem.EvaluateTiming(runtimeData.actualSwingTime, runtimeData.idealContactTime, out timingDelta);

            // 2. Evaluate Contact Quality
            BattingContactPoint contactSystem = new BattingContactPoint();
            contactSystem.sweetSpotRadius = settings.sweetSpotRadius;
            contactSystem.goodContactRadius = settings.goodContactRadius;
            contactSystem.edgeContactRadius = settings.edgeContactRadius;

            Vector3 sweetSpotPos = batController != null ? batController.SweetSpotPosition : transform.position + transform.forward * 0.5f;
            float distFromSweetSpot = 0f;
            ContactQuality contactQuality = contactSystem.EvaluateContact(contactPoint, sweetSpotPos, out distFromSweetSpot);

            // 3. Evaluate Profile Attributes
            PlayerProfile profile = playerAttributes != null ? playerAttributes.Profile : null;

            // 4. Compute Shot Result
            Vector3 batsmanForward = transform.forward;
            float incomingSpeed = ball.SpeedKph;

            BattingResult result = BattingEvaluator.EvaluateShot(
                currentShot,
                timingQuality,
                timingDelta,
                contactQuality,
                distFromSweetSpot,
                profile,
                settings,
                batsmanForward,
                incomingSpeed
            );

            runtimeData.lastResult = result;

            // 5. Apply physics to ball
            ball.ApplyBatContact(result.exitVelocity, result);

            // 6. Camera ball-following
            if (CricketCameraManager.Instance != null && ball is Component)
            {
                CricketCameraManager.Instance.SetFollowTarget(((Component)ball).transform);
            }

            SetState(BattingState.Contact);

            if (OnShotExecuted != null)
            {
                OnShotExecuted(result);
            }
        }

        private void RegisterMiss()
        {
            runtimeData.hasMadeContactThisSwing = true;

            BattingResult missResult = new BattingResult();
            missResult.shotType = runtimeData.currentShotType;
            missResult.timing = TimingQuality.Miss;
            missResult.contactQuality = ContactQuality.Miss;
            missResult.isMiss = true;
            missResult.estimatedRuns = 0;
            missResult.direction = transform.forward;
            runtimeData.lastResult = missResult;

            SetState(BattingState.Miss);

            if (OnShotExecuted != null)
            {
                OnShotExecuted(missResult);
            }
        }

        // Public API for mobile UI / external scripting
        public void PlayShot(BattingShotType shotType, Vector2 direction, bool isLofted)
        {
            if (battingInput != null)
            {
                battingInput.TriggerShot(shotType, direction, isLofted);
            }
            else
            {
                InitiateSwing(shotType);
            }
        }
    }
}
