using System;

namespace CricketGame.Gameplay.Running
{
    public interface IRunningInput
    {
        bool IsRunningRequested { get; }
        bool IsDiveRequested { get; }
        bool IsCancelRequested { get; }

        void RequestRun();
        void RequestDive();
        void RequestCancel();

        void ConsumeRunRequest();
        void ConsumeDiveRequest();
        void ConsumeCancelRequest();

        void ResetInput();
    }
}
