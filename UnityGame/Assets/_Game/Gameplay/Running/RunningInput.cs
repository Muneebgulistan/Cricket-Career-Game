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
            if (UnityEngine.Input.GetKeyDown(KeyCode.R) || UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                RequestRun();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                RequestDive();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.C) || UnityEngine.Input.GetKeyDown(KeyCode.Escape))
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
