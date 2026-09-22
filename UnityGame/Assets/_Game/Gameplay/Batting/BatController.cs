using System;
using UnityEngine;
using CricketGame.Gameplay.Ball;

namespace CricketGame.Gameplay.Batting
{
    public class BatController : MonoBehaviour
    {
        [Header("Bat Hierarchy References")]
        [SerializeField] private Transform handle;
        [SerializeField] private Transform blade;
        [SerializeField] private Transform sweetSpot;
        [SerializeField] private Collider batCollider;

        [Header("Attachment Settings")]
        [SerializeField] private Vector3 localAttachPosition = new Vector3(0.05f, -0.3f, 0.1f);
        [SerializeField] private Vector3 localAttachRotation = new Vector3(0f, 0f, 15f);

        [Header("Procedural Swing Settings")]
        [SerializeField] private Vector3 readyLocalRotation = new Vector3(10f, 0f, 10f);
        [SerializeField] private Vector3 backliftLocalRotation = new Vector3(-35f, 25f, -20f);
        [SerializeField] private Vector3 contactLocalRotation = new Vector3(45f, -15f, 10f);
        [SerializeField] private Vector3 followThroughLocalRotation = new Vector3(85f, -40f, 25f);

        public Transform SweetSpot { get { return sweetSpot; } }
        public Transform Blade { get { return blade; } }
        public Collider Collider { get { return batCollider; } }

        public Vector3 SweetSpotPosition
        {
            get
            {
                if (sweetSpot != null) return sweetSpot.position;
                if (blade != null) return blade.position;
                return transform.position;
            }
        }

        public event Action<ICricketBall, Vector3> OnBallContact;

        private void Awake()
        {
            if (batCollider == null) batCollider = GetComponent<Collider>();
            if (handle == null) handle = transform.Find("Handle");
            if (blade == null) blade = transform.Find("Blade");
            if (sweetSpot == null)
            {
                Transform spot = transform.Find("SweetSpot");
                if (spot != null) sweetSpot = spot;
                else if (blade != null) sweetSpot = blade;
                else sweetSpot = transform;
            }
        }

        public void AttachToHand(Transform handTransform)
        {
            if (handTransform == null) return;

            transform.SetParent(handTransform);
            transform.localPosition = localAttachPosition;
            transform.localRotation = Quaternion.Euler(localAttachRotation);
            transform.localScale = Vector3.one;
        }

        public void ApplyProceduralPose(BattingState state, float progress)
        {
            Quaternion targetRot = Quaternion.identity;

            switch (state)
            {
                case BattingState.Ready:
                case BattingState.Idle:
                    targetRot = Quaternion.Euler(readyLocalRotation);
                    break;
                case BattingState.Backlift:
                    targetRot = Quaternion.Slerp(Quaternion.Euler(readyLocalRotation), Quaternion.Euler(backliftLocalRotation), progress);
                    break;
                case BattingState.Swing:
                    targetRot = Quaternion.Slerp(Quaternion.Euler(backliftLocalRotation), Quaternion.Euler(contactLocalRotation), progress);
                    break;
                case BattingState.Contact:
                    targetRot = Quaternion.Euler(contactLocalRotation);
                    break;
                case BattingState.FollowThrough:
                    targetRot = Quaternion.Slerp(Quaternion.Euler(contactLocalRotation), Quaternion.Euler(followThroughLocalRotation), progress);
                    break;
                case BattingState.Recovery:
                    targetRot = Quaternion.Slerp(Quaternion.Euler(followThroughLocalRotation), Quaternion.Euler(readyLocalRotation), progress);
                    break;
            }

            transform.localRotation = targetRot;
        }

        private void OnTriggerEnter(Collider other)
        {
            ICricketBall ball = other.GetComponent<ICricketBall>();
            if (ball != null)
            {
                Vector3 contactPoint = other.ClosestPoint(SweetSpotPosition);
                TriggerContact(ball, contactPoint);
            }
        }

        public void TriggerContact(ICricketBall ball, Vector3 contactPoint)
        {
            if (OnBallContact != null)
            {
                OnBallContact(ball, contactPoint);
            }
        }
    }
}
