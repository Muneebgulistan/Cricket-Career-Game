using UnityEngine;

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

    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation State")]
        [SerializeField] private PlayerAnimationState currentState = PlayerAnimationState.Idle;

        [Header("Animator Component (Optional)")]
        [SerializeField] private Animator animator;

        [Header("Procedural Visual Limbs (Fallback)")]
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;

        private float limbSwingTimer = 0f;

        public PlayerAnimationState CurrentState
        {
            get { return currentState; }
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
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
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetFloat("Speed", speed);
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetBool("IsRunning", isRunning);
                animator.SetBool("IsBatting", isBatting);
                animator.SetBool("IsBowling", isBowling);
                animator.SetBool("IsFielding", isFielding);
            }

            // Procedural swing for placeholder geometry
            UpdateProceduralLimbs(speed);
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
