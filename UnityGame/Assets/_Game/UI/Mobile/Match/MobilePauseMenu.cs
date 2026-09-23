using System;
using UnityEngine;
using UnityEngine.UI;
using CricketGame.Camera;
using CricketGame.Gameplay.Match;

namespace CricketGame.UI.Mobile.Match
{
    public class MobilePauseMenu : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject controlsHelpPanel;

        [Header("Buttons")]
        [SerializeField] private Button pauseTriggerButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button closeHelpButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitToHubButton;

        [Header("Camera Switching Buttons")]
        [SerializeField] private Button camBroadcastButton;
        [SerializeField] private Button camBattingButton;
        [SerializeField] private Button camBowlingButton;
        [SerializeField] private Button camFieldButton;

        private bool isPaused = false;
        public bool IsPaused { get { return isPaused; } }

        public event Action OnResumeRequested;
        public event Action OnRestartRequested;
        public event Action OnExitRequested;

        private void Awake()
        {
            SetupListeners();
            if (menuPanel != null) menuPanel.SetActive(false);
            if (controlsHelpPanel != null) controlsHelpPanel.SetActive(false);
        }

        private void SetupListeners()
        {
            if (pauseTriggerButton != null) pauseTriggerButton.onClick.AddListener(OpenPauseMenu);
            if (resumeButton != null) resumeButton.onClick.AddListener(ClosePauseMenu);
            if (controlsButton != null) controlsButton.onClick.AddListener(ToggleControlsHelp);
            if (closeHelpButton != null) closeHelpButton.onClick.AddListener(CloseControlsHelp);
            if (restartButton != null) restartButton.onClick.AddListener(HandleRestart);
            if (exitToHubButton != null) exitToHubButton.onClick.AddListener(HandleExit);

            if (camBroadcastButton != null) camBroadcastButton.onClick.AddListener(delegate { SwitchCam(CricketCameraMode.BroadcastCamera); });
            if (camBattingButton != null) camBattingButton.onClick.AddListener(delegate { SwitchCam(CricketCameraMode.BattingCamera); });
            if (camBowlingButton != null) camBowlingButton.onClick.AddListener(delegate { SwitchCam(CricketCameraMode.BowlingCamera); });
            if (camFieldButton != null) camFieldButton.onClick.AddListener(delegate { SwitchCam(CricketCameraMode.FieldCamera); });
        }

        public void OpenPauseMenu()
        {
            isPaused = true;
            if (menuPanel != null) menuPanel.SetActive(true);
            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.PauseMatch();
            }
        }

        public void ClosePauseMenu()
        {
            isPaused = false;
            if (menuPanel != null) menuPanel.SetActive(false);
            if (controlsHelpPanel != null) controlsHelpPanel.SetActive(false);
            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.ResumeMatch();
            }
            if (OnResumeRequested != null)
            {
                OnResumeRequested();
            }
        }

        public void ToggleControlsHelp()
        {
            if (controlsHelpPanel != null)
            {
                controlsHelpPanel.SetActive(!controlsHelpPanel.activeSelf);
            }
        }

        public void CloseControlsHelp()
        {
            if (controlsHelpPanel != null)
            {
                controlsHelpPanel.SetActive(false);
            }
        }

        private void SwitchCam(CricketCameraMode mode)
        {
            if (CricketCameraManager.Instance != null)
            {
                CricketCameraManager.Instance.SwitchCameraMode(mode);
            }
        }

        private void HandleRestart()
        {
            if (OnRestartRequested != null)
            {
                OnRestartRequested();
            }
        }

        private void HandleExit()
        {
            if (OnExitRequested != null)
            {
                OnExitRequested();
            }
        }
    }
}
