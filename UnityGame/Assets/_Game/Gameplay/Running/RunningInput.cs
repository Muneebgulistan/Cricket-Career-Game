using System;
using UnityEngine;

namespace CricketGame.Gameplay.Running
{
    public class RunningInput : MonoBehaviour, IRunningInput
    {
        private bool isRunningRequested;
        private bool isDiveRequested;
        private bool isCancelRequested;

        public bool IsRunningRequested
        {
            get { return isRunningRequested; }
        }

        public bool IsDiveRequested
        {
            get { return isDiveRequested; }
        }

        public bool IsCancelRequested
        {
            get { return isCancelRequested; }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
            {
                RequestRun();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                RequestDive();
            }

            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Escape))
            {
                RequestCancel();
            }
        }

        public void RequestRun()
        {
            isRunningRequested = true;
        }

        public void RequestDive()
        {
            isDiveRequested = true;
        }

        public void RequestCancel()
        {
            isCancelRequested = true;
        }

        public void ConsumeRunRequest()
        {
            isRunningRequested = false;
        }

        public void ConsumeDiveRequest()
        {
            isDiveRequested = false;
        }

        public void ConsumeCancelRequest()
        {
            isCancelRequested = false;
        }

        public void ResetInput()
        {
            isRunningRequested = false;
            isDiveRequested = false;
            isCancelRequested = false;
        }
    }
}
