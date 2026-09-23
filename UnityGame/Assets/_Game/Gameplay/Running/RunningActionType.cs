using System;

namespace CricketGame.Gameplay.Running
{
    public enum RunningActionType
    {
        None,
        StartRun,
        TurnForSecond,
        TurnForThird,
        ReturnToCrease,
        Dive,
        Cancel
    }

    public enum RunnerRole
    {
        Striker,
        NonStriker
    }

    public enum CreaseEnd
    {
        StrikerEnd,
        NonStrikerEnd
    }
}
