using System;
using UnityEngine;
using CricketGame.Players;
using CricketGame.SaveSystem;
using CricketGame.Cricket;

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
            if (SaveManager.Instance != null) SaveManager.Instance.SaveCareer(ActiveCareer);
            if (OnCareerUpdated != null) OnCareerUpdated(ActiveCareer);
        }

        public bool LoadExistingCareer()
        {
            if (SaveManager.Instance == null) return false;

            CareerProfile loaded = SaveManager.Instance.LoadCareer();
            if (loaded != null)
            {
                ActiveCareer = loaded;
                Debug.Log(string.Format("[CareerManager] Successfully loaded career for: {0}", ActiveCareer.player.name));
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
            if (ActiveCareer == null || result == null) return;

            var perf = result.userPerformance;
            if (perf != null)
            {
                ActiveCareer.statistics.RecordMatchBatting(perf.runs, perf.balls, perf.fours, perf.sixes, perf.isOut);
                ActiveCareer.statistics.RecordMatchBowling(perf.oversBowled, perf.maidens, perf.runsConceded, perf.wickets);
                ActiveCareer.statistics.RecordMatchFielding(perf.catches, perf.runOuts, perf.stumpings);

                // Update condition
                ActiveCareer.player.form = Mathf.Clamp(ActiveCareer.player.form + (result.isPlayerVictory ? 4 : -2), 10, 100);
                ActiveCareer.player.fitness = Mathf.Clamp(ActiveCareer.player.fitness - 5, 20, 100);
                ActiveCareer.player.experience += 10;
            }

            SaveCurrentCareer();
            if (OnCareerUpdated != null) OnCareerUpdated(ActiveCareer);
        }
    }
}
