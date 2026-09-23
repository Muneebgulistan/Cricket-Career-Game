using System;
using System.Collections.Generic;
using CricketGame.Cricket;

namespace CricketGame.Tournaments
{
    public enum TournamentState
    {
        NotStarted,
        Locked,
        Available,
        InProgress,
        Completed,
        Failed,
        Eliminated
    }

    public enum TournamentQualificationStatus
    {
        InContention,
        Qualified,
        Eliminated,
        Champion,
        RunnerUp
    }

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

        // Step 11 Extended Tournament State & Tracking
        public TournamentState state = TournamentState.Available;
        public TournamentQualificationStatus qualificationStatus = TournamentQualificationStatus.InContention;
        public int matchesPlayed = 0;
        public int matchesWon = 0;
        public int matchesLost = 0;
        public int matchesTied = 0;
        public int playerRunsScored = 0;
        public int playerWicketsTaken = 0;
        public float playerBattingAverage = 0f;
        public float playerBowlingEconomy = 0f;
        public int playerOfTheMatchCount = 0;
        public int currentRound = 1;
        public int totalRounds = 3;
        public int tournamentPosition = 1;

        public FixtureData GetNextFixture()
        {
            if (fixtures != null && currentFixtureIndex < fixtures.Count)
            {
                return fixtures[currentFixtureIndex];
            }
            return null;
        }

        public void UpdatePlayerPerformance(int runs, int wickets, bool won, bool potm)
        {
            matchesPlayed++;
            if (won) matchesWon++;
            else matchesLost++;

            playerRunsScored += runs;
            playerWicketsTaken += wickets;
            if (potm) playerOfTheMatchCount++;

            playerBattingAverage = matchesPlayed > 0 ? (float)playerRunsScored / matchesPlayed : playerRunsScored;
            state = isCompleted ? TournamentState.Completed : TournamentState.InProgress;
        }

        public void UpdateState(TournamentState newState)
        {
            state = newState;
            if (state == TournamentState.Completed)
            {
                isCompleted = true;
            }
        }

        public void UpdateQualificationStatus(TournamentQualificationStatus newStatus)
        {
            qualificationStatus = newStatus;
            if (newStatus == TournamentQualificationStatus.Eliminated)
            {
                state = TournamentState.Eliminated;
            }
            else if (newStatus == TournamentQualificationStatus.Champion)
            {
                state = TournamentState.Completed;
                isCompleted = true;
            }
        }
    }
}
