using System;
using CricketGame.Cricket;

namespace CricketGame.Players
{
    [Serializable]
    public class PlayerProfile
    {
        public string id;
        public string name;
        public int age;
        public string nationality;
        public PlayingRole playingRole;
        public BattingStyle battingStyle;
        public BowlingStyle bowlingStyle;
        
        // Core Ratings (1 - 99)
        public int overallRating;
        public int battingRating;
        public int bowlingRating;
        public int fieldingRating;
        
        // Dynamic Player Condition (0 - 100)
        public int fitness;
        public int form;
        public int experience;
        
        // Career Status & Team
        public CareerLevel currentLevel;
        public string currentTeam;

        public PlayerProfile()
        {
            id = Guid.NewGuid().ToString();
            name = "New Player";
            age = 16;
            nationality = "Pakistan";
            playingRole = PlayingRole.Batsman;
            battingStyle = BattingStyle.RightHand;
            bowlingStyle = BowlingStyle.None;
            
            overallRating = 55;
            battingRating = 60;
            bowlingRating = 40;
            fieldingRating = 50;
            
            fitness = 100;
            form = 75;
            experience = 0;
            
            currentLevel = CareerLevel.Under16Cup;
            currentTeam = "Lahore Eagles U-16";
        }

        public PlayerProfile(string playerName, int playerAge, string playerNationality, PlayingRole role, BattingStyle batStyle, BowlingStyle bowlStyle)
        {
            id = Guid.NewGuid().ToString();
            name = playerName;
            age = playerAge;
            nationality = playerNationality;
            playingRole = role;
            battingStyle = batStyle;
            bowlingStyle = bowlStyle;

            RecalculateOverallRating();
            fitness = 100;
            form = 75;
            experience = 0;
            currentLevel = CareerLevel.Under16Cup;
            currentTeam = "Regional Academy U-16";
        }

        public void RecalculateOverallRating()
        {
            switch (playingRole)
            {
                case PlayingRole.Batsman:
                    overallRating = (int)(battingRating * 0.7f + fieldingRating * 0.3f);
                    break;
                case PlayingRole.Bowler:
                    overallRating = (int)(bowlingRating * 0.7f + fieldingRating * 0.3f);
                    break;
                case PlayingRole.AllRounder:
                    overallRating = (int)(battingRating * 0.45f + bowlingRating * 0.45f + fieldingRating * 0.1f);
                    break;
                case PlayingRole.WicketKeeper:
                    overallRating = (int)(battingRating * 0.6f + fieldingRating * 0.4f);
                    break;
            }
        }
    }
}
