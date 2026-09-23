using System;

namespace CricketGame.Gameplay.Running
{
    [Serializable]
    public class RunningDecision
    {
        public RunnerRole runner;
        public RunningActionType action;
        public CreaseEnd destination;
        public float estimatedTime;
        public float riskRating;
        public bool isCommitted;

        public RunningDecision()
        {
            runner = RunnerRole.Striker;
            action = RunningActionType.None;
            destination = CreaseEnd.NonStrikerEnd;
            estimatedTime = 0f;
            riskRating = 0f;
            isCommitted = false;
        }

        public RunningDecision(RunnerRole runnerRole, RunningActionType actionType, CreaseEnd destEnd, float risk)
        {
            runner = runnerRole;
            action = actionType;
            destination = destEnd;
            estimatedTime = 0f;
            riskRating = risk;
            isCommitted = true;
        }
    }
}
