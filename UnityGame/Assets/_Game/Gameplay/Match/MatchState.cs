using System;

namespace CricketGame.Gameplay.Match
{
    public enum MatchState
    {
        PreMatch,
        Toss,
        Innings1,
        InningsBreak,
        Innings2,
        MatchFinished,
        Paused
    }

    public enum TossChoice
    {
        Heads,
        Tails
    }

    public enum TossDecision
    {
        Bat,
        Bowl
    }
}
