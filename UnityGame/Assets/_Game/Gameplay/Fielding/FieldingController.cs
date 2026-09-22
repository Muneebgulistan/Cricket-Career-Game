using System;
using UnityEngine;
using CricketGame.Players;
using CricketGame.Gameplay.Ball;

namespace CricketGame.Gameplay.Fielding
{
    public class FieldingController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerAnimationController animationController;
        [SerializeField] private PlayerAttributes playerAttributes;

        [Header("Position & Settings")]
        [SerializeField] private FieldingPosition assignedPosition = FieldingPosition.Cover;
        [SerializeField] private FieldingSettings settings;

        [Header("Runtime State")]
        [SerializeField] private FieldingRuntimeData runtimeData = new FieldingRuntimeData();

        [Header("Ball Reference")]
        [SerializeField] private SimpleCricketBall targetBall;

        public FieldingPosition AssignedPosition { get { return assignedPosition; } set { assignedPosition = value; } }
        public FieldingRuntimeData RuntimeData { get { return runtimeData; } }
        public FieldingState CurrentState { get { return runtimeData.currentState; } }
        public PlayerAttributes Attributes { get { return playerAttributes; } }
        public PlayerController Controller { get { return playerController; } }

        public event Action<FieldingState> OnStateChanged;
        public event Action<FieldingResult> OnFieldingCompleted;

        private Vector3 homePosition;
        private float reactionTimer = 0f;
        private float stateTimer = 0f;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
            if (animationController == null) animationController = GetComponent<PlayerAnimationController>();
            if (playerAttributes == null) playerAttributes = GetComponent<PlayerAttributes>();
            if (settings == null) settings = FieldingSettings.CreateDefault();

            homePosition = transform.position;
        }

        private void Start()
        {
            if (targetBall == null)
            {
                targetBall = FindFirstObjectByType<SimpleCricketBall>();
            }
        }

        public void SetHomePosition(Vector3 pos)
        {
            homePosition = pos;
            transform.position = pos;
        }

        public void SetState(FieldingState newState)
        {
            runtimeData.TransitionState(newState);
            stateTimer = 0f;
            if (OnStateChanged != null)
            {
                OnStateChanged(newState);
            }
        }

        public void AssignInterception(FieldingDecision decision, SimpleCricketBall ball)
        {
            runtimeData.lastDecision = decision;
            runtimeData.interceptionPoint = decision.interceptionPoint;
            targetBall = ball;

            // Calculate reaction delay based on attributes
            int reactionRating = playerAttributes != null ? playerAttributes.reaction : 50;
            reactionTimer = Mathf.Lerp(settings.reactionDelayMax, settings.reactionDelayMin, reactionRating / 100f);

            SetState(FieldingState.Anticipating);
        }

        private void Update()
        {
            stateTimer += Time.deltaTime;
            runtimeData.timeInCurrentState = stateTimer;

            if (targetBall != null)
            {
                runtimeData.distanceToBall = Vector3.Distance(transform.position, targetBall.Position);
            }

            switch (runtimeData.currentState)
            {
                case FieldingState.Idle:
                case FieldingState.Ready:
                    break;

                case FieldingState.Anticipating:
                    HandleAnticipatingState();
                    break;

                case FieldingState.MovingToBall:
                    HandleMovingState();
                    break;

                case FieldingState.ApproachingBall:
                    HandleApproachingState();
                    break;

                case FieldingState.Pickup:
                    HandlePickupState();
                    break;

                case FieldingState.ThrowPreparation:
                    HandleThrowPreparationState();
                    break;

                case FieldingState.Throwing:
                    HandleThrowingState();
                    break;

                case FieldingState.FollowThrough:
                    HandleFollowThroughState();
                    break;

                case FieldingState.Returning:
                    HandleReturningState();
                    break;

                case FieldingState.Completed:
                    break;
            }
        }

        private void HandleAnticipatingState()
        {
            reactionTimer -= Time.deltaTime;
            if (reactionTimer <= 0f)
            {
                SetState(FieldingState.MovingToBall);
            }
        }

        private void HandleMovingState()
        {
            Vector3 targetPos = runtimeData.interceptionPoint;
            // Also track dynamic ball position if ball has rolled past original point
            if (targetBall != null && targetBall.IsInPlay)
            {
                float distToTarget = Vector3.Distance(transform.position, targetPos);
                if (distToTarget < 3.0f)
                {
                    targetPos = targetBall.Position;
                }
            }

            Vector3 moveDir = targetPos - transform.position;
            moveDir.y = 0f;
            float distance = moveDir.magnitude;

            if (distance < settings.pickupRadius * 1.8f)
            {
                SetState(FieldingState.ApproachingBall);
                return;
            }

            if (playerMovement != null)
            {
                Vector2 moveInput = new Vector2(moveDir.x, moveDir.z).normalized;
                playerMovement.SetVirtualInput(moveInput, true); // Sprint to ball
            }
            else
            {
                transform.position += moveDir.normalized * (settings.baseSprintSpeed * Time.deltaTime);
                if (moveDir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(moveDir);
                }
            }
        }

        private void HandleApproachingState()
        {
            Vector3 ballPos = targetBall != null ? targetBall.Position : runtimeData.interceptionPoint;
            Vector3 moveDir = ballPos - transform.position;
            moveDir.y = 0f;
            float distance = moveDir.magnitude;

            if (distance <= settings.pickupRadius || stateTimer > 1.5f)
            {
                // Stop movement
                if (playerMovement != null)
                {
                    playerMovement.SetVirtualInput(Vector2.zero, false);
                }

                SetState(FieldingState.Pickup);
                return;
            }

            // Decelerate as approaching ball
            if (playerMovement != null)
            {
                Vector2 moveInput = new Vector2(moveDir.x, moveDir.z).normalized;
                playerMovement.SetVirtualInput(moveInput, false); // Jog/walk into pickup
            }
        }

        private void HandlePickupState()
        {
            if (stateTimer < 0.25f) return; // Small collection animation window

            bool isCatch = runtimeData.lastDecision != null && runtimeData.lastDecision.isCatchOpportunity && targetBall != null && targetBall.Position.y > 0.4f;

            if (isCatch)
            {
                float difficulty = runtimeData.lastDecision.catchDifficulty;
                FieldingOutcome catchOutcome = FieldingEvaluator.EvaluateCatchSuccess(playerAttributes, difficulty);

                if (catchOutcome == FieldingOutcome.Catch)
                {
                    if (targetBall != null) targetBall.PickUpBall(transform);
                    runtimeData.hasBall = true;
                    FieldingResult res = FieldingResult.CreateCatch(GetFielderName(), assignedPosition, true);
                    CompleteAction(res);
                    SetState(FieldingState.Completed);
                    return;
                }
                else
                {
                    // Dropped catch! Ball bounces and rolls
                    FieldingResult res = FieldingResult.CreateCatch(GetFielderName(), assignedPosition, false);
                    runtimeData.hasBall = false;
                    CompleteAction(res);
                    SetState(FieldingState.Returning);
                    return;
                }
            }
            else
            {
                // Ground pickup
                float ballSpeed = targetBall != null ? targetBall.SpeedKph : 30f;
                FieldingOutcome pickupOutcome = FieldingEvaluator.EvaluateGroundPickup(playerAttributes, ballSpeed, 0f);

                if (pickupOutcome == FieldingOutcome.CleanPickup)
                {
                    if (targetBall != null) targetBall.PickUpBall(transform);
                    runtimeData.hasBall = true;
                    SetState(FieldingState.ThrowPreparation);
                }
                else
                {
                    // Fumble: extra delay before recovery
                    runtimeData.fumbleCount++;
                    if (stateTimer > 0.65f)
                    {
                        if (targetBall != null) targetBall.PickUpBall(transform);
                        runtimeData.hasBall = true;
                        SetState(FieldingState.ThrowPreparation);
                    }
                }
            }
        }

        private void HandleThrowPreparationState()
        {
            // Choose throw target: defaults to WicketKeeper or Bowler depending on distance
            Vector3 keeperPos = FieldingTargetUtility.GetTargetWorldPosition(FieldingTarget.WicketKeeper);
            Vector3 bowlerPos = FieldingTargetUtility.GetTargetWorldPosition(FieldingTarget.Bowler);

            float distToKeeper = Vector3.Distance(transform.position, keeperPos);
            float distToBowler = Vector3.Distance(transform.position, bowlerPos);

            FieldingTarget target = distToKeeper <= distToBowler ? FieldingTarget.WicketKeeper : FieldingTarget.Bowler;
            runtimeData.selectedThrowTarget = target;

            // Turn toward target
            Vector3 targetPos = FieldingTargetUtility.GetTargetWorldPosition(target);
            Vector3 toTarget = targetPos - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(toTarget);
            }

            if (stateTimer >= 0.35f)
            {
                SetState(FieldingState.Throwing);
            }
        }

        private void HandleThrowingState()
        {
            Vector3 targetPos = FieldingTargetUtility.GetTargetWorldPosition(runtimeData.selectedThrowTarget);
            Vector3 throwOrigin = transform.position + new Vector3(0f, 1.6f, 0f);

            FieldingThrowData throwData = FieldingEvaluator.CalculateThrow(
                throwOrigin,
                runtimeData.selectedThrowTarget,
                targetPos,
                playerAttributes,
                settings
            );

            // Launch throw through existing ball system
            if (targetBall != null)
            {
                targetBall.LaunchThrow(throwOrigin, throwData.velocity);
            }
            runtimeData.hasBall = false;

            // Run-out evaluation
            float batsmanDistToCrease = 4.5f; // Estimated mid-pitch run
            bool isRunOut = FieldingEvaluator.EvaluateRunOut(throwData, batsmanDistToCrease, settings.batsmanRunSpeed);

            FieldingResult res = isRunOut 
                ? FieldingResult.CreateRunOut(GetFielderName(), assignedPosition, true, 1)
                : FieldingResult.CreateGroundField(GetFielderName(), assignedPosition, runtimeData.fumbleCount == 0, 1);
            res.throwData = throwData;

            CompleteAction(res);
            SetState(FieldingState.FollowThrough);
        }

        private void HandleFollowThroughState()
        {
            if (stateTimer >= 0.5f)
            {
                SetState(FieldingState.Returning);
            }
        }

        private void HandleReturningState()
        {
            Vector3 toHome = homePosition - transform.position;
            toHome.y = 0f;

            if (toHome.magnitude <= 1.0f || stateTimer >= 3.0f)
            {
                if (playerMovement != null) playerMovement.SetVirtualInput(Vector2.zero, false);
                transform.position = homePosition;
                SetState(FieldingState.Completed);
                return;
            }

            if (playerMovement != null)
            {
                Vector2 moveInput = new Vector2(toHome.x, toHome.z).normalized;
                playerMovement.SetVirtualInput(moveInput, false); // Jog home
            }
            else
            {
                transform.position += toHome.normalized * (settings.baseWalkSpeed * Time.deltaTime);
            }
        }

        private void CompleteAction(FieldingResult res)
        {
            runtimeData.lastResult = res;
            if (OnFieldingCompleted != null)
            {
                OnFieldingCompleted(res);
            }
        }

        public string GetFielderName()
        {
            return playerAttributes != null ? playerAttributes.playerName : assignedPosition.ToString();
        }

        public void ResetToHome()
        {
            transform.position = homePosition;
            if (playerMovement != null) playerMovement.SetVirtualInput(Vector2.zero, false);
            runtimeData.Reset();
            SetState(FieldingState.Idle);
        }
    }
}
