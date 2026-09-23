using System;
using UnityEngine;
using UnityEngine.UI;

namespace CricketGame.Gameplay.Running
{
    public class RunningTestHUD : MonoBehaviour
    {
        [SerializeField] private RunningManager runningManager;
        [SerializeField] private Text statusText;
        [SerializeField] private Button runButton;
        [SerializeField] private Button diveButton;
        [SerializeField] private Button cancelButton;

        private void Start()
        {
            if (runningManager == null)
            {
                runningManager = FindObjectOfType<RunningManager>();
            }

            if (runButton != null)
            {
                runButton.onClick.AddListener(OnRunClicked);
            }
            if (diveButton != null)
            {
                diveButton.onClick.AddListener(OnDiveClicked);
            }
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        public void OnRunClicked()
        {
            if (runningManager != null)
            {
                runningManager.AttemptRun(null, null);
            }
        }

        public void OnDiveClicked()
        {
            if (runningManager != null)
            {
                runningManager.TriggerDive(RunnerRole.Striker);
            }
        }

        public void OnCancelClicked()
        {
            if (runningManager != null)
            {
                runningManager.CancelOrReturn();
            }
        }

        private void Update()
        {
            if (statusText != null && runningManager != null)
            {
                statusText.text = string.Format("Runs: {0} | Active: {1} | Striker: {2} | NonStriker: {3}",
                    runningManager.CompletedRuns,
                    runningManager.IsRunActive,
                    runningManager.RuntimeData.strikerState,
                    runningManager.RuntimeData.nonStrikerState);
            }
        }
    }
}
