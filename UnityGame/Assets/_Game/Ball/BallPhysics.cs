using UnityEngine;
using CricketGame.Cricket;

namespace CricketGame.Ball
{
    public class BallPhysics : MonoBehaviour
    {
        [Header("3D Ball Trajectory Configuration")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float pitchRestitution = 0.65f;
        [SerializeField] private float pitchFriction = 0.85f;

        private Vector3 velocity;
        private bool isDelivered = false;
        private bool hasBounced = false;

        public void DeliverBall(Vector3 releasePos, Vector3 targetPitchSpot, float speedKph, float swingAngle, float spinTurn)
        {
            transform.position = releasePos;
            float speedMps = speedKph * (1000f / 3600f);
            
            Vector3 direction = (targetPitchSpot - releasePos).normalized;
            velocity = direction * speedMps;
            
            isDelivered = true;
            hasBounced = false;
        }

        private void Update()
        {
            if (!isDelivered) return;

            // Apply gravity and flight movement
            velocity.y += gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;

            // Simple pitch bounce check (Y = 0 is pitch surface)
            if (!hasBounced && transform.position.y <= 0.05f)
            {
                hasBounced = true;
                velocity.y = -velocity.y * pitchRestitution;
                velocity.x *= pitchFriction;
                velocity.z *= pitchFriction;
            }
        }
    }
}
