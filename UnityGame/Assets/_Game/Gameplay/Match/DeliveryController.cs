using System;
using UnityEngine;
using CricketGame.Gameplay.Ball;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Gameplay.Fielding;
using CricketGame.Gameplay.Running;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.Gameplay.Match
{
    public class DeliveryController : MonoBehaviour
    {
        [Header("Subsystem References")]
        [SerializeField] private BowlingController bowlingController;
        [SerializeField] private BattingController battingController;
        [SerializeField] private FieldingManager fieldingManager;
        [SerializeField] private RunningManager runningManager;
        [SerializeField] private ScoringManager scoringManager;

        [Header("Current Phase")]
        [SerializeField] private MatchPhase currentPhase = MatchPhase.Preparation;

        public MatchPhase CurrentPhase
        {
            get { return currentPhase; }
        }

        public event Action<MatchPhase> OnPhaseChanged;
        public event Action<UnifiedDeliveryResult> OnDeliveryResolved;

        public void Initialize(
            BowlingController bowling,
            BattingController batting,
            FieldingManager fielding,
            RunningManager running,
            ScoringManager scoring)
        {
            bowlingController = bowling;
            battingController = batting;
            fieldingManager = fielding;
            runningManager = running;
            scoringManager = scoring;
            SetPhase(MatchPhase.Preparation);
        }

        public void SetPhase(MatchPhase newPhase)
        {
            currentPhase = newPhase;
            if (OnPhaseChanged != null)
            {
                OnPhaseChanged(newPhase);
            }
        }

        public void StartDelivery()
        {
            SetPhase(MatchPhase.BowlerRunUp);
            if (runningManager != null)
            {
                runningManager.StartDeliveryPlay();
            }
        }

        public void OnBallReleased()
        {
            SetPhase(MatchPhase.BallInFlight);
        }

        public void OnBatSwung()
        {
            SetPhase(MatchPhase.BattingStrike);
        }

        public void OnBallHitOrMissed(bool wasHit)
        {
            if (wasHit)
            {
                SetPhase(MatchPhase.FieldingInterception);
            }
            else
            {
                // Ball passed bat to keeper/stumps
                SetPhase(MatchPhase.DeliveryResolution);
            }
        }

        public UnifiedDeliveryResult ResolveDelivery(
            BowlingResult bowlingResult, 
            BattingResult battingResult, 
            FieldingResult fieldingResult, 
            RunningResult runningResult,
            string strikerName)
        {
            SetPhase(MatchPhase.DeliveryResolution);

            UnifiedDeliveryResult res = new UnifiedDeliveryResult();
            res.dismissedPlayerName = strikerName;

            // 1. Check Bowling Result (e.g. Wide or Invalid)
            if (bowlingResult != null)
            {
                res.releaseSpeed = bowlingResult.speedKph;
                res.deliveryType = bowlingResult.deliveryType.ToString();

                if (!bowlingResult.wasValidDelivery)
                {
                    res.isLegalBall = false;
                    res.runsExtra = 1;
                    res.commentary = "Wide ball!";
                    PublishResult(res);
                    return res;
                }
            }

            // 2. Check Fielding Catch
            if (fieldingResult != null && fieldingResult.outcome == FieldingOutcome.Catch)
            {
                res.isWicket = true;
                res.wicketType = "Caught";
                res.fielderName = fieldingResult.fielderName;
                res.isLegalBall = true;
                res.commentary = string.Format("OUT! Caught cleanly by {0}!", fieldingResult.fielderName);
                PublishResult(res);
                return res;
            }

            // 3. Check Boundary (Four or Six)
            if (fieldingResult != null && (fieldingResult.outcome == FieldingOutcome.Four || fieldingResult.outcome == FieldingOutcome.Six))
            {
                res.isLegalBall = true;
                if (fieldingResult.outcome == FieldingOutcome.Six)
                {
                    res.runsBat = 6;
                    res.isBoundarySix = true;
                    res.commentary = "SIX! Driven way over the boundary!";
                }
                else
                {
                    res.runsBat = 4;
                    res.isBoundaryFour = true;
                    res.commentary = "FOUR! Pierces the gap to the rope!";
                }
                PublishResult(res);
                return res;
            }

            // 4. Check Batting Miss / Clean Bowled
            if (battingResult != null && battingResult.isMiss && (bowlingResult != null && bowlingResult.line == BowlingLine.OffStump && bowlingResult.length == BowlingLength.Yorker))
            {
                res.isWicket = true;
                res.wicketType = "Bowled";
                res.isLegalBall = true;
                res.commentary = "BOWLED HIM! Clean through the gate!";
                PublishResult(res);
                return res;
            }

            // 5. Check Running Result & Run Outs
            if (runningResult != null)
            {
                res.runsBat = runningResult.runsCompleted;
                res.isLegalBall = true;

                if (runningResult.wasRunOut)
                {
                    res.isWicket = true;
                    res.wicketType = "RunOut";
                    res.commentary = runningResult.summary;
                }
                else if (res.runsBat == 0)
                {
                    res.commentary = "Dot ball, fielded safely.";
                }
                else
                {
                    res.commentary = string.Format("{0} run(s) completed between wickets.", res.runsBat);
                }

                PublishResult(res);
                return res;
            }

            // Default Dot Ball
            res = UnifiedDeliveryResult.Dot();
            PublishResult(res);
            return res;
        }

        private void PublishResult(UnifiedDeliveryResult res)
        {
            if (scoringManager != null)
            {
                scoringManager.ProcessDelivery(res);
            }

            if (OnDeliveryResolved != null)
            {
                OnDeliveryResolved(res);
            }
        }
    }
}
