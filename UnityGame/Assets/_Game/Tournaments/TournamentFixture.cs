using System;
using CricketGame.Cricket;

namespace CricketGame.Tournaments
{
    [Serializable]
    public enum FixtureStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Abandoned
    }

    [Serializable]
    public class TournamentFixture
    {
        public int matchNumber;
        public string fixtureId;
        public string homeTeamName;
        public string awayTeamName;
        public string venue;
        public int roundNumber;
        public MatchFormat format;
        public FixtureStatus status;
        public bool isCompleted;
        public string winnerTeamName;
        public string resultSummary;

        public TournamentFixture()
        {
            fixtureId = Guid.NewGuid().ToString();
            status = FixtureStatus.Scheduled;
            isCompleted = false;
            format = MatchFormat.T20;
            venue = "National Stadium";
            winnerTeamName = string.Empty;
            resultSummary = "Scheduled";
        }

        public TournamentFixture(
            int matchNum, 
            string fixId, 
            string home, 
            string away, 
            string venueName, 
            int round, 
            MatchFormat matchFormat)
        {
            matchNumber = matchNum;
            fixtureId = fixId;
            homeTeamName = home;
            awayTeamName = away;
            venue = venueName;
            roundNumber = round;
            format = matchFormat;
            status = FixtureStatus.Scheduled;
            isCompleted = false;
            winnerTeamName = string.Empty;
            resultSummary = "Scheduled";
        }

        public string GetOpponent(string userTeamName)
        {
            if (string.Equals(homeTeamName, userTeamName, StringComparison.OrdinalIgnoreCase))
            {
                return awayTeamName;
            }
            return homeTeamName;
        }

        public bool IsUserHome(string userTeamName)
        {
            return string.Equals(homeTeamName, userTeamName, StringComparison.OrdinalIgnoreCase);
        }

        public void MarkCompleted(string winner, string summary)
        {
            winnerTeamName = winner;
            resultSummary = summary;
            isCompleted = true;
            status = FixtureStatus.Completed;
        }

        public static TournamentFixture FromFixtureData(FixtureData data, int matchNum, string venueName)
        {
            if (data == null) return null;

            TournamentFixture tf = new TournamentFixture();
            tf.matchNumber = matchNum;
            tf.fixtureId = data.fixtureId;
            tf.homeTeamName = data.homeTeamName;
            tf.awayTeamName = data.awayTeamName;
            tf.venue = string.IsNullOrEmpty(venueName) ? "National Stadium" : venueName;
            tf.roundNumber = data.roundNumber;
            tf.format = data.format;
            tf.isCompleted = data.isCompleted;
            tf.status = data.isCompleted ? FixtureStatus.Completed : FixtureStatus.Scheduled;
            tf.winnerTeamName = data.winnerTeamId;
            tf.resultSummary = data.resultSummary;
            return tf;
        }

        public FixtureData ToFixtureData()
        {
            FixtureData fd = new FixtureData();
            fd.fixtureId = fixtureId;
            fd.homeTeamId = homeTeamName;
            fd.homeTeamName = homeTeamName;
            fd.awayTeamId = awayTeamName;
            fd.awayTeamName = awayTeamName;
            fd.format = format;
            fd.roundNumber = roundNumber;
            fd.isCompleted = isCompleted;
            fd.winnerTeamId = winnerTeamName;
            fd.resultSummary = resultSummary;
            return fd;
        }
    }
}
