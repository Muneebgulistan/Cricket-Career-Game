using UnityEngine;
using CricketGame.Animation;
using CricketGame.Visuals;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;

namespace CricketGame.Players
{
    public enum PlayerAnimationState
    {
        Idle,
        Walk,
        Run,
        Sprint,
        Crouch,
        Jump,
        Celebrate,
        Batting,
        Bowling,
        Fielding,
        WicketKeeping
    }

    /// <summary>
    /// Master animation controller that acts as the bridge between gameplay logic
    /// controllers and specialized Animator/IK subsystems (Batting, Bowling, Fielding, Visual FX).
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation State")]
        [SerializeField] private PlayerAnimationState currentState = PlayerAnimationState.Idle;

        [Header("Animator Component")]
        [SerializeField] private Animator animator;

        [Header("Specialized Subsystem Handlers")]
        [SerializeField] private BattingAnimationHandler battingHandler;
        [SerializeField] private BowlingAnimationHandler bowlingHandler;
        [SerializeField] private FieldingAnimationHandler fieldingHandler;
        [SerializeField] private PlayerVisualEffects visualEffects;

        [Header("Procedural Visual Limbs (Fallback)")]
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;

        private float limbSwingTimer = 0f;

        public PlayerAnimationState CurrentState { get { return currentState; } }
        public Animator AnimatorComponent { get { return animator; } }
        public BattingAnimationHandler BattingHandler { get { return battingHandler; } }
        public BowlingAnimationHandler BowlingHandler { get { return bowlingHandler; } }
        public FieldingAnimationHandler FieldingHandler { get { return fieldingHandler; } }
        public PlayerVisualEffects VisualEffects { get { return visualEffects; } }

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (battingHandler == null) battingHandler = GetComponent<BattingAnimationHandler>();
            if (bowlingHandler == null) bowlingHandler = GetComponent<BowlingAnimationHandler>();
            if (fieldingHandler == null) fieldingHandler = GetComponent<FieldingAnimationHandler>();
            if (visualEffects == null) visualEffects = GetComponent<PlayerVisualEffects>();

            // Wire the shared Animator into all sub-handlers
            if (animator != null)
            {
                if (battingHandler != null) battingHandler.SetAnimator(animator);
                if (bowlingHandler != null) bowlingHandler.SetAnimator(animator);
                if (fieldingHandler != null) fieldingHandler.SetAnimator(animator);
            }
        }

        public void InitializeLimbs(Transform lArm, Transform rArm, Transform lLeg, Transform rLeg)
        {
            leftArm = lArm;
            rightArm = rArm;
            leftLeg = lLeg;
            rightLeg = rLeg;
        }

        public void UpdateAnimation(float speed, bool isGrounded, bool isRunning, bool isBatting, bool isBowling, bool isFielding)
        {
            // Evaluate state
            if (!isGrounded)
            {
                currentState = PlayerAnimationState.Jump;
            }
            else if (isBatting && speed < 0.1f)
            {
                currentState = PlayerAnimationState.Batting;
            }
            else if (isBowling && speed < 0.1f)
            {
                currentState = PlayerAnimationState.Bowling;
            }
            else if (speed < 0.1f)
            {
                currentState = PlayerAnimationState.Idle;
            }
            else if (isRunning || speed > 3.5f)
            {
                currentState = PlayerAnimationState.Run;
            }
            else
            {
                currentState = PlayerAnimationState.Walk;
            }

            // Update Animator if present
            if (animator != null)
            {
                animator.SetFloat("Speed", speed);
                animator.SetBool("IsBatting", isBatting);
                animator.SetBool("IsBowling", isBowling);
                animator.SetBool("IsFielding", isFielding);
            }

            // Procedural swing for placeholder geometry (when animator not driven by clips)
            UpdateProceduralLimbs(speed);
        }

        // Bridge methods for gameplay controllers
        public void PlayBattingShot(BattingShotType shotType, bool isLofted, float exitSpeed = 100f)
        {
            currentState = PlayerAnimationState.Batting;
            if (battingHandler != null)
            {
                battingHandler.TriggerShotAnimation(shotType, isLofted);
            }
            else if (animator != null)
            {
                animator.SetBool("IsBatting", true);
                animator.SetInteger("ActionType", (int)shotType);
                animator.SetTrigger("ActionTrigger");
            }

            if (visualEffects != null && isLofted)
            {
                visualEffects.EnableBatTrail(exitSpeed);
            }
        }

        public void StartBowlingRunUp(BowlingBaseType bowlerType, DeliveryVariation variation, float runUpSpeed = 5.5f)
        {
            currentState = PlayerAnimationState.Bowling;
            if (bowlingHandler != null)
            {
                bowlingHandler.StartRunUp(bowlerType, variation, runUpSpeed);
            }
            else if (animator != null)
            {
                animator.SetBool("IsBowling", true);
                animator.SetFloat("Speed", runUpSpeed);
                animator.SetInteger("ActionType", (int)bowlerType);
                animator.SetTrigger("ActionTrigger");
            }
        }

        public void PlayFieldingAction(FielderActionAnimation action)
        {
            currentState = PlayerAnimationState.Fielding;
            if (fieldingHandler != null)
            {
                fieldingHandler.TriggerAction(action);
            }
            else if (animator != null)
            {
                animator.SetBool("IsFielding", true);
                animator.SetInteger("ActionType", (int)action);
                animator.SetTrigger("ActionTrigger");
            }

            if (visualEffects != null && action == FielderActionAnimation.Diving)
            {
                visualEffects.TriggerFielderSlideParticles();
            }
        }

        private void UpdateProceduralLimbs(float speed)
        {
            if (leftArm == null && rightArm == null && leftLeg == null && rightLeg == null) return;

            if (speed > 0.1f)
            {
                limbSwingTimer += Time.deltaTime * speed * 4f;
                float swingAngle = Mathf.Sin(limbSwingTimer) * 25f;

                if (leftArm != null) leftArm.localRotation = Quaternion.Euler(swingAngle, 0f, 0f);
                if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-swingAngle, 0f, 0f);
                if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-swingAngle * 0.8f, 0f, 0f);
                if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(swingAngle * 0.8f, 0f, 0f);
            }
            else
            {
                limbSwingTimer = 0f;
                if (leftArm != null) leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, Quaternion.identity, Time.deltaTime * 10f);
                if (rightArm != null) rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, Quaternion.identity, Time.deltaTime * 10f);
                if (leftLeg != null) leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
                if (rightLeg != null) rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            }
        }
    }
}
