using System;
using System.Collections.Generic;
using UnityEngine;
using CricketGame.Fielding;
using CricketGame.Gameplay.Ball;
using CricketGame.Gameplay.Batting;
using CricketGame.Camera;

namespace CricketGame.Gameplay.Fielding
{
    public class FieldingManager : MonoBehaviour
    {
        public static FieldingManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private SimpleCricketBall activeBall;
        [SerializeField] private BattingController batsman;
        [SerializeField] private FieldingSettings settings;
        [SerializeField] private List<FieldingController> allFielders = new List<FieldingController>();

        [Header("Runtime State")]
        [SerializeField] private FieldingController activePrimaryFielder;
        [SerializeField] private bool isPlayActive = false;
        [SerializeField] private FieldingResult lastResult;

        public FieldingController ActivePrimaryFielder { get { return activePrimaryFielder; } }
        public bool IsPlayActive { get { return isPlayActive; } }
        public FieldingResult LastResult { get { return lastResult; } }
        public List<FieldingController> AllFielders { get { return allFielders; } }

        public event Action<FieldingController> OnPrimaryFielderSelected;
        public event Action<FieldingResult> OnPlayCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            if (settings == null) settings = FieldingSettings.CreateDefault();
        }

        private void Start()
        {
            if (activeBall == null) activeBall = FindFirstObjectByType<SimpleCricketBall>();
            if (batsman == null) batsman = FindFirstObjectByType<BattingController>();

            DiscoverAndRegisterFielders();

            // Hook up ball hit listener
            if (activeBall != null)
            {
                activeBall.OnBatContact += HandleBatContact;
            }
        }

        private void OnDestroy()
        {
            if (activeBall != null)
            {
                activeBall.OnBatContact -= HandleBatContact;
            }
        }

        public void DiscoverAndRegisterFielders()
        {
            allFielders.Clear();
            FieldingController[] found = FindObjectsByType<FieldingController>(FindObjectsSortMode.None);
            for (int i = 0; i < found.Length; i++)
            {
                allFielders.Add(found[i]);
                found[i].OnFieldingCompleted += HandleFielderActionCompleted;
            }
        }

        public void RegisterFielder(FieldingController fielder)
        {
            if (fielder != null && !allFielders.Contains(fielder))
            {
                allFielders.Add(fielder);
                fielder.OnFieldingCompleted += HandleFielderActionCompleted;
            }
        }

        private void Update()
        {
            if (!isPlayActive) return;

            // Monitor ball boundary crossing
            if (activeBall != null && activeBall.IsInPlay)
            {
                bool isFour;
                bool isSix;
                if (FieldingEvaluator.CheckBoundary(activeBall.Position, settings.boundaryRadius, activeBall.HasBounced, out isFour, out isSix))
                {
                    ResolveBoundary(isSix);
                }
            }
        }

        public void HandleBatContact(BattingResult battingResult)
        {
            if (activeBall == null) return;
            bool isLofted = battingResult != null && (battingResult.launchAngle > 12.0f || battingResult.exitVelocity.y > 3.5f);
            EvaluateAndDispatchFielding(activeBall.Position, activeBall.Velocity, isLofted);
        }

        public void EvaluateAndDispatchFielding(Vector3 ballPos, Vector3 ballVelocity, bool isLofted)
        {
            if (allFielders.Count == 0) DiscoverAndRegisterFielders();

            isPlayActive = true;

            // 1. Score all available fielders
            FieldingController bestFielder = null;
            float highestScore = -1f;

            for (int i = 0; i < allFielders.Count; i++)
            {
                FieldingController f = allFielders[i];
                float score = FieldingEvaluator.ScoreFielder(
                    f.AssignedPosition,
                    f.transform.position,
                    ballPos,
                    ballVelocity,
                    f.Attributes,
                    settings
                );

                if (score > highestScore)
                {
                    highestScore = score;
                    bestFielder = f;
                }
            }

            if (bestFielder == null && allFielders.Count > 0)
            {
                bestFielder = allFielders[0];
            }

            activePrimaryFielder = bestFielder;

            if (bestFielder != null)
            {
                // 2. Calculate 3D interception point and arrival time
                float arrivalTime;
                Vector3 interceptPoint;
                int reactionRating = bestFielder.Attributes != null ? bestFielder.Attributes.reaction : 50;
                float reactionDelay = Mathf.Lerp(settings.reactionDelayMax, settings.reactionDelayMin, reactionRating / 100f);
                float speedRating = bestFielder.Attributes != null ? bestFielder.Attributes.speed : 50;
                float fielderSpeed = Mathf.Lerp(settings.baseRunSpeed * 0.85f, settings.baseSprintSpeed * 1.15f, speedRating / 100f);

                FieldingEvaluator.CalculateInterception(
                    bestFielder.transform.position,
                    fielderSpeed,
                    reactionDelay,
                    ballPos,
                    ballVelocity,
                    -9.81f,
                    out arrivalTime,
                    out interceptPoint
                );

                // 3. Catch feasibility
                bool isCatchOpportunity = isLofted && FieldingEvaluator.IsCatchFeasible(ballPos, ballVelocity, interceptPoint.y, settings.catchMaxHeight);
                float difficulty = 0f;
                if (isCatchOpportunity)
                {
                    difficulty = FieldingEvaluator.CalculateCatchDifficulty(bestFielder.transform.position, interceptPoint, ballVelocity.magnitude, arrivalTime);
                }

                FieldingDecision decision = new FieldingDecision();
                decision.selectedFielderPosition = bestFielder.AssignedPosition;
                decision.interceptionPoint = interceptPoint;
                decision.estimatedArrivalTime = arrivalTime;
                decision.isCatchOpportunity = isCatchOpportunity;
                decision.catchDifficulty = difficulty;
                decision.recommendedAction = isCatchOpportunity ? FieldingActionType.Catch : FieldingActionType.GroundPickup;
                decision.score = highestScore;

                bestFielder.AssignInterception(decision, activeBall);

                // Switch camera to follow action
                if (CricketCameraManager.Instance != null)
                {
                    CricketCameraManager.Instance.SwitchCameraMode(CricketCameraMode.FieldCamera);
                    CricketCameraManager.Instance.SetFollowTarget(activeBall.transform);
                }

                if (OnPrimaryFielderSelected != null)
                {
                    OnPrimaryFielderSelected(bestFielder);
                }
            }
        }

        private void HandleFielderActionCompleted(FieldingResult result)
        {
            isPlayActive = false;
            lastResult = result;

            if (OnPlayCompleted != null)
            {
                OnPlayCompleted(result);
            }
        }

        private void ResolveBoundary(bool isSix)
        {
            if (!isPlayActive) return;

            isPlayActive = false;
            if (activeBall != null) activeBall.StopBall();

            FieldingResult boundaryResult = FieldingResult.CreateBoundary(isSix, isSix ? 6 : 4);
            lastResult = boundaryResult;

            if (OnPlayCompleted != null)
            {
                OnPlayCompleted(boundaryResult);
            }
        }

        // ----------------------------------------------------
        // 8 Test Scenarios Execution
        // ----------------------------------------------------
        public void PlayScenario(int scenarioIndex)
        {
            if (activeBall == null) activeBall = FindFirstObjectByType<SimpleCricketBall>();
            if (activeBall == null) return;

            // Reset all fielders
            for (int i = 0; i < allFielders.Count; i++)
            {
                allFielders[i].ResetToHome();
            }

            Vector3 hitOrigin = new Vector3(0f, 0.4f, 8.8f);
            Vector3 exitVelocity = Vector3.zero;
            bool isLofted = false;

            switch (scenarioIndex)
            {
                case 1: // Ground ball to Point (cut shot)
                    exitVelocity = new Vector3(22.0f, 0.1f, -1.0f);
                    isLofted = false;
                    break;
                case 2: // Ground ball to Cover (cover drive)
                    exitVelocity = new Vector3(20.0f, 0.1f, -18.0f);
                    isLofted = false;
                    break;
                case 3: // Ground ball to MidWicket (pull shot)
                    exitVelocity = new Vector3(-20.0f, 0.1f, -16.0f);
                    isLofted = false;
                    break;
                case 4: // High ball for Catch (lofted drive straight)
                    exitVelocity = new Vector3(3.0f, 15.0f, -22.0f);
                    isLofted = true;
                    break;
                case 5: // Ball toward LongOff (ground drive)
                    exitVelocity = new Vector3(12.0f, 0.2f, -28.0f);
                    isLofted = false;
                    break;
                case 6: // Ball toward DeepMidWicket (lofted pull)
                    exitVelocity = new Vector3(-26.0f, 8.0f, -18.0f);
                    isLofted = true;
                    break;
                case 7: // Ball toward FineLeg (glance)
                    exitVelocity = new Vector3(-18.0f, 0.2f, 18.0f);
                    isLofted = false;
                    break;
                case 8: // Ball toward ThirdMan (thick edge)
                    exitVelocity = new Vector3(18.0f, 0.2f, 20.0f);
                    isLofted = false;
                    break;
            }

            // Setup hit ball
            activeBall.transform.position = hitOrigin;
            BattingResult dummyShot = new BattingResult();
            dummyShot.exitVelocity = exitVelocity;
            dummyShot.launchAngle = isLofted ? 25.0f : 2.0f;
            activeBall.ApplyBatContact(exitVelocity, dummyShot);
        }
    }
}
