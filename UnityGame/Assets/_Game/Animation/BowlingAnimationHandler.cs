using System;
using UnityEngine;
using CricketGame.Gameplay.Bowling;

namespace CricketGame.Animation
{
    public class BowlingAnimationHandler : MonoBehaviour
    {
        [Header("Animator & Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private BowlingController bowlingController;

        [Header("Delivery Animation Mapping")]
        [SerializeField] private BowlingBaseType currentBowlerType = BowlingBaseType.Fast;
        [SerializeField] private DeliveryVariation currentVariation = DeliveryVariation.GoodLength;

        [Header("Animation State Flags")]
        private bool isRunningUp = false;
        private bool hasFrontFootLanded = false;
        private bool hasReleasedBall = false;

        public bool IsRunningUp { get { return isRunningUp; } }
        public bool HasFrontFootLanded { get { return hasFrontFootLanded; } }
        public bool HasReleasedBall { get { return hasReleasedBall; } }
        public BowlingBaseType CurrentBowlerType { get { return currentBowlerType; } }

        public event Action OnFrontFootLandedEvent;
        public event Action OnBallReleaseEvent;
        public event Action OnFollowThroughCompleteEvent;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (bowlingController == null) bowlingController = GetComponent<BowlingController>();
        }

        public void SetAnimator(Animator anim)
        {
            animator = anim;
        }

        public void SetBowlingController(BowlingController controller)
        {
            bowlingController = controller;
        }

        public void StartRunUp(BowlingBaseType bowlerType, DeliveryVariation variation, float runUpSpeed = 5.5f)
        {
            currentBowlerType = bowlerType;
            currentVariation = variation;
            isRunningUp = true;
            hasFrontFootLanded = false;
            hasReleasedBall = false;

            if (animator != null)
            {
                animator.SetBool("IsBowling", true);
                animator.SetFloat("Speed", runUpSpeed);
                animator.SetInteger("ActionType", (int)bowlerType);
                animator.SetTrigger("ActionTrigger");
            }
        }

        public void UpdateRunUpProgress(float progress01, float currentSpeed)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", currentSpeed);
                animator.SetFloat("HorizontalMovement", progress01);
            }
        }

        public void TriggerDeliveryStride()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 3.0f);
            }
        }

        // Animation Event Callbacks
        public void OnAnimationEvent_FrontFootLand()
        {
            hasFrontFootLanded = true;
            if (OnFrontFootLandedEvent != null)
            {
                OnFrontFootLandedEvent();
            }
        }

        public void OnAnimationEvent_BallRelease()
        {
            hasReleasedBall = true;
            if (OnBallReleaseEvent != null)
            {
                OnBallReleaseEvent();
            }

            if (bowlingController != null && bowlingController.CurrentState == BowlingState.DeliveryStride)
            {
                bowlingController.ExecuteBallRelease();
            }
        }

        public void OnAnimationEvent_FollowThroughComplete()
        {
            isRunningUp = false;
            if (animator != null)
            {
                animator.SetBool("IsBowling", false);
                animator.SetFloat("Speed", 0f);
            }

            if (OnFollowThroughCompleteEvent != null)
            {
                OnFollowThroughCompleteEvent();
            }
        }
    }
}
