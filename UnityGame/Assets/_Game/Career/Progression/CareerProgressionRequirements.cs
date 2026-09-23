using System;
using System.Collections.Generic;
using CricketGame.Career.Stages;

namespace CricketGame.Career.Progression
{
    [Serializable]
    public class StageProgressionCriteria
    {
        public CareerStage targetStage;
        public int minMatchesPlayed;
        public int minRuns;
        public int minWickets;
        public float minBattingAverage;
        public float minBattingStrikeRate;
        public float maxBowlingEconomy;
        public int minMatchWins;
        public int minPlayerOfTheMatchAwards;
        public bool requireTournamentCompletion;

        public StageProgressionCriteria()
        {
            targetStage = CareerStage.UNDER_16;
            minMatchesPlayed = 0;
            minRuns = 0;
            minWickets = 0;
            minBattingAverage = 0f;
            minBattingStrikeRate = 0f;
            maxBowlingEconomy = 99f;
            minMatchWins = 0;
            minPlayerOfTheMatchAwards = 0;
            requireTournamentCompletion = false;
        }

        public StageProgressionCriteria(
            CareerStage target,
            int matches,
            int runs,
            int wickets,
            float avg,
            float sr,
            float econ,
            int wins,
            int potm,
            bool tourComp)
        {
            this.targetStage = target;
            this.minMatchesPlayed = matches;
            this.minRuns = runs;
            this.minWickets = wickets;
            this.minBattingAverage = avg;
            this.minBattingStrikeRate = sr;
            this.maxBowlingEconomy = econ;
            this.minMatchWins = wins;
            this.minPlayerOfTheMatchAwards = potm;
            this.requireTournamentCompletion = tourComp;
        }
    }

    public static class CareerProgressionRequirements
    {
        private static Dictionary<CareerStage, StageProgressionCriteria> criteriaMap;

        static CareerProgressionRequirements()
        {
            criteriaMap = new Dictionary<CareerStage, StageProgressionCriteria>();

            // U16 -> U19 promotion criteria
            criteriaMap[CareerStage.UNDER_19] = new StageProgressionCriteria(
                CareerStage.UNDER_19,
                4,      // matches
                100,    // runs or
                6,      // wickets
                20.0f,  // min average
                90.0f,  // min strike rate
                8.0f,   // max economy
                2,      // match wins
                1,      // POTM award
                true    // tournament completed
            );

            // U19 -> Domestic
            criteriaMap[CareerStage.DOMESTIC] = new StageProgressionCriteria(
                CareerStage.DOMESTIC,
                6, 250, 12, 28.0f, 105.0f, 7.2f, 3, 2, true
            );

            // Domestic -> Country League
            criteriaMap[CareerStage.COUNTRY_LEAGUE] = new StageProgressionCriteria(
                CareerStage.COUNTRY_LEAGUE,
                8, 450, 20, 32.0f, 115.0f, 6.8f, 5, 3, true
            );

            // Country League -> Home Series
            criteriaMap[CareerStage.HOME_SERIES] = new StageProgressionCriteria(
                CareerStage.HOME_SERIES,
                10, 750, 35, 36.0f, 120.0f, 6.2f, 7, 4, true
            );

            // Home Series -> Away Series
            criteriaMap[CareerStage.AWAY_SERIES] = new StageProgressionCriteria(
                CareerStage.AWAY_SERIES,
                12, 1100, 50, 38.0f, 125.0f, 5.8f, 8, 5, true
            );

            // Away Series -> Test Series
            criteriaMap[CareerStage.TEST_SERIES] = new StageProgressionCriteria(
                CareerStage.TEST_SERIES,
                15, 1600, 70, 42.0f, 110.0f, 5.0f, 10, 6, true
            );

            // Test Series -> T20 World Cup
            criteriaMap[CareerStage.T20_WORLD_CUP] = new StageProgressionCriteria(
                CareerStage.T20_WORLD_CUP,
                18, 2100, 85, 40.0f, 135.0f, 6.0f, 12, 7, true
            );

            // T20 World Cup -> ODI World Cup
            criteriaMap[CareerStage.ODI_WORLD_CUP] = new StageProgressionCriteria(
                CareerStage.ODI_WORLD_CUP,
                20, 2800, 100, 45.0f, 120.0f, 5.5f, 14, 8, true
            );
        }

        public static StageProgressionCriteria GetCriteriaForStage(CareerStage targetStage)
        {
            StageProgressionCriteria criteria;
            if (criteriaMap.TryGetValue(targetStage, out criteria))
            {
                return criteria;
            }

            return new StageProgressionCriteria(targetStage, 0, 0, 0, 0f, 0f, 99f, 0, 0, false);
        }
    }
}
