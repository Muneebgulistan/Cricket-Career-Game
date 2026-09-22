using System;
using UnityEngine;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;

namespace CricketGame.Gameplay.Ball
{
    public class SimpleCricketBall : MonoBehaviour, ICricketBall
    {
        [Header("Physics Settings")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float pitchRestitution = 0.65f;
        [SerializeField] private float pitchFriction = 0.85f;
        [SerializeField] private float airDrag = 0.02f;
        [SerializeField] private float groundBounceThreshold = 0.05f;

        [Header("Runtime State")]
        [SerializeField] private BallState currentState = BallState.Held;
        [SerializeField] private Vector3 velocity = Vector3.zero;
        [SerializeField] private bool isInPlay = false;
        [SerializeField] private bool hasBounced = false;
        [SerializeField] private bool hasHitBat = false;
        [SerializeField] private float initialSpeedKph = 135f;

        [Header("Delivery Parameters")]
        [SerializeField] private float swingAcceleration = 0f;
        [SerializeField] private float seamDeviationAngle = 0f;
        [SerializeField] private float spinTurnAngle = 0f;
        [SerializeField] private float bounceMultiplier = 1.0f;

        public Vector3 Position { get { return transform.position; } }
        public Vector3 Velocity { get { return velocity; } }
        public Vector3 Direction { get { return velocity.sqrMagnitude > 0.01f ? velocity.normalized : transform.forward; } }
        public bool IsInPlay { get { return isInPlay; } }
        public bool HasBounced { get { return hasBounced; } }
        public bool HasHitBat { get { return hasHitBat; } }
        public float SpeedKph { get { return velocity.magnitude * 3.6f; } }
        public BallState CurrentState { get { return currentState; } }

        public event Action<BallState> OnBallStateChanged;
        public event Action<Vector3> OnPitchBounce;
        public event Action<BattingResult> OnBatContact;

        public void SetState(BallState newState)
        {
            currentState = newState;
            if (OnBallStateChanged != null)
            {
                OnBallStateChanged(newState);
            }
        }

        public void LaunchDelivery(BowlingReleaseData releaseData)
        {
            if (releaseData == null) return;

            transform.position = releaseData.releasePosition;
            velocity = releaseData.initialVelocity;
            initialSpeedKph = releaseData.speedKph;
            swingAcceleration = releaseData.swingAcceleration;
            seamDeviationAngle = releaseData.seamDeviationAngle;
            spinTurnAngle = releaseData.spinTurnAngle;
            bounceMultiplier = releaseData.bounceMultiplier;

            isInPlay = true;
            hasBounced = false;
            hasHitBat = false;

            SetState(BallState.Released);
            SetState(BallState.InFlight);
        }

        public void DeliverBall(Vector3 releasePos, Vector3 targetPitchSpot, float speedKph, float lateralCurve)
        {
            transform.position = releasePos;
            initialSpeedKph = speedKph;
            float speedMps = speedKph * (1000f / 3600f);

            Vector3 direction = (targetPitchSpot - releasePos).normalized;
            velocity = direction * speedMps;

            swingAcceleration = lateralCurve * 4.5f; // Convert lateral curve into swing acceleration
            seamDeviationAngle = 0f;
            spinTurnAngle = 0f;
            bounceMultiplier = 1.0f;

            isInPlay = true;
            hasBounced = false;
            hasHitBat = false;

            SetState(BallState.Released);
            SetState(BallState.InFlight);
        }

        public void ApplyBatContact(Vector3 exitVelocity, BattingResult result)
        {
            velocity = exitVelocity;
            hasHitBat = true;
            hasBounced = false; // Reset bounce flag for post-contact flight
            swingAcceleration = 0f; // Aerodynamic bowling swing ends upon bat contact

            SetState(BallState.Hit);

            if (OnBatContact != null)
            {
                OnBatContact(result);
            }
        }

        public void ResetBall(Vector3 position)
        {
            transform.position = position;
            velocity = Vector3.zero;
            swingAcceleration = 0f;
            seamDeviationAngle = 0f;
            spinTurnAngle = 0f;
            bounceMultiplier = 1.0f;
            isInPlay = false;
            hasBounced = false;
            hasHitBat = false;

            SetState(BallState.Held);
        }

        public void PickUpBall(Transform holder = null)
        {
            isInPlay = false;
            velocity = Vector3.zero;
            swingAcceleration = 0f;
            if (holder != null)
            {
                transform.position = holder.position;
            }
            SetState(BallState.Held);
        }

        public void LaunchThrow(Vector3 origin, Vector3 throwVelocity)
        {
            transform.position = origin;
            velocity = throwVelocity;
            initialSpeedKph = throwVelocity.magnitude * 3.6f;
            swingAcceleration = 0f;
            seamDeviationAngle = 0f;
            spinTurnAngle = 0f;
            bounceMultiplier = 0.8f;

            isInPlay = true;
            hasBounced = false;
            hasHitBat = false;

            SetState(BallState.InFlight);
        }

        public void StopBall()
        {
            velocity = Vector3.zero;
            isInPlay = false;
            SetState(BallState.Dead);
        }

        private void Update()
        {
            if (!isInPlay) return;

            // Apply air drag
            if (airDrag > 0f)
            {
                velocity -= velocity * (airDrag * Time.deltaTime);
            }

            // Apply aerodynamic swing force while ball is in flight before bounce
            if (!hasBounced && currentState == BallState.InFlight)
            {
                velocity.x += swingAcceleration * Time.deltaTime;
            }

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;

            // Update position
            transform.position += velocity * Time.deltaTime;

            // Pitch & ground bounce check
            if (transform.position.y <= groundBounceThreshold)
            {
                HandleGroundBounce();
            }

            // Out-of-bounds check (beyond 85m from pitch center)
            if (transform.position.magnitude > 85f)
            {
                isInPlay = false;
                velocity = Vector3.zero;
                SetState(BallState.Dead);
            }
        }

        private void HandleGroundBounce()
        {
            Vector3 pos = transform.position;
            pos.y = groundBounceThreshold;
            transform.position = pos;

            if (Mathf.Abs(velocity.y) > 0.5f)
            {
                // Active bounce on pitch surface
                float effectiveRestitution = pitchRestitution * bounceMultiplier;
                velocity.y = -velocity.y * effectiveRestitution;
                velocity.x *= pitchFriction;
                velocity.z *= pitchFriction;

                if (!hasBounced)
                {
                    hasBounced = true;
                    SetState(BallState.Bounced);

                    // Apply seam deviation
                    if (Mathf.Abs(seamDeviationAngle) > 0.01f)
                    {
                        Quaternion seamRot = Quaternion.AngleAxis(seamDeviationAngle, Vector3.up);
                        velocity = seamRot * velocity;
                    }

                    // Apply spin turn
                    if (Mathf.Abs(spinTurnAngle) > 0.01f)
                    {
                        Quaternion spinRot = Quaternion.AngleAxis(spinTurnAngle, Vector3.up);
                        velocity = spinRot * velocity;
                    }

                    if (OnPitchBounce != null)
                    {
                        OnPitchBounce(pos);
                    }

                    SetState(BallState.PostBounce);
                }
            }
            else
            {
                // Rolling on ground surface
                velocity.y = 0f;
                velocity.x *= Mathf.Clamp01(1f - (Time.deltaTime * 1.5f));
                velocity.z *= Mathf.Clamp01(1f - (Time.deltaTime * 1.5f));

                if (velocity.sqrMagnitude < 0.1f)
                {
                    velocity = Vector3.zero;
                    isInPlay = false;
                    SetState(BallState.Dead);
                }
            }
        }
    }
}
