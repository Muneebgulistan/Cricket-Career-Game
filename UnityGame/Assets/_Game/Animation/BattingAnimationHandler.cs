using System;
using UnityEngine;
using CricketGame.Gameplay.Ball;
using CricketGame.Gameplay.Batting;

namespace CricketGame.Animation
{
    public class BattingAnimationHandler : MonoBehaviour
    {
        [Header("Animator & IK Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private BatController batController;
        [SerializeField] private SimpleCricketBall targetBall;

        [Header("Hand IK Targets")]
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;
        [SerializeField] [Range(0f, 1f)] private float handIKWeight = 1.0f;

        [Header("Look IK Settings")]
        [SerializeField] [Range(0f, 1f)] private float headLookWeight = 0.85f;
        [SerializeField] private float lookDistanceMax = 35f;

        [Header("Procedural Grip Offsets")]
        [SerializeField] private Vector3 leftHandHandleOffset = new Vector3(0f, 0.12f, 0f);
        [SerializeField] private Vector3 rightHandHandleOffset = new Vector3(0f, -0.06f, 0f);

        private BattingShotType currentShotType = BattingShotType.Defensive;
        private bool isTrackingBall = true;
        private bool isSwinging = false;

        public BattingShotType CurrentShotType { get { return currentShotType; } }
        public bool IsSwinging { get { return isSwinging; } }
        public float HandIKWeight { get { return handIKWeight; } set { handIKWeight = Mathf.Clamp01(value); } }
        public float HeadLookWeight { get { return headLookWeight; } set { headLookWeight = Mathf.Clamp01(value); } }

        public event Action<BattingShotType> OnShotAnimationTriggered;
        public event Action OnBatSwingPeak;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (batController == null) batController = GetComponentInChildren<BatController>();
            if (targetBall == null) targetBall = FindFirstObjectByType<SimpleCricketBall>();
        }

        public void SetTargetBall(SimpleCricketBall ball)
        {
            targetBall = ball;
        }

        public void SetAnimator(Animator anim)
        {
            animator = anim;
        }

        public void SetBatController(BatController bat)
        {
            batController = bat;
        }

        public void SetIKTargets(Transform leftHand, Transform rightHand)
        {
            leftHandTarget = leftHand;
            rightHandTarget = rightHand;
        }

        public void TriggerShotAnimation(BattingShotType shotType, bool isLofted)
        {
            currentShotType = shotType;
            isSwinging = true;

            if (animator != null)
            {
                animator.SetBool("IsBatting", true);
                animator.SetInteger("ActionType", (int)shotType);
                animator.SetFloat("VerticalMovement", isLofted ? 1.0f : 0.0f);
                animator.SetTrigger("ActionTrigger");
            }

            if (OnShotAnimationTriggered != null)
            {
                OnShotAnimationTriggered(shotType);
            }
        }

        public void ResetToStance()
        {
            isSwinging = false;
            isTrackingBall = true;

            if (animator != null)
            {
                animator.SetBool("IsBatting", true);
                animator.SetFloat("Speed", 0f);
                animator.SetInteger("ActionType", (int)BattingShotType.Defensive);
            }
        }

        public void OnAnimationEvent_BatSwingPeak()
        {
            if (OnBatSwingPeak != null)
            {
                OnBatSwingPeak();
            }
        }

        public void OnAnimationEvent_FollowThroughComplete()
        {
            isSwinging = false;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (animator == null) return;

            ApplyHandIK();
            ApplyLookIK();
        }

        public void ApplyHandIK()
        {
            if (animator == null || handIKWeight <= 0.001f) return;

            Vector3 lPos;
            Quaternion lRot;
            Vector3 rPos;
            Quaternion rRot;

            CalculateHandIKTransforms(out lPos, out lRot, out rPos, out rRot);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, lPos);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, lRot);

            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handIKWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, handIKWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rPos);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rRot);
        }

        public void CalculateHandIKTransforms(out Vector3 leftPos, out Quaternion leftRot, out Vector3 rightPos, out Quaternion rightRot)
        {
            if (leftHandTarget != null && rightHandTarget != null)
            {
                leftPos = leftHandTarget.position;
                leftRot = leftHandTarget.rotation;
                rightPos = rightHandTarget.position;
                rightRot = rightHandTarget.rotation;
                return;
            }

            Transform handleTransform = batController != null ? batController.transform : transform;
            leftPos = handleTransform.position + handleTransform.TransformDirection(leftHandHandleOffset);
            leftRot = handleTransform.rotation;
            rightPos = handleTransform.position + handleTransform.TransformDirection(rightHandHandleOffset);
            rightRot = handleTransform.rotation;
        }

        public void ApplyLookIK()
        {
            if (animator == null || !isTrackingBall || headLookWeight <= 0.001f) return;

            if (targetBall != null && targetBall.IsInPlay)
            {
                Vector3 ballPos = targetBall.Position;
                float dist = Vector3.Distance(transform.position, ballPos);
                if (dist <= lookDistanceMax)
                {
                    animator.SetLookAtWeight(headLookWeight, 0.1f, 0.85f, 0.4f, 0.6f);
                    animator.SetLookAtPosition(ballPos);
                    return;
                }
            }

            // Default look at bowler delivery area if ball not active
            Vector3 defaultTarget = transform.position + transform.forward * 12f + Vector3.up * 1.5f;
            animator.SetLookAtWeight(0.25f, 0f, 0.3f, 0f, 0.5f);
            animator.SetLookAtPosition(defaultTarget);
        }
    }
}
