using System;
using UnityEngine;

namespace CricketGame.Gameplay.Scoring
{
    public class ScoringManager : MonoBehaviour
    {
        public static ScoringManager Instance { get; private set; }

        [Header("Runtime Match Score")]
        [SerializeField] private MatchScore matchScore;
        [SerializeField] private BatterScore currentStriker;
        [SerializeField] private BatterScore currentNonStriker;
        [SerializeField] private BowlerFigures currentBowler;

        private ScoringController scoringController;

        public MatchScore CurrentMatchScore
        {
            get { return matchScore; }
        }

        public InningsScore CurrentInnings
        {
            get { return matchScore != null ? matchScore.CurrentInnings : null; }
        }

        public BatterScore CurrentStriker
        {
            get { return currentStriker; }
        }

        public BatterScore CurrentNonStriker
        {
            get { return currentNonStriker; }
        }

        public BowlerFigures CurrentBowler
        {
            get { return currentBowler; }
        }

        public event Action<MatchScore> OnScoreUpdated;
        public event Action<BatterScore> OnWicketFallen;
        public event Action<int, BowlerFigures> OnOverCompleted;
        public event Action<InningsScore> OnInningsCompleted;
        public event Action<MatchScore> OnMatchCompleted;

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

            scoringController = new ScoringController();
            if (matchScore == null)
            {
                matchScore = new MatchScore();
            }
        }

        public void InitializeMatch(string teamA, string teamB, int maxOvers)
        {
            if (scoringController == null) scoringController = new ScoringController();
            matchScore = new MatchScore(Guid.NewGuid().ToString(), teamA, teamB, maxOvers);
        }

        public void SetBatters(string strikerId, string strikerName, string nonStrikerId, string nonStrikerName)
        {
            if (CurrentInnings == null) return;
            currentStriker = CurrentInnings.GetOrCreateBatter(strikerId, strikerName, true);
            currentNonStriker = CurrentInnings.GetOrCreateBatter(nonStrikerId, nonStrikerName, false);
            currentStriker.isOnStrike = true;
            currentNonStriker.isOnStrike = false;
        }

        public void SetBowler(string bowlerId, string bowlerName)
        {
            if (CurrentInnings == null) return;
            currentBowler = CurrentInnings.GetOrCreateBowler(bowlerId, bowlerName);
        }

        public void RotateStrike()
        {
            BatterScore temp = currentStriker;
            currentStriker = currentNonStriker;
            currentNonStriker = temp;

            if (currentStriker != null) currentStriker.isOnStrike = true;
            if (currentNonStriker != null) currentNonStriker.isOnStrike = false;
        }

        public void ReplaceDismissedBatter(string newBatterId, string newBatterName)
        {
            if (CurrentInnings == null) return;
            // The dismissed batter was currentStriker
            currentStriker = CurrentInnings.GetOrCreateBatter(newBatterId, newBatterName, true);
            currentStriker.isOnStrike = true;
        }

        public void ProcessDelivery(UnifiedDeliveryResult result)
        {
            if (CurrentInnings == null || result == null) return;
            if (scoringController == null) scoringController = new ScoringController();

            bool rotateStrike;
            bool overComplete;

            scoringController.ApplyDeliveryResult(
                result, 
                CurrentInnings, 
                currentStriker, 
                currentNonStriker, 
                currentBowler, 
                out rotateStrike, 
                out overComplete);

            if (result.isWicket)
            {
                if (OnWicketFallen != null)
                {
                    OnWicketFallen(currentStriker);
                }
            }

            if (rotateStrike)
            {
                RotateStrike();
            }

            if (overComplete)
            {
                int overNum = CurrentInnings.CompletedOvers;
                if (OnOverCompleted != null)
                {
                    OnOverCompleted(overNum, currentBowler);
                }
                CurrentInnings.currentOverBalls.Clear();
            }

            if (OnScoreUpdated != null)
            {
                OnScoreUpdated(matchScore);
            }

            // Check Innings and Match Completion
            if (CurrentInnings.IsInningsComplete)
            {
                if (matchScore.currentInningsIndex == 0)
                {
                    if (OnInningsCompleted != null)
                    {
                        OnInningsCompleted(CurrentInnings);
                    }
                    matchScore.StartSecondInnings();
                }
                else
                {
                    matchScore.ConcludeMatch();
                    if (OnMatchCompleted != null)
                    {
                        OnMatchCompleted(matchScore);
                    }
                }
            }
        }
    }
}
