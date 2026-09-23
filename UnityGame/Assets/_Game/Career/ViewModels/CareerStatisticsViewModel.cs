using System;

namespace CricketGame.Career.ViewModels
{
    [Serializable]
    public class CareerStatisticsViewModel
    {
        // Batting
        public int matches;
        public int innings;
        public int runs;
        public int highestScore;
        public float average;
        public float strikeRate;
        public int hundreds;
        public int fifties;
        public int fours;
        public int sixes;

        // Bowling
        public float overs;
        public int wickets;
        public float bowlingAverage;
        public float economy;
        public string bestFigures;
        public int maidens;
        public int fiveWicketHauls;

        // Fielding
        public int catches;
        public int runOuts;
        public int stumpings;

        public CareerStatisticsViewModel()
        {
            matches = 0;
            innings = 0;
            runs = 0;
            highestScore = 0;
            average = 0f;
            strikeRate = 0f;
            hundreds = 0;
            fifties = 0;
            fours = 0;
            sixes = 0;
            overs = 0f;
            wickets = 0;
            bowlingAverage = 0f;
            economy = 0f;
            bestFigures = "0/0";
            maidens = 0;
            fiveWicketHauls = 0;
            catches = 0;
            runOuts = 0;
            stumpings = 0;
        }

        public static CareerStatisticsViewModel FromStatistics(CareerStatistics stats)
        {
            CareerStatisticsViewModel vm = new CareerStatisticsViewModel();
            if (stats == null) return vm;

            var bat = stats.allTimeBatting;
            if (bat != null)
            {
                vm.matches = bat.matches;
                vm.innings = bat.innings;
                vm.runs = bat.runs;
                vm.highestScore = bat.highestScore;
                vm.average = bat.Average;
                vm.strikeRate = bat.StrikeRate;
                vm.hundreds = bat.hundreds;
                vm.fifties = bat.fifties;
                vm.fours = bat.fours;
                vm.sixes = bat.sixes;
            }

            var bowl = stats.allTimeBowling;
            if (bowl != null)
            {
                vm.overs = bowl.overs;
                vm.wickets = bowl.wickets;
                vm.bowlingAverage = bowl.Average;
                vm.economy = bowl.Economy;
                vm.bestFigures = string.Format("{0}/{1}", bowl.bestWickets, bowl.bestRunsConceded);
                vm.maidens = bowl.maidens;
                vm.fiveWicketHauls = bowl.fiveWicketHauls;
            }

            var field = stats.allTimeFielding;
            if (field != null)
            {
                vm.catches = field.catches;
                vm.runOuts = field.runOuts;
                vm.stumpings = field.stumpings;
            }

            return vm;
        }
    }
}
