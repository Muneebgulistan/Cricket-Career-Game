using System;
using UnityEngine;
using CricketGame.Core;
using CricketGame.Career;
using CricketGame.Players;
using CricketGame.SaveSystem;

namespace CricketGame.UI
{
    public enum AppFlowState
    {
        MainMenu,
        PlayerCreation,
        CareerHub,
        MatchLoading,
        InMatch,
        MatchResult,
        Settings,
        Quit
    }

    /// <summary>
    /// Master application flow orchestrator coordinating user journey from
    /// app launch through Main Menu, Player Creation, Career Hub, and Match transitions.
    /// </summary>
    public class AppFlowManager : MonoBehaviour
    {
        public static AppFlowManager Instance { get; private set; }

        [Header("State Tracking")]
        [SerializeField] private AppFlowState currentState = AppFlowState.MainMenu;

        [Header("Panel References (Stack Driven)")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject playerCreationPanel;
        [SerializeField] private GameObject careerHubPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Subscreen Controllers")]
        [SerializeField] private PlayerCreationScreen creationScreen;
        [SerializeField] private CareerHubDashboard hubDashboard;

        public AppFlowState CurrentState { get { return currentState; } }
        public PlayerCreationScreen CreationScreen { get { return creationScreen; } set { creationScreen = value; } }
        public CareerHubDashboard HubDashboard { get { return hubDashboard; } set { hubDashboard = value; } }

        public GameObject MainMenuPanel { get { return mainMenuPanel; } set { mainMenuPanel = value; } }
        public GameObject PlayerCreationPanel { get { return playerCreationPanel; } set { playerCreationPanel = value; } }
        public GameObject CareerHubPanel { get { return careerHubPanel; } set { careerHubPanel = value; } }
        public GameObject SettingsPanel { get { return settingsPanel; } set { settingsPanel = value; } }

        public event Action<AppFlowState, AppFlowState> OnStateChanged;

        public static void SetInstanceForTesting(AppFlowManager inst)
        {
            Instance = inst;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            InitializeFlow();
        }

        public void InitializeFlow()
        {
            TransitionTo(AppFlowState.MainMenu);
        }

        public void TransitionTo(AppFlowState newState)
        {
            AppFlowState previousState = currentState;
            currentState = newState;

            switch (newState)
            {
                case AppFlowState.MainMenu:
                    if (UIManager.Instance != null && mainMenuPanel != null)
                    {
                        UIManager.Instance.ClearStack();
                        UIManager.Instance.PushPanel(mainMenuPanel);
                    }
                    if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.MainMenu);
                    break;

                case AppFlowState.PlayerCreation:
                    if (UIManager.Instance != null && playerCreationPanel != null)
                    {
                        UIManager.Instance.PushPanel(playerCreationPanel);
                    }
                    break;

                case AppFlowState.CareerHub:
                    if (UIManager.Instance != null && careerHubPanel != null)
                    {
                        UIManager.Instance.ClearStack();
                        UIManager.Instance.PushPanel(careerHubPanel);
                    }
                    if (hubDashboard != null)
                    {
                        hubDashboard.RefreshDashboard();
                    }
                    if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.CareerHub);
                    break;

                case AppFlowState.Settings:
                    if (UIManager.Instance != null && settingsPanel != null)
                    {
                        UIManager.Instance.PushPanel(settingsPanel);
                    }
                    break;

                case AppFlowState.MatchLoading:
                    if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.PlayingMatch);
                    break;

                case AppFlowState.InMatch:
                    if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.PlayingMatch);
                    break;

                case AppFlowState.MatchResult:
                    if (GameStateManager.Instance != null) GameStateManager.Instance.ChangeState(GameState.CareerMatchResult);
                    break;

                case AppFlowState.Quit:
                    Application.Quit();
                    break;
            }

            if (OnStateChanged != null)
            {
                OnStateChanged(previousState, newState);
            }
        }

        // ==========================================
        // FLOW ACTIONS
        // ==========================================

        public void StartNewCareer()
        {
            TransitionTo(AppFlowState.PlayerCreation);
        }

        public bool LoadCareer(string slotName = null)
        {
            CareerProfile profile = null;

            if (SaveProfileManager.Instance != null)
            {
                profile = SaveProfileManager.Instance.LoadProfile(slotName);
            }
            else if (SaveManager.Instance != null)
            {
                profile = SaveManager.Instance.LoadCareer();
            }

            if (profile != null)
            {
                if (CareerManager.Instance != null)
                {
                    CareerManager.Instance.SetActiveCareerForTesting(profile);
                }

                if (hubDashboard != null)
                {
                    hubDashboard.BindProfile(profile);
                }

                TransitionTo(AppFlowState.CareerHub);
                return true;
            }

            CricketGame.Core.CricketLogger.LogWarning("[AppFlowManager] No saved career found to load.");
            return false;
        }

        public void OnPlayerCreationComplete(PlayerProfile player)
        {
            CareerProfile profile = null;
            if (SaveProfileManager.Instance != null && SaveProfileManager.Instance.CurrentProfile != null)
            {
                profile = SaveProfileManager.Instance.CurrentProfile;
            }
            else if (CareerManager.Instance != null && CareerManager.Instance.ActiveCareer != null)
            {
                profile = CareerManager.Instance.ActiveCareer;
            }
            else
            {
                profile = new CareerProfile(player);
                if (SaveProfileManager.Instance != null)
                {
                    SaveProfileManager.Instance.SaveProfile(profile);
                }
            }

            if (hubDashboard != null)
            {
                hubDashboard.BindProfile(profile);
            }

            TransitionTo(AppFlowState.CareerHub);
        }

        public void LaunchMatch(string sceneName = "SceneMatch")
        {
            TransitionTo(AppFlowState.MatchLoading);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.LoadSceneAsync(sceneName, () =>
                {
                    TransitionTo(AppFlowState.InMatch);
                });
            }
            else
            {
                TransitionTo(AppFlowState.InMatch);
            }
        }

        public void ReturnToCareerHub()
        {
            TransitionTo(AppFlowState.CareerHub);
        }

        public void OpenSettings()
        {
            TransitionTo(AppFlowState.Settings);
        }

        public void CloseSettings()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.PopPanel();
            }
            currentState = AppFlowState.MainMenu;
        }

        public void QuitGame()
        {
            TransitionTo(AppFlowState.Quit);
        }
    }
}
