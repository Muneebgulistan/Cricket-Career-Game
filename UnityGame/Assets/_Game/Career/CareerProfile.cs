using System;
using CricketGame.Cricket;
using CricketGame.Players;

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
        }
    }
}
