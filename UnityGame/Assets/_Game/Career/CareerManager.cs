using System;
using UnityEngine;
using CricketGame.Players;
using CricketGame.SaveSystem;
using CricketGame.Cricket;
using CricketGame.Career.Stages;
using CricketGame.Career.Rewards;
using CricketGame.Career.Objectives;
using CricketGame.Career.Evaluation;
using CricketGame.Career.Progression;
using CricketGame.Career.MatchIntegration;
using CricketGame.Tournaments;

namespace CricketGame.Career
{
    public class CareerManager : MonoBehaviour
    {
        public static CareerManager Instance { get; private set; }

        public CareerProfile ActiveCareer { get; private set; }

        public bool HasActiveCareer
        {
            get { return ActiveCareer != null; }
        }

        public event Action<CareerProfile> OnCareerUpdated;
        public event Action<CareerPerformanceReport> OnMatchEvaluated;
        public event Action<CareerStage> OnStagePromoted;

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
        }

        public void CreateNewCareer(PlayerProfile player)
        {
            ActiveCareer = new CareerProfile(player);
            Debug.Log(string.Format("[CareerManager] Created new career for: {0}", player.name));
            SaveCurrentCareer();
            if (OnCareerUpdated != null) OnCareerUpdated(ActiveCareer);
        }

        public bool LoadExistingCareer()
        {
            if (SaveManager.Instance == null) return false;

            CareerProfile loaded = SaveManager.Instance.LoadCareer();
            if (loaded != null)
            {
                ActiveCareer = loaded;
                string pName = (ActiveCareer.player != null) ? ActiveCareer.player.name : "Unknown";
                Debug.Log(string.Format("[CareerManager] Successfully loaded career for: {0}", pName));
                if (OnCareerUpdated != null) OnCareerUpdated(ActiveCareer);
                return true;
            }
            return false;
        }

        public void SaveCurrentCareer()
        {
            if (ActiveCareer != null && SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveCareer(ActiveCareer);
            }
        }

        public void RecordMatchOutcome(MatchResult result)
        {
            RecordMatchOutcome(result, null);
        }

        public void RecordMatchOutcome(MatchResult result, TournamentProgress tournament)
        {
            if (ActiveCareer == null || result == null) return;

            var perf = result.userPerformance;
            if (perf != null)
            {
                CareerPerformanceReport report = CareerProgressionEvaluator.ApplyPostMatchProgression(
                    ActiveCareer, 
                    perf, 
                    result, 
                    tournament);

                if (OnMatchEvaluated != null)
                {
                    OnMatchEvaluated(report);
                }
            }

            SaveCurrentCareer();
            if (OnCareerUpdated != null)
            {
                OnCareerUpdated(ActiveCareer);
            }
        }

        public bool AdvanceCareerStage(TournamentProgress tournament)
        {
            if (ActiveCareer == null) return false;

            CareerStage newStage;
            bool promoted = CareerProgressionService.TryPromoteCareer(ActiveCareer, tournament, out newStage);
            if (promoted)
            {
                Debug.Log(string.Format("[CareerManager] Player successfully promoted to stage: {0}", newStage));
                SaveCurrentCareer();

                if (OnStagePromoted != null)
                {
                    OnStagePromoted(newStage);
                }
                if (OnCareerUpdated != null)
                {
                    OnCareerUpdated(ActiveCareer);
                }
            }

            return promoted;
        }

        public bool ClaimReward(CareerReward reward)
        {
            if (ActiveCareer == null || reward == null) return false;

            bool success = reward.Apply(ActiveCareer);
            if (success)
            {
                SaveCurrentCareer();
                if (OnCareerUpdated != null)
                {
                    OnCareerUpdated(ActiveCareer);
                }
            }
            return success;
        }

        public void AddObjective(CareerObjective objective)
        {
            if (ActiveCareer != null && objective != null)
            {
                ActiveCareer.AddObjective(objective);
                SaveCurrentCareer();
                if (OnCareerUpdated != null)
                {
                    OnCareerUpdated(ActiveCareer);
                }
            }
        }
    }
}
