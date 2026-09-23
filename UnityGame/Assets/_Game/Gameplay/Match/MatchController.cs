using System;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.Gameplay.Match
{
    public class MatchController
    {
        private MatchSettings settings;
        private MatchRuntimeData runtimeData;
        private MatchState state;
        private OverController overController;
        private InningsController inningsController;
        private ScoringManager scoringManager;

        public MatchSettings Settings { get { return settings; } }
        public MatchRuntimeData RuntimeData { get { return runtimeData; } }
        public MatchState State { get { return state; } }

        public event Action<MatchState> OnMatchStateChanged;
        public event Action<string> OnTossCompleted;
        public event Action<CricketGame.Cricket.MatchResult> OnMatchCompleted;

        public MatchController(MatchSettings matchSettings, ScoringManager scoring)
        {
            settings = matchSettings != null ? matchSettings : MatchSettings.DefaultT20();
            runtimeData = new MatchRuntimeData();
            scoringManager = scoring;
            overController = new OverController(settings.totalOvers);
            inningsController = new InningsController(settings.totalOvers);
            SetState(MatchState.PreMatch);
        }

        public void SetState(MatchState newState)
        {
            state = newState;
            if (OnMatchStateChanged != null)
            {
                OnMatchStateChanged(newState);
            }
        }

        public void SetupMatch(string homeTeam, string awayTeam)
        {
            runtimeData.Reset();
            if (scoringManager != null)
            {
                scoringManager.InitializeMatch(homeTeam, awayTeam, settings.totalOvers);
            }
            SetState(MatchState.Toss);
        }

        public void PerformToss(TossChoice playerCall, TossDecision playerDecisionIfWon)
        {
            SetState(MatchState.Toss);

            // Random 50/50 coin toss
            bool isHeads = UnityEngine.Random.Range(0, 2) == 0;
            TossChoice actualCoin = isHeads ? TossChoice.Heads : TossChoice.Tails;
            bool playerWonToss = (playerCall == actualCoin);

            string homeTeam = scoringManager != null ? scoringManager.CurrentMatchScore.teamAName : "Lahore Eagles";
            string awayTeam = scoringManager != null ? scoringManager.CurrentMatchScore.teamBName : "Karachi Kings";

            runtimeData.tossWinnerTeam = playerWonToss ? homeTeam : awayTeam;

            if (playerWonToss)
            {
                runtimeData.tossDecision = playerDecisionIfWon;
            }
            else
            {
                // AI chooses to bat 60% of the time in T20
                runtimeData.tossDecision = UnityEngine.Random.Range(0, 100) < 60 ? TossDecision.Bat : TossDecision.Bowl;
            }

            // Determine active batting and bowling teams
            if ((runtimeData.tossWinnerTeam == homeTeam && runtimeData.tossDecision == TossDecision.Bat) ||
                (runtimeData.tossWinnerTeam == awayTeam && runtimeData.tossDecision == TossDecision.Bowl))
            {
                runtimeData.activeBattingTeam = homeTeam;
                runtimeData.activeBowlingTeam = awayTeam;
            }
            else
            {
                runtimeData.activeBattingTeam = awayTeam;
                runtimeData.activeBowlingTeam = homeTeam;
            }

            string tossDesc = string.Format("{0} won the toss and elected to {1} first.", 
                runtimeData.tossWinnerTeam, 
                runtimeData.tossDecision == TossDecision.Bat ? "bat" : "bowl");

            if (OnTossCompleted != null)
            {
                OnTossCompleted(tossDesc);
            }

            StartInnings(1);
        }

        public void StartInnings(int inningsNumber)
        {
            runtimeData.currentInningsNumber = inningsNumber;
            if (inningsNumber == 1)
            {
                SetState(MatchState.Innings1);
            }
            else
            {
                SetState(MatchState.Innings2);
                if (scoringManager != null && scoringManager.CurrentMatchScore != null)
                {
                    runtimeData.targetScore = scoringManager.CurrentMatchScore.firstInnings.totalRuns + 1;
                    // Swap batting and bowling teams for 2nd innings
                    string temp = runtimeData.activeBattingTeam;
                    runtimeData.activeBattingTeam = runtimeData.activeBowlingTeam;
                    runtimeData.activeBowlingTeam = temp;
                }
            }

            overController.StartNewOver(runtimeData.currentBowlerId);
        }

        public void RecordDeliveryResult(UnifiedDeliveryResult result)
        {
            if (result == null || state == MatchState.MatchFinished) return;

            overController.RecordDelivery(result);

            // Check if over is complete
            if (overController.IsOverComplete)
            {
                overController.CompleteOver(runtimeData.currentBowlerId);
                runtimeData.lastBowlerId = runtimeData.currentBowlerId;
            }

            // Check if innings is complete
            if (scoringManager != null && scoringManager.CurrentInnings != null)
            {
                if (inningsController.CheckInningsEnded(scoringManager.CurrentInnings))
                {
                    if (runtimeData.currentInningsNumber == 1)
                    {
                        SetState(MatchState.InningsBreak);
                        StartInnings(2);
                    }
                    else
                    {
                        ConcludeMatch();
                    }
                }
            }
        }

        public void ConcludeMatch()
        {
            SetState(MatchState.MatchFinished);

            CricketGame.Cricket.MatchResult matchRes = new CricketGame.Cricket.MatchResult();
            if (scoringManager != null && scoringManager.CurrentMatchScore != null)
            {
                var ms = scoringManager.CurrentMatchScore;
                ms.ConcludeMatch();

                matchRes.homeTeamName = ms.teamAName;
                matchRes.awayTeamName = ms.teamBName;
                matchRes.winnerTeamName = ms.winnerTeam;
                matchRes.resultDescription = ms.resultText;
                matchRes.isPlayerVictory = (ms.winnerTeam == ms.teamAName);

                matchRes.firstInnings.totalRuns = ms.firstInnings.totalRuns;
                matchRes.firstInnings.wicketsLost = ms.firstInnings.wickets;
                matchRes.firstInnings.oversCompleted = ms.firstInnings.OversFloat;
                matchRes.firstInnings.battingTeamName = ms.firstInnings.battingTeam;
                matchRes.firstInnings.bowlingTeamName = ms.firstInnings.bowlingTeam;

                matchRes.secondInnings.totalRuns = ms.secondInnings.totalRuns;
                matchRes.secondInnings.wicketsLost = ms.secondInnings.wickets;
                matchRes.secondInnings.oversCompleted = ms.secondInnings.OversFloat;
                matchRes.secondInnings.battingTeamName = ms.secondInnings.battingTeam;
                matchRes.secondInnings.bowlingTeamName = ms.secondInnings.bowlingTeam;
            }

            if (OnMatchCompleted != null)
            {
                OnMatchCompleted(matchRes);
            }
        }

        public void PauseMatch()
        {
            runtimeData.isMatchPaused = true;
            SetState(MatchState.Paused);
            Time.timeScale = 0f;
        }

        public void ResumeMatch()
        {
            runtimeData.isMatchPaused = false;
            SetState(runtimeData.currentInningsNumber == 1 ? MatchState.Innings1 : MatchState.Innings2);
            Time.timeScale = 1f;
        }
    }
}
