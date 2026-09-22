using UnityEngine;
using CricketGame.Gameplay.Batting;

namespace CricketGame.Gameplay.Ball
{
    public interface ICricketBall
    {
        Vector3 Position { get; }
        Vector3 Velocity { get; }
        Vector3 Direction { get; }
        bool IsInPlay { get; }
        bool HasBounced { get; }
        float SpeedKph { get; }

        void ApplyBatContact(Vector3 exitVelocity, BattingResult result);
        void ResetBall(Vector3 position);
    }
}
