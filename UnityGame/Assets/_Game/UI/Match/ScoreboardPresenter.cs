using System;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Gameplay.Match;
using CricketGame.Gameplay.Scoring;

namespace CricketGame.UI.Match
{
    public class ScoreboardPresenter : MonoBehaviour
    {
        [Header("Manager References")]
        [SerializeField] private ScoringManager scoringManager;
        [SerializeField] private MatchManager matchManager;

        [Header("Widgets")]
        [SerializeField] private MatchScoreboard scoreboardWidget;
        [SerializeField] private BatterScoreWidget batterWidget;
        [SerializeField] private BowlerFigureWidget bowlerWidget;
        [SerializeField] private OverSummaryWidget overSummaryWidget;
        [SerializeField] private MatchResultWidget resultWidget;

        private void Start()
        {
            if (scoringManager == null)
            {
                scoringManager = ScoringManager.Instance;
            }

            if (matchManager == null)
            {
                matchManager = MatchManager.Instance;
            }

            SubscribeEvents();
            RefreshAll();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (scoringManager != null)
            {
                scoringManager.OnScoreUpdated += HandleScoreUpdated;
                scoringManager.OnOverCompleted += HandleOverCompleted;
            }

            if (matchManager != null)
            {
                matchManager.OnMatchCompleted += HandleMatchCompleted;
            }

            if (resultWidget != null)
            {
                resultWidget.OnContinueClicked += HandleContinueToCareer;
            }
        }

        private void UnsubscribeEvents()
        {
            if (scoringManager != null)
            {
                scoringManager.OnScoreUpdated -= HandleScoreUpdated;
                scoringManager.OnOverCompleted -= HandleOverCompleted;
            }

            if (matchManager != null)
            {
                matchManager.OnMatchCompleted -= HandleMatchCompleted;
            }

            if (resultWidget != null)
            {
                resultWidget.OnContinueClicked -= HandleContinueToCareer;
            }
        }

        private void HandleScoreUpdated(MatchScore score)
        {
            RefreshAll();
        }

        private void HandleOverCompleted(int overNum, BowlerFigures bowler)
        {
            RefreshAll();
        }

        private void HandleMatchCompleted(CricketGame.Cricket.MatchResult result)
        {
            if (resultWidget != null)
            {
                resultWidget.ShowResult(result);
            }
        }

        private void HandleContinueToCareer()
        {
            // Transition back to career hub
            if (CricketGame.Core.SceneController.Instance != null)
            {
                CricketGame.Core.SceneController.Instance.LoadScene(CricketGame.Core.SceneController.SceneCareerHub);
            }
        }

        public void RefreshAll()
        {
            if (scoringManager == null) return;

            InningsScore inn = scoringManager.CurrentInnings;
            if (inn != null)
            {
                bool isPp = (matchManager != null && matchManager.Settings != null) ?
                    MatchRules.IsPowerplayActive(inn.legalBalls, matchManager.Settings.powerplayOvers) : false;

                if (scoreboardWidget != null)
                {
                    scoreboardWidget.UpdateDisplay(inn, isPp);
                }

                if (batterWidget != null)
                {
                    batterWidget.UpdateDisplay(scoringManager.CurrentStriker, scoringManager.CurrentNonStriker);
                }

                if (bowlerWidget != null)
                {
                    bowlerWidget.UpdateDisplay(scoringManager.CurrentBowler);
                }

                if (overSummaryWidget != null)
                {
                    overSummaryWidget.UpdateDisplay(inn.currentOverBalls);
                }
            }
        }
    }
}
