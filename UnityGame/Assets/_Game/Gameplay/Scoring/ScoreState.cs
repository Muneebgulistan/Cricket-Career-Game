using System;

namespace CricketGame.Gameplay.Scoring
{
    public enum ScoreState
    {
        Idle,
        ReadyForDelivery,
        ProcessingDelivery,
        OverEnded,
        InningsEnded,
        MatchEnded
    }
}
