using System;
using System.Collections.Generic;
using CricketGame.Cricket;

namespace CricketGame.Tournaments
{
    [Serializable]
    public class FixtureData
    {
        public string fixtureId;
        public string homeTeamId;
        public string homeTeamName;
        public string awayTeamId;
        public string awayTeamName;
        public MatchFormat format;
        public int roundNumber;
        public bool isCompleted;
        public string winnerTeamId;
        public string resultSummary;
    }

    [Serializable]
    public class TournamentStanding
    {
        public string teamId;
        public string teamName;
        public int played;
        public int won;
        public int lost;
        public int tied;
        public int points;
        public float netRunRate;
    }

    [Serializable]
    public class TournamentProgress
    {
        public string tournamentId;
        public string tournamentName;
        public CareerLevel level;
        public MatchFormat format;
        public int totalOvers;
        public List<FixtureData> fixtures = new List<FixtureData>();
        public List<TournamentStanding> standings = new List<TournamentStanding>();
        public int currentFixtureIndex = 0;
        public bool isCompleted = false;

        public FixtureData GetNextFixture()
        {
            if (fixtures != null && currentFixtureIndex < fixtures.Count)
            {
                return fixtures[currentFixtureIndex];
            }
            return null;
        }
    }
}
