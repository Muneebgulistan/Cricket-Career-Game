using UnityEngine;

namespace CricketGame.Camera
{
    public enum CricketCameraMode
    {
        BroadcastCamera,
        BattingCamera,
        BowlingCamera,
        FieldCamera,
        WicketCamera,
        ReplayCamera
    }

    public interface ICricketCamera
    {
        CricketCameraMode Mode { get; }
        void Activate(UnityEngine.Camera camera);
        void Deactivate();
        void UpdateCamera(UnityEngine.Camera camera, float deltaTime);
        void SetTarget(Transform target);
    }
}
