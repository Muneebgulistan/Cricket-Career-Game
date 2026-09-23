using System;

namespace CricketGame.Gameplay.Scoring
{
    [Serializable]
    public class MatchScore
    {
        public string matchId;
        public string teamAName;
        public string teamBName;
        public int currentInningsIndex; // 0 = 1st innings, 1 = 2nd innings
        public InningsScore firstInnings;
        public InningsScore secondInnings;
        public bool isMatchComplete;
        public string winnerTeam;
        public string resultText;

        public InningsScore CurrentInnings
        {
            get
            {
                return (currentInningsIndex == 0) ? firstInnings : secondInnings;
            }
        }

        public MatchScore()
        {
            matchId = Guid.NewGuid().ToString();
            teamAName = "Team A";
            teamBName = "Team B";
            currentInningsIndex = 0;
            firstInnings = new InningsScore("Team A", "Team B", 20, 0);
            secondInnings = new InningsScore("Team B", "Team A", 20, 0);
            isMatchComplete = false;
            winnerTeam = string.Empty;
            resultText = "Match in progress";
        }

        public MatchScore(string id, string teamA, string teamB, int overs)
        {
            matchId = id;
            teamAName = teamA;
            teamBName = teamB;
            currentInningsIndex = 0;
            firstInnings = new InningsScore(teamA, teamB, overs, 0);
            secondInnings = new InningsScore(teamB, teamA, overs, 0);
            isMatchComplete = false;
            winnerTeam = string.Empty;
            resultText = "Match in progress";
        }

        public void StartSecondInnings()
        {
            currentInningsIndex = 1;
            int target = firstInnings.totalRuns + 1;
            secondInnings.targetScore = target;
        }

        public void ConcludeMatch()
        {
            isMatchComplete = true;
            if (firstInnings == null || secondInnings == null) return;

            int target = firstInnings.totalRuns + 1;
            if (secondInnings.totalRuns >= target)
            {
                // Chasing team won
                int wicketsRemaining = 10 - secondInnings.wickets;
                winnerTeam = secondInnings.battingTeam;
                resultText = string.Format("{0} won by {1} wickets", winnerTeam, wicketsRemaining);
            }
            else if (secondInnings.totalRuns == firstInnings.totalRuns)
            {
                // Tie
                winnerTeam = "Tie";
                resultText = "Match tied!";
            }
            else
            {
                // Defending team won
                int runMargin = firstInnings.totalRuns - secondInnings.totalRuns;
                winnerTeam = firstInnings.battingTeam;
                resultText = string.Format("{0} won by {1} runs", winnerTeam, runMargin);
            }
        }
    }
}
