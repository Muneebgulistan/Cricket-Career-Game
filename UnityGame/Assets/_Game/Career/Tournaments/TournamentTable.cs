using System;
using System.Collections.Generic;
using CricketGame.Tournaments;

namespace CricketGame.Career.Tournaments
{
    public class TournamentTable
    {
        public List<TournamentStanding> standings;

        // Separate accumulators for Net Run Rate (NRR) calculation
        private Dictionary<string, int> runsScored = new Dictionary<string, int>();
        private Dictionary<string, float> oversFaced = new Dictionary<string, float>();
        private Dictionary<string, int> runsConceded = new Dictionary<string, int>();
        private Dictionary<string, float> oversBowled = new Dictionary<string, float>();

        public TournamentTable()
        {
            standings = new List<TournamentStanding>();
        }

        public TournamentTable(List<string> teams)
        {
            standings = new List<TournamentStanding>();
            for (int i = 0; i < teams.Count; i++)
            {
                AddTeam(teams[i], teams[i]);
            }
        }

        public void AddTeam(string id, string name)
        {
            TournamentStanding s = new TournamentStanding();
            s.teamId = id;
            s.teamName = name;
            s.played = 0;
            s.won = 0;
            s.lost = 0;
            s.tied = 0;
            s.points = 0;
            s.netRunRate = 0f;
            standings.Add(s);

            runsScored[id] = 0;
            oversFaced[id] = 0f;
            runsConceded[id] = 0;
            oversBowled[id] = 0f;
        }

        public TournamentStanding GetStanding(string teamId)
        {
            for (int i = 0; i < standings.Count; i++)
            {
                if (standings[i].teamId == teamId) return standings[i];
            }
            return null;
        }

        public void RecordMatch(
            string teamAId, 
            int teamARuns, 
            float teamAOvers, 
            string teamBId, 
            int teamBRuns, 
            float teamBOvers, 
            string winnerId)
        {
            TournamentStanding standingA = GetStanding(teamAId);
            TournamentStanding standingB = GetStanding(teamBId);

            if (standingA == null || standingB == null) return;

            standingA.played++;
            standingB.played++;

            if (string.IsNullOrEmpty(winnerId) || winnerId == "Tie")
            {
                standingA.tied++;
                standingB.tied++;
                standingA.points += 1;
                standingB.points += 1;
            }
            else if (winnerId == teamAId)
            {
                standingA.won++;
                standingA.points += 2;
                standingB.lost++;
            }
            else
            {
                standingB.won++;
                standingB.points += 2;
                standingA.lost++;
            }

            // NRR Accumulation
            if (!runsScored.ContainsKey(teamAId)) runsScored[teamAId] = 0;
            if (!oversFaced.ContainsKey(teamAId)) oversFaced[teamAId] = 0f;
            if (!runsConceded.ContainsKey(teamAId)) runsConceded[teamAId] = 0;
            if (!oversBowled.ContainsKey(teamAId)) oversBowled[teamAId] = 0f;

            if (!runsScored.ContainsKey(teamBId)) runsScored[teamBId] = 0;
            if (!oversFaced.ContainsKey(teamBId)) oversFaced[teamBId] = 0f;
            if (!runsConceded.ContainsKey(teamBId)) runsConceded[teamBId] = 0;
            if (!oversBowled.ContainsKey(teamBId)) oversBowled[teamBId] = 0f;

            runsScored[teamAId] += teamARuns;
            oversFaced[teamAId] += Math.Max(1.0f, teamAOvers);
            runsConceded[teamAId] += teamBRuns;
            oversBowled[teamAId] += Math.Max(1.0f, teamBOvers);

            runsScored[teamBId] += teamBRuns;
            oversFaced[teamBId] += Math.Max(1.0f, teamBOvers);
            runsConceded[teamBId] += teamARuns;
            oversBowled[teamBId] += Math.Max(1.0f, teamAOvers);

            UpdateNRR(standingA);
            UpdateNRR(standingB);
            SortStandings();
        }

        private void UpdateNRR(TournamentStanding s)
        {
            float forRate = oversFaced[s.teamId] > 0 ? (runsScored[s.teamId] / oversFaced[s.teamId]) : 0f;
            float againstRate = oversBowled[s.teamId] > 0 ? (runsConceded[s.teamId] / oversBowled[s.teamId]) : 0f;
            s.netRunRate = forRate - againstRate;
        }

        public void SortStandings()
        {
            // Sort by Points descending, then NRR descending
            standings.Sort(CompareStandings);
        }

        private int CompareStandings(TournamentStanding a, TournamentStanding b)
        {
            if (b.points != a.points)
            {
                return b.points.CompareTo(a.points);
            }
            return b.netRunRate.CompareTo(a.netRunRate);
        }
    }
}
