using System;
using UnityEngine;
using CricketGame.AI;
using CricketGame.Audio;
using CricketGame.Camera;
using CricketGame.Cricket;
using CricketGame.Gameplay.Ball;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Gameplay.Fielding;
using CricketGame.Gameplay.Running;
using CricketGame.Gameplay.Scoring;
using CricketGame.Players;

namespace CricketGame.Gameplay.Match
{
    public class MatchOrchestrator : MonoBehaviour
    {
        public static MatchOrchestrator Instance { get; private set; }

        [Header("Subsystems")]
        [SerializeField] private DeliveryController deliveryController;
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private PlayerTurnController turnController;
        [SerializeField] private BowlingController bowlingController;
        [SerializeField] private BattingController battingController;
        [SerializeField] private FieldingManager fieldingManager;
        [SerializeField] private RunningManager runningManager;
        [SerializeField] private SimpleCricketBall activeBall;

        [Header("Settings & Difficulty")]
        [SerializeField] private AIDifficulty matchDifficulty = AIDifficulty.Normal;

        private MatchController matchController;
        private bool isDeliveryInProgress = false;
        private BowlingDelivery currentDelivery;
        private BowlingReleaseData currentReleaseData;

        public AIDifficulty MatchDifficulty
        {
            get { return matchDifficulty; }
            set 
            { 
                matchDifficulty = value;
                if (turnController != null) turnController.Difficulty = value;
            }
        }

        public bool IsDeliveryInProgress
        {
            get { return isDeliveryInProgress; }
        }

        public event Action<UnifiedDeliveryResult> OnDeliveryCycleCompleted;

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

            if (turnController == null) turnController = GetComponent<PlayerTurnController>();
            if (scoringManager == null) scoringManager = FindFirstObjectByType<ScoringManager>();
            if (deliveryController == null) deliveryController = FindFirstObjectByType<DeliveryController>();
            if (bowlingController == null) bowlingController = FindFirstObjectByType<BowlingController>();
            if (battingController == null) battingController = FindFirstObjectByType<BattingController>();
            if (fieldingManager == null) fieldingManager = FindFirstObjectByType<FieldingManager>();
            if (runningManager == null) runningManager = FindFirstObjectByType<RunningManager>();
            if (activeBall == null) activeBall = FindFirstObjectByType<SimpleCricketBall>();
        }

        public void Initialize(
            MatchController controller,
            ScoringManager scoring,
            DeliveryController delivery,
            PlayerTurnController turn,
            BowlingController bowling,
            BattingController batting,
            FieldingManager fielding,
            RunningManager running,
            SimpleCricketBall ball)
        {
            matchController = controller;
            scoringManager = scoring;
            deliveryController = delivery;
            turnController = turn;
            bowlingController = bowling;
            battingController = batting;
            fieldingManager = fielding;
            runningManager = running;
            activeBall = ball;
        }

        public UnifiedDeliveryResult ExecuteDeliveryCycleDeterministic(
            BowlingDelivery delivery,
            BattingResult battingResultOverride = null,
            FieldingResult fieldingResultOverride = null,
            RunningResult runningResultOverride = null,
            PlayerProfile strikerProfile = null,
            PlayerProfile bowlerProfile = null,
            int seed = 12345)
        {
            isDeliveryInProgress = true;
            currentDelivery = delivery != null ? delivery : BowlingDelivery.CreateDefault(BowlingBaseType.Fast, BowlingLength.GoodLength, BowlingLine.OffStump);

            // 1. Calculate release data
            Vector3 releasePoint = new Vector3(0f, 2.1f, -10.5f);
            BowlingSettings bowlSettings = new BowlingSettings();
            currentReleaseData = BowlingEvaluator.CalculateReleaseData(currentDelivery, releasePoint, bowlerProfile, bowlSettings, 0.85f, seed);

            // 2. Bowling Result calculation
            Vector3 actualBounce = currentReleaseData.intendedPitchPoint;
            BowlingResult bowlingResult = BowlingEvaluator.EvaluateResult(currentDelivery, currentReleaseData, actualBounce, true, true);

            // 3. Batting Result evaluation
            BattingResult battingResult = battingResultOverride;
            if (battingResult == null)
            {
                if (turnController != null)
                {
                    AIMatchContext ctx = AIMatchContext.CreateDefault();
                    ctx.seed = seed;
                    ctx.difficulty = matchDifficulty;
                    battingResult = turnController.ExecuteAIBatting(currentDelivery, ctx, strikerProfile, BattingSettings.CreateDefault(), Vector3.forward);
                }
                else
                {
                    OpponentBatsmanAI batsmanAI = new OpponentBatsmanAI(matchDifficulty);
                    AIMatchContext ctx = AIMatchContext.CreateDefault();
                    ctx.seed = seed;
                    battingResult = batsmanAI.SimulateShotResult(currentDelivery, ctx, strikerProfile, BattingSettings.CreateDefault(), Vector3.forward);
                }
            }

            // 4. Fielding Result evaluation
            FieldingResult fieldingResult = fieldingResultOverride;
            if (fieldingResult == null)
            {
                if (battingResult.isMiss)
                {
                    fieldingResult = FieldingResult.CreateGroundField("WicketKeeper", FieldingPosition.WicketKeeper, true, 0);
                }
                else if (battingResult.isEdge)
                {
                    // High edge -> catch opportunity
                    fieldingResult = (battingResult.launchAngle > 20f) 
                        ? FieldingResult.CreateCatch("WicketKeeper", FieldingPosition.WicketKeeper, true) 
                        : FieldingResult.CreateGroundField("Slips", FieldingPosition.Slip1, true, 1);
                }
                else if (battingResult.estimatedRuns == 6)
                {
                    fieldingResult = FieldingResult.CreateBoundary(true, 6);
                }
                else if (battingResult.estimatedRuns == 4)
                {
                    fieldingResult = FieldingResult.CreateBoundary(false, 4);
                }
                else
                {
                    fieldingResult = FieldingResult.CreateGroundField("Cover Point", FieldingPosition.Point, true, battingResult.estimatedRuns);
                }
            }

            // 5. Running Result evaluation
            RunningResult runningResult = runningResultOverride;
            if (runningResult == null)
            {
                if (fieldingResult.outcome == FieldingOutcome.Catch)
                {
                    runningResult = new RunningResult(0, false, RunnerRole.Striker, CreaseEnd.StrikerEnd, 10f, "Caught out, no runs taken");
                }
                else if (fieldingResult.outcome == FieldingOutcome.Four || fieldingResult.outcome == FieldingOutcome.Six)
                {
                    runningResult = RunningResult.Success(battingResult.estimatedRuns, 50f);
                }
                else
                {
                    int runs = Mathf.Clamp(battingResult.estimatedRuns, 0, 3);
                    runningResult = RunningResult.Success(runs, 10f);
                }
            }

            // 6. Resolve delivery via DeliveryController
            string strikerName = strikerProfile != null ? strikerProfile.name : "Batsman";
            UnifiedDeliveryResult unifiedResult = null;

            if (deliveryController != null)
            {
                unifiedResult = deliveryController.ResolveDelivery(bowlingResult, battingResult, fieldingResult, runningResult, strikerName);
            }
            else
            {
                // Fallback direct resolver if deliveryController not linked
                unifiedResult = DirectResolveDelivery(bowlingResult, battingResult, fieldingResult, runningResult, strikerName);
            }

            // 7. Emit Audio / Visual Events
            EmitGameplayEvents(unifiedResult, battingResult, fieldingResult);

            // 8. Record to MatchController if available
            if (matchController != null)
            {
                matchController.RecordDeliveryResult(unifiedResult);
            }

            isDeliveryInProgress = false;

            if (OnDeliveryCycleCompleted != null)
            {
                OnDeliveryCycleCompleted(unifiedResult);
            }

            return unifiedResult;
        }

        private UnifiedDeliveryResult DirectResolveDelivery(
            BowlingResult bowlingResult, 
            BattingResult battingResult, 
            FieldingResult fieldingResult, 
            RunningResult runningResult,
            string strikerName)
        {
            if (fieldingResult != null && fieldingResult.outcome == FieldingOutcome.Catch)
            {
                return UnifiedDeliveryResult.Wicket("Caught", strikerName, fieldingResult.fielderName);
            }
            if (fieldingResult != null && fieldingResult.outcome == FieldingOutcome.Six)
            {
                return UnifiedDeliveryResult.Runs(6);
            }
            if (fieldingResult != null && fieldingResult.outcome == FieldingOutcome.Four)
            {
                return UnifiedDeliveryResult.Runs(4);
            }
            if (battingResult != null && battingResult.isMiss && bowlingResult != null && bowlingResult.line == BowlingLine.OffStump && bowlingResult.length == BowlingLength.Yorker)
            {
                return UnifiedDeliveryResult.Wicket("Bowled", strikerName, string.Empty);
            }
            if (runningResult != null)
            {
                if (runningResult.wasRunOut)
                {
                    return UnifiedDeliveryResult.Wicket("RunOut", strikerName, string.Empty);
                }
                return UnifiedDeliveryResult.Runs(runningResult.runsCompleted);
            }
            return UnifiedDeliveryResult.Dot();
        }

        private void EmitGameplayEvents(UnifiedDeliveryResult res, BattingResult batRes, FieldingResult fieldRes)
        {
            if (res == null) return;

            if (batRes != null && !batRes.isMiss)
            {
                GameplayAudioEvents.TriggerBatContact();
            }

            if (res.isBoundarySix)
            {
                GameplayAudioEvents.TriggerBoundarySix();
            }
            else if (res.isBoundaryFour)
            {
                GameplayAudioEvents.TriggerBoundaryFour();
            }
            else if (res.isWicket)
            {
                if (string.Equals(res.wicketType, "Caught", StringComparison.OrdinalIgnoreCase))
                {
                    GameplayAudioEvents.TriggerCatch();
                }
                else if (string.Equals(res.wicketType, "RunOut", StringComparison.OrdinalIgnoreCase))
                {
                    GameplayAudioEvents.TriggerRunOut();
                }
                else
                {
                    GameplayAudioEvents.TriggerWicketFall();
                }
            }
            else if (res.TotalRuns == 0)
            {
                GameplayAudioEvents.TriggerDotBall();
            }
        }
    }
}
