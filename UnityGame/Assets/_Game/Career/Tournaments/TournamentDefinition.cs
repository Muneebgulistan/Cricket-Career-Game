using System;
using System.Collections.Generic;
using CricketGame.Cricket;

namespace CricketGame.Career.Tournaments
{
    [Serializable]
    public class TournamentDefinition
    {
        public string tournamentId;
        public string tournamentName;
        public CareerLevel level;
        public MatchFormat format;
        public int totalOvers;
        public List<string> teamNames;

        public TournamentDefinition()
        {
            tournamentId = "U16_NATIONAL_CUP";
            tournamentName = "National Under-16 Championship";
            level = CareerLevel.Under16Cup;
            format = MatchFormat.T20;
            totalOvers = 20;
            teamNames = new List<string>
            {
                "Lahore Eagles U-16",
                "Karachi Kings U-16",
                "Islamabad United U-16",
                "Peshawar Zalmi U-16"
            };
        }

        public static TournamentDefinition CreateUnder16NationalCup()
        {
            return new TournamentDefinition();
        }
    }
}
