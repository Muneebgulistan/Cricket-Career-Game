#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using CricketGame.AI;
using CricketGame.Core;
using CricketGame.Cricket;
using CricketGame.Gameplay.Ball;
using CricketGame.Gameplay.Batting;
using CricketGame.Gameplay.Bowling;
using CricketGame.Gameplay.Match;
using CricketGame.Gameplay.Scoring;
using CricketGame.Players;

namespace CricketGame.Testing
{
    public sealed class StressTestReport
    {
        public bool Passed;
        public int MatchesSimulated;
        public int TotalDeliveriesSimulated;
        public int Innings1Runs;
        public int Innings1Wickets;
        public int Innings2Runs;
        public int Innings2Wickets;
        public int StateTransitions;
        public long RetainedManagedBytes;
        public double ExecutionTimeMs;
        public bool PhysicsProbePassed;
        public bool MatchStateMachinePassed;
        public bool MemoryProbePassed;
        public string Summary;
    }

    /// <summary>
    /// Runs full, scored T20 matches through the production scoring and match
    /// controllers, plus a 100x custom-ball ground-collision probe. Intended for
    /// Editor QA in a clean, non-playing scene.
    /// </summary>
    public static class MatchSimulationStressTest
    {
        private const int RepeatedMatchCount = 3;
        private const long MaxRetainedManagedBytes = 4L * 1024L * 1024L;

        private sealed class MatchRun
        {
            public int Deliveries;
            public int FirstRuns;
            public int FirstWickets;
            public int SecondRuns;
            public int SecondWickets;
            public int StateTransitions;
            public bool Finished;
        }

        public static StressTestReport RunFullT20StressTest(float timeScaleMultiplier = 100f)
        {
            StressTestReport report = new StressTestReport();
            float originalTimeScale = Time.timeScale;
            UnityEngine.Random.State originalRandomState = UnityEngine.Random.state;
            Stopwatch stopwatch = Stopwatch.StartNew();
            long retainedBytes = 0;
            bool noLeaks = true;
            bool statesPassed = true;
            bool physicsPassed = false;

            try
            {
                if (Application.isPlaying)
                    throw new InvalidOperationException("Run the release-candidate stress suite from Edit Mode, not during gameplay.");
                if (timeScaleMultiplier < 100f)
                    throw new ArgumentOutOfRangeException("timeScaleMultiplier", "The stress run must use at least 100x time scale.");

                UnityEngine.Random.InitState(25027);
                Time.timeScale = timeScaleMultiplier;
                physicsPassed = RunBallTunnelingProbe(timeScaleMultiplier);

                // One warm-up prevents first-use JIT and AI singleton setup from
                // being counted as retained match memory.
                RunSingleT20(0);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                long memoryBefore = GC.GetTotalMemory(true);

                report.MatchesSimulated = RepeatedMatchCount;
                for (int i = 0; i < RepeatedMatchCount; i++)
                {
                    MatchRun run = RunSingleT20(i + 1);
                    report.TotalDeliveriesSimulated += run.Deliveries;
                    report.StateTransitions += run.StateTransitions;
                    statesPassed &= run.Finished && run.StateTransitions >= 5 && run.Deliveries > 0 && run.Deliveries <= 240;
                    if (i == RepeatedMatchCount - 1)
                    {
                        report.Innings1Runs = run.FirstRuns;
                        report.Innings1Wickets = run.FirstWickets;
                        report.Innings2Runs = run.SecondRuns;
                        report.Innings2Wickets = run.SecondWickets;
                    }
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                retainedBytes = GC.GetTotalMemory(true) - memoryBefore;
                noLeaks = retainedBytes <= MaxRetainedManagedBytes;

                report.PhysicsProbePassed = physicsPassed;
                report.MatchStateMachinePassed = statesPassed;
                report.RetainedManagedBytes = retainedBytes;
                report.MemoryProbePassed = noLeaks;
                report.Passed = physicsPassed && statesPassed && noLeaks;
            }
            catch (Exception ex)
            {
                report.Passed = false;
                report.Summary = "Stress test threw " + ex.GetType().Name + ": " + ex.Message;
                CricketLogger.LogError(report.Summary);
            }
            finally
            {
                stopwatch.Stop();
                report.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                Time.timeScale = originalTimeScale;
                UnityEngine.Random.state = originalRandomState;
            }

            if (string.IsNullOrEmpty(report.Summary))
            {
                report.Summary = string.Format(CultureInfo.InvariantCulture,
                    "T20 stress: {0}; matches={1}; deliveries={2}; states={3}; physics={4}; retained={5} bytes; time={6:F1} ms.",
                    report.Passed ? "PASS" : "FAIL", report.MatchesSimulated,
                    report.TotalDeliveriesSimulated, report.StateTransitions,
                    report.PhysicsProbePassed ? "PASS" : "FAIL", report.RetainedManagedBytes,
                    report.ExecutionTimeMs);
                if (report.Passed) CricketLogger.Log(report.Summary);
                else CricketLogger.LogError(report.Summary);
            }
            return report;
        }

        private static MatchRun RunSingleT20(int matchNumber)
        {
            ScoringManager previousScoringManager = ScoringManager.Instance;
            ScoringManager.SetInstanceForTesting(null);
            GameObject root = new GameObject("T20_StressTest_" + matchNumber);
            try
            {
                return RunSingleT20Core(root, matchNumber);
            }
            finally
            {
                if (root != null) UnityEngine.Object.DestroyImmediate(root);
                ScoringManager.SetInstanceForTesting(previousScoringManager);
                if (GameObject.Find("T20_StressTest_" + matchNumber) != null)
                    throw new InvalidOperationException("Stress-test match objects remained after cleanup.");
            }
        }

        private static MatchRun RunSingleT20Core(GameObject root, int matchNumber)
        {
            MatchRun run = new MatchRun();
            ScoringManager scoring = root.AddComponent<ScoringManager>();
            ScoringManager.SetInstanceForTesting(scoring);

            CricketAI ai = root.AddComponent<CricketAI>();
            ai.Difficulty = AIDifficulty.Hard;
            MatchController controller = new MatchController(MatchSettings.DefaultT20(), scoring);
            controller.OnMatchStateChanged += delegate { run.StateTransitions++; };
            controller.SetupMatch("Stress XI", "Stress Opponents");
            controller.PerformToss(TossChoice.Heads, TossDecision.Bat);

            PlayerProfile testBatter = new PlayerProfile { name = "QA Batter" };
            BattingSettings battingSettings = new BattingSettings();
            int lastInningsIndex = -1;
            int batterNumber = 0;
            int maxDeliveries = 240;

            while (!scoring.CurrentMatchScore.isMatchComplete && run.Deliveries < maxDeliveries)
            {
                MatchScore matchScore = scoring.CurrentMatchScore;
                int inningsIndex = matchScore.currentInningsIndex;
                InningsScore innings = matchScore.CurrentInnings;
                if (innings == null || innings.IsInningsComplete) break;

                if (inningsIndex != lastInningsIndex)
                {
                    lastInningsIndex = inningsIndex;
                    controller.RuntimeData.currentInningsNumber = inningsIndex + 1;
                    controller.RuntimeData.currentBowlerId = string.Empty;
                    scoring.SetBatters("qa-" + inningsIndex + "-striker", "QA Batter " + inningsIndex,
                        "qa-" + inningsIndex + "-non-striker", "QA Partner " + inningsIndex);
                    scoring.SetBowler("qa-" + inningsIndex + "-bowler-0", "QA Bowler 0");
                    if (inningsIndex == 1 && controller.State != MatchState.Innings2)
                        controller.StartInnings(2);
                }

                int overNumber = innings.CompletedOvers;
                int bowlerNumber = overNumber % 5;
                string bowlerId = "qa-" + inningsIndex + "-bowler-" + bowlerNumber;
                scoring.SetBowler(bowlerId, "QA Bowler " + bowlerNumber);
                controller.RuntimeData.currentBowlerId = bowlerId;

                AIMatchContext context = AIMatchContext.CreateDefault();
                context.currentOver = innings.CompletedOvers;
                context.currentBallInOver = innings.BallsInCurrentOver;
                context.totalOvers = GameConstants.T20MatchOvers == 20f ? 20 : Mathf.RoundToInt(GameConstants.T20MatchOvers);
                context.currentRuns = innings.totalRuns;
                context.wicketsLost = innings.wickets;
                context.isSecondInnings = inningsIndex == 1;
                context.targetRuns = innings.targetScore;
                context.isPowerplay = MatchRules.IsPowerplayActive(innings.legalBalls, 6);
                context.isDeathOvers = innings.CompletedOvers >= 17;
                context.difficulty = AIDifficulty.Hard;
                context.seed = (matchNumber * 10000) + run.Deliveries;

                BowlingDelivery delivery = ai.DecideCompleteDelivery(context, testBatter,
                    inningsIndex == 0 ? BowlingBaseType.Fast : BowlingBaseType.OffSpin);
                if (delivery == null || delivery.releaseSpeedKph <= 0f || delivery.releaseHeight < 0f)
                    throw new InvalidOperationException("AI returned an invalid delivery; simulation cannot advance.");

                BattingResult shot = ai.SimulateCompleteBatting(delivery, context, testBatter,
                    battingSettings, Vector3.forward);
                if (shot == null)
                    throw new InvalidOperationException("AI returned no batting result; simulation cannot advance.");

                UnifiedDeliveryResult result;
                bool forceAllOut = matchNumber == 1 && innings.wickets == GameConstants.WicketsPerInnings - 1;
                bool wicket = innings.wickets < GameConstants.WicketsPerInnings &&
                    (forceAllOut || UnityEngine.Random.value < (shot.isMiss ? 0.08f : 0.025f));
                if (wicket)
                {
                    string dismissed = scoring.CurrentStriker != null ? scoring.CurrentStriker.playerName : "QA Batter";
                    result = UnifiedDeliveryResult.Wicket(shot.isMiss ? "Bowled" : "Caught", dismissed, "QA Fielder");
                }
                else
                {
                    result = UnifiedDeliveryResult.Runs(Mathf.Clamp(shot.estimatedRuns, 0, 6));
                }

                // The live DeliveryController resolves scoring first; the match
                // state controller then observes the updated score and transitions.
                scoring.ProcessDelivery(result);
                controller.RecordDeliveryResult(result);
                run.Deliveries++;

                if (wicket && scoring.CurrentMatchScore.currentInningsIndex == inningsIndex &&
                    !scoring.CurrentInnings.IsInningsComplete)
                {
                    batterNumber++;
                    scoring.ReplaceDismissedBatter("qa-" + inningsIndex + "-replacement-" + batterNumber,
                        "QA Replacement " + batterNumber);
                }
            }

            MatchScore finalScore = scoring.CurrentMatchScore;
            run.FirstRuns = finalScore.firstInnings.totalRuns;
            run.FirstWickets = finalScore.firstInnings.wickets;
            run.SecondRuns = finalScore.secondInnings.totalRuns;
            run.SecondWickets = finalScore.secondInnings.wickets;
            run.Finished = finalScore.isMatchComplete && controller.State == MatchState.MatchFinished;

            if (!run.Finished)
                throw new InvalidOperationException("Match state machine did not conclude within 240 deliveries (possible innings deadlock).");

            return run;
        }

        private static bool RunBallTunnelingProbe(float timeScaleMultiplier)
        {
            GameObject ballObject = new GameObject("T20_StressTest_BallTunnelingProbe");
            try
            {
                SimpleCricketBall ball = ballObject.AddComponent<SimpleCricketBall>();
                ball.LaunchThrow(new Vector3(0f, 1f, 0f), Vector3.down * 40f);

                float scaledStep = (1f / 60f) * timeScaleMultiplier;
                ball.SimulateStep(scaledStep);
                return ball.HasBounced && ball.Position.y >= 0.05f - 0.0001f;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ballObject);
            }
        }
    }
}
#endif
