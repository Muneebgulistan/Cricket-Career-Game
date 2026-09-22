using System;
using UnityEngine;
using CricketGame.Gameplay.Batting;

namespace CricketGame.Gameplay.Ball
{
    public class SimpleCricketBall : MonoBehaviour, ICricketBall
    {
        [Header("Ball Physics Settings")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float pitchRestitution = 0.65f;
        [SerializeField] private float pitchFriction = 0.85f;
        [SerializeField] private float groundBounceThreshold = 0.05f;

        [Header("State")]
        [SerializeField] private Vector3 velocity = Vector3.zero;
        [SerializeField] private bool isInPlay = false;
        [SerializeField] private bool hasBounced = false;
        [SerializeField] private bool hasHitBat = false;
        [SerializeField] private float initialSpeedKph = 135f;

        public Vector3 Position { get { return transform.position; } }
        public Vector3 Velocity { get { return velocity; } }
        public Vector3 Direction { get { return velocity.sqrMagnitude > 0.01f ? velocity.normalized : transform.forward; } }
        public bool IsInPlay { get { return isInPlay; } }
        public bool HasBounced { get { return hasBounced; } }
        public bool HasHitBat { get { return hasHitBat; } }
        public float SpeedKph { get { return velocity.magnitude * 3.6f; } }

        public event Action<BattingResult> OnBatContact;

        public void DeliverBall(Vector3 releasePos, Vector3 targetPitchSpot, float speedKph, float lateralCurve)
        {
            transform.position = releasePos;
            initialSpeedKph = speedKph;
            float speedMps = speedKph * (1000f / 3600f);

            Vector3 direction = (targetPitchSpot - releasePos).normalized;
            velocity = direction * speedMps;

            // Apply lateral curve / swing
            velocity.x += lateralCurve;

            isInPlay = true;
            hasBounced = false;
            hasHitBat = false;
        }

        public void ApplyBatContact(Vector3 exitVelocity, BattingResult result)
        {
            velocity = exitVelocity;
            hasHitBat = true;
            hasBounced = false; // Reset bounce flag for post-contact flight

            if (OnBatContact != null)
            {
                OnBatContact(result);
            }
        }

        public void ResetBall(Vector3 position)
        {
            transform.position = position;
            velocity = Vector3.zero;
            isInPlay = false;
            hasBounced = false;
            hasHitBat = false;
        }

        private void Update()
        {
            if (!isInPlay) return;

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;

            // Update position
            transform.position += velocity * Time.deltaTime;

            // Pitch & ground bounce check
            if (transform.position.y <= groundBounceThreshold)
            {
                Vector3 pos = transform.position;
                pos.y = groundBounceThreshold;
                transform.position = pos;

                if (Mathf.Abs(velocity.y) > 0.5f)
                {
                    velocity.y = -velocity.y * pitchRestitution;
                    velocity.x *= pitchFriction;
                    velocity.z *= pitchFriction;
                    hasBounced = true;
                }
                else
                {
                    // Rolling on the ground
                    velocity.y = 0f;
                    velocity.x *= Mathf.Clamp01(1f - (Time.deltaTime * 0.8f));
                    velocity.z *= Mathf.Clamp01(1f - (Time.deltaTime * 0.8f));

                    if (velocity.sqrMagnitude < 0.1f)
                    {
                        velocity = Vector3.zero;
                        isInPlay = false;
                    }
                }
            }

            // Boundary stop check (beyond 85 meters from origin)
            if (transform.position.magnitude > 85f)
            {
                isInPlay = false;
                velocity = Vector3.zero;
            }
        }
    }
}
