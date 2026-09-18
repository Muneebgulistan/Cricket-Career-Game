using System;
using CricketGame.Cricket;

namespace CricketGame.Career
{
    [Serializable]
    public class LevelRequirements
    {
        public CareerLevel level;
        public string levelName;
        public int minOverallRating;
        public int minMatchesPlayed;
        public int minRunsRequired;
        public int minWicketsRequired;
        public float minAverageRating;
    }

    [Serializable]
    public class CareerProgression
    {
        public CareerLevel currentLevel = CareerLevel.Under16Cup;
        public int levelProgressPercent = 0;
        public int matchesAtCurrentLevel = 0;
        public float averageMatchRatingAtLevel = 0f;
        public bool isEligibleForPromotion = false;

        public static readonly CareerLevel[] ProgressionLadder = new CareerLevel[]
        {
            CareerLevel.Under16Cup,
            CareerLevel.Under19Cup,
            CareerLevel.DomesticCricket,
            CareerLevel.CountryRegionalLeague,
            CareerLevel.HomeSeries,
            CareerLevel.AwaySeries,
            CareerLevel.TestSeries,
            CareerLevel.T20WorldCup,
            CareerLevel.ODIWorldCup
        };

        public static string GetLevelDisplayName(CareerLevel level)
        {
            switch (level)
            {
                case CareerLevel.Under16Cup: return "Under-16 Cup";
                case CareerLevel.Under19Cup: return "Under-19 Cup";
                case CareerLevel.DomesticCricket: return "Domestic Cricket";
                case CareerLevel.CountryRegionalLeague: return "Country/Regional League";
                case CareerLevel.HomeSeries: return "International Home Series";
                case CareerLevel.AwaySeries: return "International Away Series";
                case CareerLevel.TestSeries: return "Test Match Series";
                case CareerLevel.T20WorldCup: return "T20 World Cup";
                case CareerLevel.ODIWorldCup: return "ODI World Cup";
                default: return level.ToString();
            }
        }

        public bool CanPromoteToNextLevel()
        {
            int currentIndex = Array.IndexOf(ProgressionLadder, currentLevel);
            return currentIndex >= 0 && currentIndex < ProgressionLadder.Length - 1 && isEligibleForPromotion;
        }

        public CareerLevel GetNextLevel()
        {
            int currentIndex = Array.IndexOf(ProgressionLadder, currentLevel);
            if (currentIndex >= 0 && currentIndex < ProgressionLadder.Length - 1)
            {
                return ProgressionLadder[currentIndex + 1];
            }
            return currentLevel;
        }

        public void Promote()
        {
            if (CanPromoteToNextLevel())
            {
                currentLevel = GetNextLevel();
                levelProgressPercent = 0;
                matchesAtCurrentLevel = 0;
                averageMatchRatingAtLevel = 0f;
                isEligibleForPromotion = false;
            }
        }
    }
}
