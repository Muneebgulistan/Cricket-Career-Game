using System;
using UnityEngine;
using CricketGame.Players;

namespace CricketGame.Gameplay.Running
{
    public class RunningController : MonoBehaviour
    {
        [Header("Runner Configuration")]
        public RunnerRole role = RunnerRole.Striker;
        public CreaseEnd homeEnd = CreaseEnd.StrikerEnd;
        public CreaseEnd targetEnd = CreaseEnd.NonStrikerEnd;

        [Header("Runtime State")]
        public RunningState state = RunningState.Idle;
        public float currentSpeed = 0f;
        public bool isDiving = false;
        public float currentRunProgress = 0f; // 0 to 1

        private RunningSettings settings;
        private Vector3 startPosition;
        private Vector3 destinationPosition;
        private float runDuration = 2.8f;
        private float runTimer = 0f;

        public event Action<RunningController> OnReachedCrease;
        public event Action<RunningController> OnRunOut;

        public void Initialize(RunnerRole runnerRole, CreaseEnd initialHome, RunningSettings runSettings)
        {
            role = runnerRole;
            homeEnd = initialHome;
            targetEnd = (initialHome == CreaseEnd.StrikerEnd) ? CreaseEnd.NonStrikerEnd : CreaseEnd.StrikerEnd;
            settings = runSettings != null ? runSettings : new RunningSettings();
            state = RunningState.Idle;
            isDiving = false;
            currentRunProgress = 0f;

            transform.position = RunningTarget.GetCreasePosition(homeEnd, role == RunnerRole.Striker);
        }

        public void StartRun(float runnerSpeed, CreaseEnd destination)
        {
            targetEnd = destination;
            currentSpeed = runnerSpeed;
            runDuration = RunningEvaluator.CalculateSingleRunDuration(runnerSpeed);
            runTimer = 0f;
            currentRunProgress = 0f;
            isDiving = false;

            startPosition = transform.position;
            destinationPosition = RunningTarget.GetCreasePosition(destination, role == RunnerRole.Striker);

            if (destination == CreaseEnd.NonStrikerEnd)
            {
                state = RunningState.RunningToNonStriker;
            }
            else
            {
                state = RunningState.RunningToStriker;
            }
        }

        public void TurnForAnotherRun(float runnerSpeed, CreaseEnd newDestination)
        {
            state = RunningState.Turning;
            homeEnd = targetEnd;
            targetEnd = newDestination;
            currentSpeed = runnerSpeed * (settings != null ? settings.turnSpeedFactor : 0.65f);
            runDuration = RunningEvaluator.CalculateSingleRunDuration(runnerSpeed);
            runTimer = 0f;
            currentRunProgress = 0f;
            isDiving = false;

            startPosition = transform.position;
            destinationPosition = RunningTarget.GetCreasePosition(newDestination, role == RunnerRole.Striker);

            if (newDestination == CreaseEnd.NonStrikerEnd)
            {
                state = RunningState.RunningToNonStriker;
            }
            else
            {
                state = RunningState.RunningToStriker;
            }
        }

        public void TriggerDive()
        {
            if (state == RunningState.RunningToNonStriker || state == RunningState.RunningToStriker)
            {
                isDiving = true;
                currentSpeed *= (settings != null ? settings.diveSpeedMultiplier : 1.2f);
            }
        }

        public void ReturnToCrease()
        {
            if (state == RunningState.RunningToNonStriker || state == RunningState.RunningToStriker)
            {
                state = RunningState.Returning;
                // Swap target back to start
                CreaseEnd prevTarget = targetEnd;
                targetEnd = homeEnd;
                homeEnd = prevTarget;
                startPosition = transform.position;
                destinationPosition = RunningTarget.GetCreasePosition(targetEnd, role == RunnerRole.Striker);
                runTimer = 0f;
                currentRunProgress = 0f;
            }
        }

        public void CompleteRun()
        {
            state = RunningState.RunCompleted;
            homeEnd = targetEnd;
            targetEnd = (homeEnd == CreaseEnd.StrikerEnd) ? CreaseEnd.NonStrikerEnd : CreaseEnd.StrikerEnd;
            transform.position = destinationPosition;
            currentSpeed = 0f;
            isDiving = false;

            if (OnReachedCrease != null)
            {
                OnReachedCrease(this);
            }
        }

        public void MarkRunOut()
        {
            state = RunningState.RunOut;
            currentSpeed = 0f;
            if (OnRunOut != null)
            {
                OnRunOut(this);
            }
        }

        public void ResetToCrease()
        {
            state = RunningState.Idle;
            currentSpeed = 0f;
            isDiving = false;
            currentRunProgress = 0f;
            transform.position = RunningTarget.GetCreasePosition(homeEnd, role == RunnerRole.Striker);
        }

        public void UpdateMovement(float deltaTime)
        {
            if (state != RunningState.RunningToNonStriker && 
                state != RunningState.RunningToStriker && 
                state != RunningState.Returning)
            {
                return;
            }

            runTimer += deltaTime;
            currentRunProgress = Mathf.Clamp01(runTimer / runDuration);

            transform.position = Vector3.Lerp(startPosition, destinationPosition, currentRunProgress);

            if (currentRunProgress >= 1.0f || RunningTarget.IsPastCrease(transform.position, targetEnd))
            {
                CompleteRun();
            }
        }

        private void Update()
        {
            UpdateMovement(Time.deltaTime);
        }
    }
}
