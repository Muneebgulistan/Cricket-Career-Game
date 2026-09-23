using System;
using System.Collections.Generic;
using CricketGame.Cricket;

namespace CricketGame.Career.Stages
{
    [Serializable]
    public class CareerStageDefinition
    {
        public CareerStage stage;
        public string displayName;
        public int minAge;
        public int maxAge;
        public int minOverallRating;
        public int minCareerRuns;
        public int minCareerWickets;
        public float minBattingAverage;
        public float maxBowlingEconomy;
        public string requiredTournamentId;
        public float reputationRequirement;
        public bool isUnlocked;
        public bool isCompleted;

        public CareerStageDefinition()
        {
            stage = CareerStage.UNDER_16;
            displayName = "Under-16 Championship";
            minAge = 14;
            maxAge = 16;
            minOverallRating = 40;
            minCareerRuns = 0;
            minCareerWickets = 0;
            minBattingAverage = 0f;
            maxBowlingEconomy = 99f;
            requiredTournamentId = "U16_NATIONAL_CUP";
            reputationRequirement = 0f;
            isUnlocked = true;
            isCompleted = false;
        }

        public CareerStageDefinition(
            CareerStage stage,
            string displayName,
            int minAge,
            int maxAge,
            int minRating,
            int minRuns,
            int minWickets,
            float minAvg,
            float maxEcon,
            string tournamentId,
            float reputationReq,
            bool unlocked)
        {
            this.stage = stage;
            this.displayName = displayName;
            this.minAge = minAge;
            this.maxAge = maxAge;
            this.minOverallRating = minRating;
            this.minCareerRuns = minRuns;
            this.minCareerWickets = minWickets;
            this.minBattingAverage = minAvg;
            this.maxBowlingEconomy = maxEcon;
            this.requiredTournamentId = tournamentId;
            this.reputationRequirement = reputationReq;
            this.isUnlocked = unlocked;
            this.isCompleted = false;
        }

        public static CareerStage FromCareerLevel(CareerLevel level)
        {
            switch (level)
            {
                case CareerLevel.Under16Cup: return CareerStage.UNDER_16;
                case CareerLevel.Under19Cup: return CareerStage.UNDER_19;
                case CareerLevel.DomesticCricket: return CareerStage.DOMESTIC;
                case CareerLevel.CountryRegionalLeague: return CareerStage.COUNTRY_LEAGUE;
                case CareerLevel.HomeSeries: return CareerStage.HOME_SERIES;
                case CareerLevel.AwaySeries: return CareerStage.AWAY_SERIES;
                case CareerLevel.TestSeries: return CareerStage.TEST_SERIES;
                case CareerLevel.T20WorldCup: return CareerStage.T20_WORLD_CUP;
                case CareerLevel.ODIWorldCup: return CareerStage.ODI_WORLD_CUP;
                default: return CareerStage.UNDER_16;
            }
        }

        public static CareerLevel ToCareerLevel(CareerStage stage)
        {
            switch (stage)
            {
                case CareerStage.UNDER_16: return CareerLevel.Under16Cup;
                case CareerStage.UNDER_19: return CareerLevel.Under19Cup;
                case CareerStage.DOMESTIC: return CareerLevel.DomesticCricket;
                case CareerStage.COUNTRY_LEAGUE: return CareerLevel.CountryRegionalLeague;
                case CareerStage.HOME_SERIES: return CareerLevel.HomeSeries;
                case CareerStage.AWAY_SERIES: return CareerLevel.AwaySeries;
                case CareerStage.TEST_SERIES: return CareerLevel.TestSeries;
                case CareerStage.T20_WORLD_CUP: return CareerLevel.T20WorldCup;
                case CareerStage.ODI_WORLD_CUP: return CareerLevel.ODIWorldCup;
                default: return CareerLevel.Under16Cup;
            }
        }

        public static List<CareerStageDefinition> GetStandardStageDefinitions()
        {
            List<CareerStageDefinition> stages = new List<CareerStageDefinition>();

            stages.Add(new CareerStageDefinition(
                CareerStage.UNDER_16,
                "Under-16 Championship",
                14, 16, 40, 0, 0, 0f, 99f,
                "U16_NATIONAL_CUP", 0f, true));

            stages.Add(new CareerStageDefinition(
                CareerStage.UNDER_19,
                "Under-19 World Cup",
                16, 19, 55, 150, 8, 25f, 7.5f,
                "U19_WORLD_CUP", 25f, false));

            stages.Add(new CareerStageDefinition(
                CareerStage.DOMESTIC,
                "First-Class & Domestic Trophy",
                18, 38, 65, 350, 18, 30f, 6.8f,
                "DOMESTIC_TROPHY", 45f, false));

            stages.Add(new CareerStageDefinition(
                CareerStage.COUNTRY_LEAGUE,
                "Regional T20 Super League",
                18, 40, 72, 600, 30, 32f, 6.5f,
                "REGIONAL_T20_LEAGUE", 60f, false));

            stages.Add(new CareerStageDefinition(
                CareerStage.HOME_SERIES,
                "International Home Series",
                19, 40, 78, 1000, 50, 35f, 6.0f,
                "INT_HOME_SERIES", 70f, false));

            stages.Add(new CareerStageDefinition(
                CareerStage.AWAY_SERIES,
                "International Away Series",
                20, 40, 82, 1400, 70, 38f, 5.8f,
                "INT_AWAY_SERIES", 78f, false));

            stages.Add(new CareerStageDefinition(
                CareerStage.TEST_SERIES,
                "Pinnacle Test Match Series",
                20, 40, 85, 2000, 90, 40f, 5.2f,
                "TEST_CHAMPIONSHIP", 85f, false));

            stages.Add(new CareerStageDefinition(
                CareerStage.T20_WORLD_CUP,
                "ICC Men's T20 World Cup",
                20, 40, 88, 2500, 110, 42f, 5.0f,
                "ICC_T20_WORLD_CUP", 90f, false));

            stages.Add(new CareerStageDefinition(
                CareerStage.ODI_WORLD_CUP,
                "ICC Cricket World Cup",
                20, 40, 90, 3200, 130, 45f, 4.8f,
                "ICC_ODI_WORLD_CUP", 95f, false));

            return stages;
        }
    }
}
