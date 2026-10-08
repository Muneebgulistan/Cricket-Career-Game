using UnityEngine;
using UnityEngine.UI;
using CricketGame.Core;
using CricketGame.Career;
using CricketGame.Players;
using CricketGame.Cricket;

namespace CricketGame.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button newCareerButton;
        [SerializeField] private Button continueCareerButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            SetupListeners();
            RefreshButtonStates();
        }

        private void SetupListeners()
        {
            if (newCareerButton != null)
                newCareerButton.onClick.AddListener(OnNewCareerClicked);

            if (continueCareerButton != null)
                continueCareerButton.onClick.AddListener(OnContinueCareerClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        private void RefreshButtonStates()
        {
            bool hasSave = (CareerManager.Instance != null && CareerManager.Instance.HasActiveCareer) ||
                           (CricketGame.SaveSystem.SaveProfileManager.Instance != null && CricketGame.SaveSystem.SaveProfileManager.Instance.HasProfile());
            if (continueCareerButton != null)
            {
                continueCareerButton.interactable = hasSave;
            }
        }

        public void OnNewCareerClicked()
        {
            CricketGame.Core.CricketLogger.Log("[MainMenuController] Starting New Career...");
            if (AppFlowManager.Instance != null)
            {
                AppFlowManager.Instance.StartNewCareer();
                return;
            }
            
            // Fallback default Under-16 prodigy
            PlayerProfile defaultPlayer = new PlayerProfile(
                "Muneeb Gulistan", 
                16, 
                "Pakistan", 
                PlayingRole.Batsman, 
                BattingStyle.RightHand, 
                BowlingStyle.RightArmFast
            );

            if (CareerManager.Instance != null) CareerManager.Instance.CreateNewCareer(defaultPlayer);
            if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.CareerHub);
            if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneCareerHub);
        }

        public void OnContinueCareerClicked()
        {
            CricketGame.Core.CricketLogger.Log("[MainMenuController] Continuing Career...");
            if (AppFlowManager.Instance != null)
            {
                if (AppFlowManager.Instance.LoadCareer()) return;
            }

            if (CareerManager.Instance != null && CareerManager.Instance.HasActiveCareer)
            {
                if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.CareerHub);
                if (SceneController.Instance != null) SceneController.Instance.LoadScene(SceneController.SceneCareerHub);
            }
        }

        public void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
            else
            {
                CricketGame.Core.CricketLogger.Log("[MainMenuController] Settings opened (Audio/Graphic controls toggle).");
            }
        }

        public void OnExitClicked()
        {
            CricketGame.Core.CricketLogger.Log("[MainMenuController] Exiting Game...");
            Application.Quit();
        }
    }
}
