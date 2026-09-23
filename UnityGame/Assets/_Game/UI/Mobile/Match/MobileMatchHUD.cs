using System;
using UnityEngine;
using CricketGame.Gameplay.Match;
using CricketGame.Gameplay.Scoring;
using CricketGame.UI.Mobile.Batting;
using CricketGame.UI.Mobile.Bowling;
using CricketGame.UI.Mobile.Running;
using CricketGame.UI.Mobile.Fielding;
using CricketGame.Camera;

namespace CricketGame.UI.Mobile.Match
{
    public class MobileMatchHUD : MonoBehaviour
    {
        [Header("Master Overlay Containers")]
        [SerializeField] private MobileScoreboard scoreboard;
        [SerializeField] private MobilePlayerInfo playerInfo;
        [SerializeField] private MobileOverDisplay overDisplay;
        [SerializeField] private MobileTargetDisplay targetDisplay;
        [SerializeField] private MobilePauseMenu pauseMenu;
        [SerializeField] private MobileMatchResult matchResult;

        [Header("Phase Control HUDs")]
        [SerializeField] private MobileBattingHUD battingHUD;
        [SerializeField] private MobileBowlingHUD bowlingHUD;
        [SerializeField] private MobileRunningHUD runningHUD;
        [SerializeField] private MobileFieldingHUD fieldingHUD;

        [Header("Engine References")]
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private DeliveryController deliveryController;
        [SerializeField] private ScoringManager scoringManager;

        public MobileScoreboard Scoreboard { get { return scoreboard; } }
        public MobileBattingHUD BattingHUD { get { return battingHUD; } }
        public MobileBowlingHUD BowlingHUD { get { return bowlingHUD; } }
        public MobileRunningHUD RunningHUD { get { return runningHUD; } }
        public MobileFieldingHUD FieldingHUD { get { return fieldingHUD; } }
        public MobilePauseMenu PauseMenu { get { return pauseMenu; } }
        public MobileMatchResult MatchResult { get { return matchResult; } }

        private void Awake()
        {
            FindEngineDependencies();
        }

        private void Start()
        {
            SubscribeEngineEvents();
            UpdateFullDisplay();
        }

        private void OnDestroy()
        {
            UnsubscribeEngineEvents();
        }

        private void FindEngineDependencies()
        {
            if (matchManager == null) matchManager = FindObjectOfType<MatchManager>();
            if (deliveryController == null) deliveryController = FindObjectOfType<DeliveryController>();
            if (scoringManager == null) scoringManager = FindObjectOfType<ScoringManager>();
        }

        private void SubscribeEngineEvents()
        {
            if (deliveryController != null)
            {
                deliveryController.OnPhaseChanged += HandlePhaseChanged;
                deliveryController.OnDeliveryResolved += HandleDeliveryResolved;
            }

            if (matchManager != null)
            {
                matchManager.OnMatchCompleted += HandleMatchCompleted;
            }
        }

        private void UnsubscribeEngineEvents()
        {
            if (deliveryController != null)
            {
                deliveryController.OnPhaseChanged -= HandlePhaseChanged;
                deliveryController.OnDeliveryResolved -= HandleDeliveryResolved;
            }

            if (matchManager != null)
            {
                matchManager.OnMatchCompleted -= HandleMatchCompleted;
            }
        }

        public void HandlePhaseChanged(MatchPhase phase)
        {
            // 1. Camera transition
            if (CricketCameraManager.Instance != null)
            {
                CricketCameraManager.Instance.HandleMatchPhaseCameraTransition(phase);
            }

            // 2. Mobile control layer activation
            switch (phase)
            {
                case MatchPhase.Preparation:
                    SetHUDVisibility(true, false, false, false);
                    break;
                case MatchPhase.BowlerRunUp:
                    SetHUDVisibility(true, true, false, false);
                    break;
                case MatchPhase.BallInFlight:
                case MatchPhase.BattingStrike:
                    SetHUDVisibility(true, false, false, false);
                    break;
                case MatchPhase.FieldingInterception:
                    SetHUDVisibility(false, false, true, true);
                    break;
                case MatchPhase.RunningWickets:
                    SetHUDVisibility(false, false, true, false);
                    break;
                case MatchPhase.DeliveryResolution:
                    SetHUDVisibility(false, false, false, false);
                    break;
            }

            UpdateFullDisplay();
        }

        public void SetHUDVisibility(bool showBatting, bool showBowling, bool showRunning, bool showFielding)
        {
            if (battingHUD != null) battingHUD.SetVisible(showBatting);
            if (bowlingHUD != null) bowlingHUD.SetVisible(showBowling);
            if (runningHUD != null) runningHUD.SetVisible(showRunning);
            if (fieldingHUD != null) fieldingHUD.SetVisible(showFielding);
        }

        public void HandleDeliveryResolved(UnifiedDeliveryResult result)
        {
            if (overDisplay != null && result != null)
            {
                string outcome = result.isWicket ? "W" : result.TotalRuns.ToString();
                overDisplay.AddBallOutcome(outcome);
            }

            UpdateFullDisplay();
        }

        public void HandleMatchCompleted(CricketGame.Cricket.MatchResult result)
        {
            SetHUDVisibility(false, false, false, false);
            if (matchResult != null)
            {
                matchResult.ShowResult(result);
            }
        }

        public void UpdateFullDisplay()
        {
            if (scoringManager == null) return;

            InningsScore inn = scoringManager.CurrentInnings;
            if (inn == null) return;

            string battingTeam = inn.battingTeam;
            float ov = inn.OversFloat;
            float maxOv = inn.maxOvers;
            float crr = inn.CurrentRunRate;

            int target = inn.targetScore;
            float rrr = inn.RequiredRunRate;
            bool inPowerplay = inn.CompletedOvers < 6;

            if (scoreboard != null)
            {
                scoreboard.UpdateScore(battingTeam, inn.totalRuns, inn.wickets, ov, maxOv, crr, target, rrr, inPowerplay);
            }

            if (playerInfo != null)
            {
                BatterScore striker = scoringManager.CurrentStriker;
                if (striker != null)
                {
                    playerInfo.UpdateStriker(striker.playerName, striker.runs, striker.balls, striker.fours, striker.sixes, striker.StrikeRate);
                }

                BatterScore nonStriker = scoringManager.CurrentNonStriker;
                if (nonStriker != null)
                {
                    playerInfo.UpdateNonStriker(nonStriker.playerName, nonStriker.runs, nonStriker.balls);
                }

                BowlerFigures bowler = scoringManager.CurrentBowler;
                if (bowler != null)
                {
                    playerInfo.UpdateBowler(bowler.playerName, bowler.FiguresFormatted, bowler.Economy);
                }
            }

            if (targetDisplay != null)
            {
                if (inn.targetScore > 0)
                {
                    targetDisplay.UpdateTargetEquation(battingTeam, inn.RunsNeeded, inn.RemainingBalls);
                }
                else
                {
                    targetDisplay.Hide();
                }
            }
        }
    }
}
