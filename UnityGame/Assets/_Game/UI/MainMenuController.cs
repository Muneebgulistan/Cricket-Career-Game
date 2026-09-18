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
            bool hasSave = CareerManager.Instance != null && CareerManager.Instance.HasActiveCareer;
            if (continueCareerButton != null)
            {
                continueCareerButton.interactable = hasSave;
            }
        }

        public void OnNewCareerClicked()
        {
            Debug.Log("[MainMenuController] Starting New Career...");
            
            // Create default Under-16 prodigy
            PlayerProfile defaultPlayer = new PlayerProfile(
                "Muneeb Gulistan", 
                16, 
                "Pakistan", 
                PlayingRole.Batsman, 
                BattingStyle.RightHand, 
                BowlingStyle.RightArmFast
            );

            CareerManager.Instance?.CreateNewCareer(defaultPlayer);
            GameStateManager.Instance?.ChangeState(GameState.CareerHub);
            SceneController.Instance?.LoadScene(SceneController.SceneCareerHub);
        }

        public void OnContinueCareerClicked()
        {
            Debug.Log("[MainMenuController] Continuing Career...");
            if (CareerManager.Instance != null && CareerManager.Instance.HasActiveCareer)
            {
                GameStateManager.Instance?.ChangeState(GameState.CareerHub);
                SceneController.Instance?.LoadScene(SceneController.SceneCareerHub);
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
                Debug.Log("[MainMenuController] Settings opened (Audio/Graphic controls toggle).");
            }
        }

        public void OnExitClicked()
        {
            Debug.Log("[MainMenuController] Exiting Game...");
            Application.Quit();
        }
    }
}
