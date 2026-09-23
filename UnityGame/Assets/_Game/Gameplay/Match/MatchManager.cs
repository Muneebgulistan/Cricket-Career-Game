using System;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Gameplay.Scoring;
using CricketGame.Gameplay.Running;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Gameplay.Fielding;

namespace CricketGame.Gameplay.Match
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Header("Match Settings")]
        [SerializeField] private MatchSettings settings = MatchSettings.DefaultT20();

        [Header("Teams Setup")]
        [SerializeField] private string homeTeamName = "Lahore Eagles U-16";
        [SerializeField] private string awayTeamName = "Karachi Kings U-16";

        [Header("Controllers")]
        [SerializeField] private DeliveryController deliveryController;
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private RunningManager runningManager;
        [SerializeField] private BattingController battingController;
        [SerializeField] private BowlingController bowlingController;
        [SerializeField] private FieldingManager fieldingManager;

        private MatchController matchController;

        public MatchController Controller { get { return matchController; } }
        public MatchSettings Settings { get { return settings; } }

        public event Action<MatchState> OnMatchStateChanged;
        public event Action<CricketGame.Cricket.MatchResult> OnMatchCompleted;

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

            if (scoringManager == null)
            {
                scoringManager = FindObjectOfType<ScoringManager>();
            }

            matchController = new MatchController(settings, scoringManager);
            matchController.OnMatchStateChanged += HandleMatchStateChanged;
            matchController.OnMatchCompleted += HandleMatchCompleted;
        }

        private void Start()
        {
            if (deliveryController != null)
            {
                deliveryController.Initialize(bowlingController, battingController, fieldingManager, runningManager, scoringManager);
            }

            matchController.SetupMatch(homeTeamName, awayTeamName);
            matchController.PerformToss(TossChoice.Heads, TossDecision.Bat);
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            if (OnMatchStateChanged != null)
            {
                OnMatchStateChanged(state);
            }
        }

        private void HandleMatchCompleted(CricketGame.Cricket.MatchResult result)
        {
            if (OnMatchCompleted != null)
            {
                OnMatchCompleted(result);
            }
        }

        public void PauseMatch()
        {
            if (matchController != null)
            {
                matchController.PauseMatch();
            }
        }

        public void ResumeMatch()
        {
            if (matchController != null)
            {
                matchController.ResumeMatch();
            }
        }

        public void NextDelivery()
        {
            if (deliveryController != null)
            {
                deliveryController.StartDelivery();
            }
        }
    }
}
