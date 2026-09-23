using System;
using UnityEngine;

namespace CricketGame.Gameplay.Running
{
    [Serializable]
    public class RunningResult
    {
        public int runsCompleted;
        public bool wasRunOut;
        public RunnerRole runOutRunner;
        public CreaseEnd runOutEnd;
        public float marginOfSafety; // Positive = safe by X meters, Negative = short by X meters
        public string summary;

        public RunningResult()
        {
            runsCompleted = 0;
            wasRunOut = false;
            runOutRunner = RunnerRole.Striker;
            runOutEnd = CreaseEnd.StrikerEnd;
            marginOfSafety = 100f;
            summary = "No runs taken";
        }

        public RunningResult(int runs, bool isRunOut, RunnerRole outRunner, CreaseEnd end, float margin, string text)
        {
            runsCompleted = runs;
            wasRunOut = isRunOut;
            runOutRunner = outRunner;
            runOutEnd = end;
            marginOfSafety = margin;
            summary = text;
        }

        public static RunningResult Success(int runs, float margin)
        {
            string desc = runs == 1 ? "1 run taken cleanly" : string.Format("{0} runs completed safely", runs);
            return new RunningResult(runs, false, RunnerRole.Striker, CreaseEnd.StrikerEnd, margin, desc);
        }

        public static RunningResult RunOutOccurred(int runsSoFar, RunnerRole outRunner, CreaseEnd end, float margin)
        {
            string runnerStr = outRunner == RunnerRole.Striker ? "Striker" : "Non-striker";
            string endStr = end == CreaseEnd.StrikerEnd ? "striker's end" : "bowler's end";
            string desc = string.Format("OUT! {0} run out at {1} by {2:F2}m!", runnerStr, endStr, Mathf.Abs(margin));
            return new RunningResult(runsSoFar, true, outRunner, end, margin, desc);
        }
    }
}
