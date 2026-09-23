using System;
using UnityEngine;
using CricketGame.Players;

namespace CricketGame.Gameplay.Running
{
    public class RunningManager : MonoBehaviour
    {
        public static RunningManager Instance { get; private set; }

        [Header("Settings & Components")]
        [SerializeField] private RunningSettings settings = new RunningSettings();
        [SerializeField] private RunningController strikerRunner;
        [SerializeField] private RunningController nonStrikerRunner;
        [SerializeField] private RunningInput runningInput;

        [Header("Runtime State")]
        [SerializeField] private RunningRuntimeData runtimeData = new RunningRuntimeData();

        private bool runnersSynchronized;
        private int completedRuns;
        private bool isFinished;

        public RunningSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public RunningRuntimeData RuntimeData
        {
            get { return runtimeData; }
        }

        public RunningController StrikerRunner
        {
            get { return strikerRunner; }
            set { strikerRunner = value; }
        }

        public RunningController NonStrikerRunner
        {
            get { return nonStrikerRunner; }
            set { nonStrikerRunner = value; }
        }

        public int CompletedRuns
        {
            get { return completedRuns; }
        }

        public bool IsRunActive
        {
            get { return runtimeData.isRunActive; }
        }

        public event Action OnRunStarted;
        public event Action<int> OnRunCompleted;
        public event Action<RunnerRole, CreaseEnd> OnRunOut;
        public event Action<RunningResult> OnRunningFinished;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (runningInput == null)
            {
                runningInput = GetComponent<RunningInput>();
                if (runningInput == null)
                {
                    runningInput = gameObject.AddComponent<RunningInput>();
                }
            }
            InitializeRunners();
        }

        public void InitializeRunners()
        {
            if (settings == null) settings = new RunningSettings();
            if (runtimeData == null) runtimeData = new RunningRuntimeData();
            runtimeData.Reset();
            completedRuns = 0;
            isFinished = false;

            if (strikerRunner != null)
            {
                strikerRunner.Initialize(RunnerRole.Striker, runtimeData.strikerHomeEnd, settings);
                strikerRunner.OnReachedCrease -= HandleRunnerReachedCrease;
                strikerRunner.OnReachedCrease += HandleRunnerReachedCrease;
            }

            if (nonStrikerRunner != null)
            {
                nonStrikerRunner.Initialize(RunnerRole.NonStriker, runtimeData.nonStrikerHomeEnd, settings);
                nonStrikerRunner.OnReachedCrease -= HandleRunnerReachedCrease;
                nonStrikerRunner.OnReachedCrease += HandleRunnerReachedCrease;
            }
        }

        public void StartDeliveryPlay()
        {
            runtimeData.isRunActive = false;
            completedRuns = 0;
            isFinished = false;
            runnersSynchronized = false;
            if (runningInput != null) runningInput.ResetInput();
        }

        public bool AttemptRun(PlayerAttributes strikerAttr, PlayerAttributes nonStrikerAttr)
        {
            if (isFinished) return false;

            // If not active, start run 1
            if (!runtimeData.isRunActive)
            {
                runtimeData.isRunActive = true;
                runtimeData.runsAttempted = 1;

                float strikerSpeed = RunningEvaluator.CalculateRunnerSpeed(strikerAttr, settings, completedRuns);
                float nonStrikerSpeed = RunningEvaluator.CalculateRunnerSpeed(nonStrikerAttr, settings, completedRuns);

                runtimeData.strikerCurrentSpeed = strikerSpeed;
                runtimeData.nonStrikerCurrentSpeed = nonStrikerSpeed;

                if (strikerRunner != null)
                {
                    strikerRunner.StartRun(strikerSpeed, runtimeData.strikerTargetEnd);
                }
                if (nonStrikerRunner != null)
                {
                    nonStrikerRunner.StartRun(nonStrikerSpeed, runtimeData.nonStrikerTargetEnd);
                }

                if (OnRunStarted != null) OnRunStarted();
                return true;
            }
            else if (strikerRunner != null && nonStrikerRunner != null &&
                     strikerRunner.state == RunningState.RunCompleted &&
                     nonStrikerRunner.state == RunningState.RunCompleted)
            {
                // Both runners reached, attempt next run!
                runtimeData.runsAttempted++;
                runtimeData.strikerTargetEnd = (runtimeData.strikerTargetEnd == CreaseEnd.StrikerEnd) ? CreaseEnd.NonStrikerEnd : CreaseEnd.StrikerEnd;
                runtimeData.nonStrikerTargetEnd = (runtimeData.nonStrikerTargetEnd == CreaseEnd.StrikerEnd) ? CreaseEnd.NonStrikerEnd : CreaseEnd.StrikerEnd;

                float strikerSpeed = RunningEvaluator.CalculateRunnerSpeed(strikerAttr, settings, completedRuns);
                float nonStrikerSpeed = RunningEvaluator.CalculateRunnerSpeed(nonStrikerAttr, settings, completedRuns);

                strikerRunner.TurnForAnotherRun(strikerSpeed, runtimeData.strikerTargetEnd);
                nonStrikerRunner.TurnForAnotherRun(nonStrikerSpeed, runtimeData.nonStrikerTargetEnd);

                if (OnRunStarted != null) OnRunStarted();
                return true;
            }

            return false;
        }

        public void TriggerDive(RunnerRole runner)
        {
            if (runner == RunnerRole.Striker && strikerRunner != null)
            {
                strikerRunner.TriggerDive();
                runtimeData.strikerDiving = true;
            }
            else if (runner == RunnerRole.NonStriker && nonStrikerRunner != null)
            {
                nonStrikerRunner.TriggerDive();
                runtimeData.nonStrikerDiving = true;
            }
        }

        public void CancelOrReturn()
        {
            if (!runtimeData.isRunActive || completedRuns > 0) return;

            if (strikerRunner != null) strikerRunner.ReturnToCrease();
            if (nonStrikerRunner != null) nonStrikerRunner.ReturnToCrease();
        }

        private void HandleRunnerReachedCrease(RunningController runner)
        {
            // Check if BOTH runners have completed this run
            bool strikerDone = (strikerRunner == null || strikerRunner.state == RunningState.RunCompleted);
            bool nonStrikerDone = (nonStrikerRunner == null || nonStrikerRunner.state == RunningState.RunCompleted);

            if (strikerDone && nonStrikerDone)
            {
                completedRuns++;
                runtimeData.runsCompleted = completedRuns;
                runtimeData.SwapEnds();

                if (OnRunCompleted != null)
                {
                    OnRunCompleted(completedRuns);
                }
            }
        }

        public RunningResult EvaluateThrowAtStumps(CreaseEnd targetEnd, float ballArrivalTime, float runnerArrivalStrikerEnd, float runnerArrivalNonStrikerEnd)
        {
            if (!runtimeData.isRunActive)
            {
                return RunningResult.Success(completedRuns, 10f);
            }

            // Check which runner is running toward targetEnd
            bool strikerHeadingToTarget = (runtimeData.strikerTargetEnd == targetEnd);
            RunnerRole targetRunner = strikerHeadingToTarget ? RunnerRole.Striker : RunnerRole.NonStriker;
            float runnerArrival = strikerHeadingToTarget ? runnerArrivalStrikerEnd : runnerArrivalNonStrikerEnd;
            Vector3 runnerPos = strikerHeadingToTarget ? 
                (strikerRunner != null ? strikerRunner.transform.position : Vector3.zero) :
                (nonStrikerRunner != null ? nonStrikerRunner.transform.position : Vector3.zero);

            RunningResult result = RunningEvaluator.EvaluateRunOut(
                targetRunner, 
                runnerPos, 
                targetEnd, 
                runnerArrival, 
                ballArrivalTime, 
                completedRuns);

            if (result.wasRunOut)
            {
                isFinished = true;
                runtimeData.isRunActive = false;
                if (targetRunner == RunnerRole.Striker && strikerRunner != null)
                {
                    strikerRunner.MarkRunOut();
                }
                else if (targetRunner == RunnerRole.NonStriker && nonStrikerRunner != null)
                {
                    nonStrikerRunner.MarkRunOut();
                }

                if (OnRunOut != null)
                {
                    OnRunOut(targetRunner, targetEnd);
                }
                if (OnRunningFinished != null)
                {
                    OnRunningFinished(result);
                }
            }

            return result;
        }

        public RunningResult EndRunningPlay()
        {
            isFinished = true;
            runtimeData.isRunActive = false;

            if (strikerRunner != null) strikerRunner.ResetToCrease();
            if (nonStrikerRunner != null) nonStrikerRunner.ResetToCrease();

            RunningResult res = RunningResult.Success(completedRuns, 10f);
            if (OnRunningFinished != null)
            {
                OnRunningFinished(res);
            }
            return res;
        }

        private void Update()
        {
            if (runningInput != null)
            {
                if (runningInput.IsRunningRequested)
                {
                    runningInput.ConsumeRunRequest();
                    AttemptRun(null, null);
                }
                if (runningInput.IsDiveRequested)
                {
                    runningInput.ConsumeDiveRequest();
                    TriggerDive(RunnerRole.Striker);
                }
                if (runningInput.IsCancelRequested)
                {
                    runningInput.ConsumeCancelRequest();
                    CancelOrReturn();
                }
            }
        }
    }
}
