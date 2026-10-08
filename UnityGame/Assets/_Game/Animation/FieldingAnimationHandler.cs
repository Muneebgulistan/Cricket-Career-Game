using System;
using UnityEngine;
using CricketGame.Gameplay.Ball;
using CricketGame.Gameplay.Fielding;

namespace CricketGame.Animation
{
    public enum FielderActionAnimation
    {
        Anticipation,
        Sprinting,
        Diving,
        GroundPickup,
        Catching,
        Throwing
    }

    public class FieldingAnimationHandler : MonoBehaviour
    {
        [Header("Animator & Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private SimpleCricketBall targetBall;

        [Header("Look IK Settings")]
        [SerializeField] [Range(0f, 1f)] private float ballTrackingWeight = 0.9f;
        [SerializeField] private float lookDistanceMax = 60f;

        [Header("Root Motion / Translation Mode")]
        [SerializeField] private bool useScriptedDiveTranslation = true;

        private FielderActionAnimation currentAction = FielderActionAnimation.Anticipation;
        private bool isTrackingBall = true;
        private bool isDiving = false;

        public FielderActionAnimation CurrentAction { get { return currentAction; } }
        public bool IsDiving { get { return isDiving; } }
        public float BallTrackingWeight { get { return ballTrackingWeight; } set { ballTrackingWeight = Mathf.Clamp01(value); } }
        public bool UseScriptedDiveTranslation { get { return useScriptedDiveTranslation; } set { useScriptedDiveTranslation = value; } }

        public event Action<FielderActionAnimation> OnFieldingAnimationTriggered;
        public event Action OnBallPickupFrame;
        public event Action OnBallThrowFrame;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (targetBall == null) targetBall = FindAnyObjectByType<SimpleCricketBall>();
        }

        public void SetAnimator(Animator anim)
        {
            animator = anim;
        }

        public void SetTargetBall(SimpleCricketBall ball)
        {
            targetBall = ball;
        }

        public void SetLocomotion(float speed, float horizontalDirection = 0f)
        {
            if (animator != null)
            {
                animator.SetBool("IsFielding", true);
                animator.SetFloat("Speed", speed);
                animator.SetFloat("HorizontalMovement", horizontalDirection);
            }
        }

        public void TriggerAction(FielderActionAnimation action)
        {
            currentAction = action;
            isDiving = (action == FielderActionAnimation.Diving);

            if (animator != null)
            {
                animator.SetBool("IsFielding", true);
                animator.SetInteger("ActionType", (int)action);
                animator.SetTrigger("ActionTrigger");

                if (isDiving && !useScriptedDiveTranslation)
                {
                    animator.applyRootMotion = true;
                }
                else
                {
                    animator.applyRootMotion = false;
                }
            }

            if (OnFieldingAnimationTriggered != null)
            {
                OnFieldingAnimationTriggered(action);
            }
        }

        public void ResetToReady()
        {
            currentAction = FielderActionAnimation.Anticipation;
            isDiving = false;

            if (animator != null)
            {
                animator.SetBool("IsFielding", true);
                animator.SetFloat("Speed", 0f);
                animator.applyRootMotion = false;
            }
        }

        // Animation Events
        public void OnAnimationEvent_BallPickup()
        {
            if (OnBallPickupFrame != null)
            {
                OnBallPickupFrame();
            }
        }

        public void OnAnimationEvent_BallReleaseThrow()
        {
            if (OnBallThrowFrame != null)
            {
                OnBallThrowFrame();
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (animator == null || !isTrackingBall || ballTrackingWeight <= 0.001f) return;

            if (targetBall != null && targetBall.IsInPlay)
            {
                Vector3 ballPos = targetBall.Position;
                float dist = Vector3.Distance(transform.position, ballPos);
                if (dist <= lookDistanceMax)
                {
                    animator.SetLookAtWeight(ballTrackingWeight, 0.2f, 0.9f, 0.3f, 0.5f);
                    animator.SetLookAtPosition(ballPos);
                }
            }
        }
    }
}
