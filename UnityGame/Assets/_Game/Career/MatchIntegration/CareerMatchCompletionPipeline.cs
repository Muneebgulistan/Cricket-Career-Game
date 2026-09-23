using System;
using UnityEngine;
using CricketGame.Core;
using CricketGame.Cricket;
using CricketGame.Career.Evaluation;
using CricketGame.Career.Stages;
using CricketGame.Career.Tournaments;
using CricketGame.Tournaments;
using CricketGame.Career.Rewards;

namespace CricketGame.Career.MatchIntegration
{
    public class CareerMatchCompletionPipeline : MonoBehaviour
    {
        public static CareerMatchCompletionPipeline Instance { get; private set; }

        public event Action<CareerMatchContext, CareerPerformanceReport> OnMatchProcessingCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public static CareerPerformanceReport ProcessMatchResult(MatchResult result)
        {
            CareerMatchContext context = null;
            if (CareerMatchLauncher.Instance != null)
            {
                context = CareerMatchLauncher.Instance.ActiveContext;
            }

            CareerProfile profile = (CareerManager.Instance != null) ? CareerManager.Instance.ActiveCareer : null;
            TournamentProgress tournament = (TournamentManager.Instance != null) ? TournamentManager.Instance.ActiveTournament : null;

            return ExecutePipeline(result, profile, tournament, context);
        }

        public static CareerPerformanceReport ExecutePipeline(
            MatchResult result, 
            CareerProfile profile, 
            TournamentProgress tournament, 
            CareerMatchContext context)
        {
            if (result == null)
            {
                Debug.LogWarning("[CareerMatchCompletionPipeline] Null match result received.");
                return new CareerPerformanceReport();
            }

            // Guard against duplicate processing
            if (context != null && context.isResultProcessed)
            {
                Debug.Log("[CareerMatchCompletionPipeline] Result already processed for this match session.");
                return context.lastPerformanceReport;
            }

            if (profile == null)
            {
                Debug.LogWarning("[CareerMatchCompletionPipeline] No active career profile to update.");
                return new CareerPerformanceReport();
            }

            // 1. Ensure user performance object exists
            var performance = result.userPerformance;
            if (performance == null)
            {
                performance = new PlayerMatchPerformance();
                performance.playerName = (profile.player != null) ? profile.player.name : "Player";
                result.userPerformance = performance;
            }

            // 2. Structured performance evaluation + career stats + objectives via CareerProgressionEvaluator
            CareerPerformanceReport report = CareerProgressionEvaluator.ApplyPostMatchProgression(
                profile, 
                performance, 
                result, 
                tournament);

            // 3. Update Tournament Fixture & Standings Table
            if (tournament != null)
            {
                if (TournamentManager.Instance != null)
                {
                    string fixId = (context != null && context.fixture != null) 
                        ? context.fixture.fixtureId 
                        : (tournament.GetNextFixture() != null ? tournament.GetNextFixture().fixtureId : "FIX_1");

                    string winnerTeam = result.winnerTeamName;
                    int homeRuns = result.isPlayerVictory ? 160 : 140;
                    int awayRuns = result.isPlayerVictory ? 150 : 165;

                    TournamentManager.Instance.RecordUserMatchResult(
                        fixId, 
                        winnerTeam, 
                        homeRuns, 
                        20.0f, 
                        awayRuns, 
                        20.0f
                    );
                }

                // 4. Evaluate Tournament Qualification Status
                EvaluateTournamentStatus(tournament, profile);
            }

            // 5. Evaluate Career Stage Promotion
            bool promoted = false;
            if (CareerManager.Instance != null)
            {
                promoted = CareerManager.Instance.AdvanceCareerStage(tournament);
            }

            // 6. Automatically unlock next tournament tier if promoted
            if (promoted)
            {
                CareerStage currentStage = profile.progression.currentStage;
                CareerStageDefinition def = CareerStageDefinition.GetStageDefinition(currentStage);
                if (def != null && !string.IsNullOrEmpty(def.requiredTournamentId))
                {
                    if (profile.unlockedTournaments != null && !profile.unlockedTournaments.Contains(def.requiredTournamentId))
                    {
                        profile.unlockedTournaments.Add(def.requiredTournamentId);
                        Debug.Log(string.Format("[CareerMatchCompletionPipeline] Unlocked tournament: {0}", def.requiredTournamentId));
                    }
                }
            }

            // 7. Save Career
            if (CareerManager.Instance != null)
            {
                CareerManager.Instance.SaveCurrentCareer();
            }

            // 8. Update context snapshot
            if (context != null)
            {
                context.lastMatchResult = result;
                context.lastPerformanceReport = report;
                context.isResultProcessed = true;
                context.isMatchInProgress = false;
            }

            // 9. Fire completion event
            if (Instance != null && Instance.OnMatchProcessingCompleted != null)
            {
                Instance.OnMatchProcessingCompleted(context, report);
            }

            // 10. Transition to Career Match Result Screen
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.CareerMatchResult);
            }

            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadScene(SceneController.SceneCareerMatchResult);
            }

            return report;
        }

        private static void EvaluateTournamentStatus(TournamentProgress tournament, CareerProfile profile)
        {
            if (tournament == null) return;

            if (tournament.isCompleted)
            {
                if (tournament.matchesWon > tournament.matchesLost && tournament.matchesWon >= 2)
                {
                    tournament.UpdateQualificationStatus(TournamentQualificationStatus.Champion);
                    if (profile != null)
                    {
                        profile.reputation += 25f;
                        profile.skillPoints += 10;
                    }
                }
                else if (tournament.matchesWon > 0)
                {
                    tournament.UpdateQualificationStatus(TournamentQualificationStatus.Qualified);
                }
                else
                {
                    tournament.UpdateQualificationStatus(TournamentQualificationStatus.Eliminated);
                }
            }
            else
            {
                tournament.UpdateQualificationStatus(TournamentQualificationStatus.InContention);
            }
        }
    }
}
