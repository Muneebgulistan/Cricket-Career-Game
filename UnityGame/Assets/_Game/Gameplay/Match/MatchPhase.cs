using System;

namespace CricketGame.Gameplay.Match
{
    public enum MatchPhase
    {
        Preparation,
        BowlerRunUp,
        Release,
        BallInFlight,
        BattingStrike,
        FieldingInterception,
        RunningWickets,
        DeliveryResolution,
        OverBreak,
        InningsBreak,
        MatchComplete
    }
}
