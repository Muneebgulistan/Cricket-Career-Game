using System;

namespace CricketGame.Gameplay.Running
{
    public enum RunningState
    {
        Idle,
        Ready,
        RunningToNonStriker,
        RunningToStriker,
        Turning,
        CompletingRun,
        Returning,
        RunCompleted,
        RunOutAttempt,
        RunOut,
        Completed
    }
}
