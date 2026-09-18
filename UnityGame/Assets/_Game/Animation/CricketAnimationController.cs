using UnityEngine;

namespace CricketGame.Animation
{
    public enum AnimationStateId
    {
        IdleStance,
        BattingBacklift,
        BattingDrive,
        BattingPull,
        BattingDefense,
        BowlerRunUp,
        BowlerDeliveryRelease,
        BowlerFollowThrough,
        FielderReady,
        FielderThrow
    }

    public class CricketAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        public void TriggerAnimation(AnimationStateId anim)
        {
            if (animator != null)
            {
                animator.SetTrigger(anim.ToString());
            }
            Debug.Log($"[CricketAnimationController] Played animation: {anim}");
        }
    }
}
