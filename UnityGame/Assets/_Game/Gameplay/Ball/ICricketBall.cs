using System;
using UnityEngine;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;

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
        BallState CurrentState { get; }

        void ApplyBatContact(Vector3 exitVelocity, BattingResult result);
        void ResetBall(Vector3 position);
        void LaunchDelivery(BowlingReleaseData releaseData);
        void DeliverBall(Vector3 releasePos, Vector3 targetPitchSpot, float speedKph, float lateralCurve);

        event Action<BallState> OnBallStateChanged;
        event Action<Vector3> OnPitchBounce;
        event Action<BattingResult> OnBatContact;
    }
}
