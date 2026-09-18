using UnityEngine;

namespace CricketGame.Camera
{
    public enum CameraViewMode
    {
        BowlerRunUp,
        DeliveryFollow,
        BoundaryTrack,
        SixElevation,
        WicketReplay
    }

    public class BroadcastCamera : MonoBehaviour
    {
        public CameraViewMode currentView = CameraViewMode.DeliveryFollow;
        [SerializeField] private Transform targetBall;
        [SerializeField] private Vector3 defaultOffset = new Vector3(0, 3.5f, -12f);

        public void SetCameraMode(CameraViewMode mode)
        {
            currentView = mode;
            Debug.Log($"[BroadcastCamera] Switched to camera view: {mode}");
        }

        private void LateUpdate()
        {
            if (targetBall != null && currentView == CameraViewMode.DeliveryFollow)
            {
                transform.position = Vector3.Lerp(transform.position, targetBall.position + defaultOffset, Time.deltaTime * 5f);
            }
        }
    }
}
