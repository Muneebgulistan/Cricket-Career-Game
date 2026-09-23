using System;
using System.Collections.Generic;
using CricketGame.Cricket;
using CricketGame.Players;
using CricketGame.Career.Objectives;

namespace CricketGame.Career
{
    [Serializable]
    public class CareerProfile
    {
        public string careerId;
        public PlayerProfile player;
        public CareerStatus status;
        public CareerProgression progression;
        public CareerStatistics statistics;
        
        public int currentSeason;
        public int currentWeek;
        public int skillPoints;
        public float reputation;
        public bool isSelectedInPlayingXI;

        // Step 11 Extended Progression & Objectives
        public List<CareerObjective> activeObjectives;
        public List<CareerObjective> completedObjectives;
        public List<string> unlockedTournaments;
        public List<string> claimedRewards;
        public string currentTournamentId;
        public string recentMatchResultSummary;

        public CareerProfile()
        {
            careerId = Guid.NewGuid().ToString();
            player = new PlayerProfile();
            status = CareerStatus.Active;
            progression = new CareerProgression();
            statistics = new CareerStatistics();
            currentSeason = 1;
            currentWeek = 1;
            skillPoints = 0;
            reputation = 50f;
            isSelectedInPlayingXI = true;

            activeObjectives = new List<CareerObjective>();
            completedObjectives = new List<CareerObjective>();
            unlockedTournaments = new List<string> { "U16_NATIONAL_CUP" };
            claimedRewards = new List<string>();
            currentTournamentId = "U16_NATIONAL_CUP";
            recentMatchResultSummary = string.Empty;

            InitializeDefaultObjectives();
        }

        public CareerProfile(PlayerProfile customPlayer)
        {
            careerId = Guid.NewGuid().ToString();
            player = customPlayer;
            status = CareerStatus.Active;
            progression = new CareerProgression();
            statistics = new CareerStatistics();
            currentSeason = 1;
            currentWeek = 1;
            skillPoints = 0;
            reputation = 50f;
            isSelectedInPlayingXI = true;

            activeObjectives = new List<CareerObjective>();
            completedObjectives = new List<CareerObjective>();
            unlockedTournaments = new List<string> { "U16_NATIONAL_CUP" };
            claimedRewards = new List<string>();
            currentTournamentId = "U16_NATIONAL_CUP";
            recentMatchResultSummary = string.Empty;

            InitializeDefaultObjectives();
        }

        public void InitializeDefaultObjectives()
        {
            if (activeObjectives == null) activeObjectives = new List<CareerObjective>();
            if (activeObjectives.Count == 0)
            {
                activeObjectives.Add(CareerObjective.CreateScore50Objective());
                activeObjectives.Add(CareerObjective.CreateTake3WicketsObjective());
                activeObjectives.Add(CareerObjective.CreateWinMatchObjective());
            }
        }

        public void AddObjective(CareerObjective obj)
        {
            if (obj == null) return;
            if (activeObjectives == null) activeObjectives = new List<CareerObjective>();
            activeObjectives.Add(obj);
        }

        public void CompleteObjective(string objectiveId)
        {
            if (activeObjectives == null) return;
            for (int i = 0; i < activeObjectives.Count; i++)
            {
                if (activeObjectives[i].id == objectiveId)
                {
                    var obj = activeObjectives[i];
                    obj.Complete();
                    if (completedObjectives == null) completedObjectives = new List<CareerObjective>();
                    completedObjectives.Add(obj);
                    activeObjectives.RemoveAt(i);
                    break;
                }
            }
        }

        public void UnlockTournament(string tourId)
        {
            if (unlockedTournaments == null) unlockedTournaments = new List<string>();
            if (!unlockedTournaments.Contains(tourId))
            {
                unlockedTournaments.Add(tourId);
            }
        }
    }
}
