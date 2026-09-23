using System;
using System.Collections.Generic;
using UnityEngine;
using CricketGame.Cricket;
using CricketGame.Tournaments;

namespace CricketGame.Career.Tournaments
{
    public class TournamentManager : MonoBehaviour
    {
        public static TournamentManager Instance { get; private set; }

        [SerializeField] private TournamentProgress activeTournament;
        private TournamentTable standingsTable;

        public TournamentProgress ActiveTournament { get { return activeTournament; } }
        public TournamentTable Table { get { return standingsTable; } }

        public event Action<TournamentProgress> OnTournamentUpdated;
        public event Action<string> OnTournamentWon;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (activeTournament == null)
            {
                InitializeUnder16Cup();
            }
        }

        public void InitializeUnder16Cup()
        {
            TournamentDefinition def = TournamentDefinition.CreateUnder16NationalCup();
            activeTournament = new TournamentProgress();
            activeTournament.tournamentId = def.tournamentId;
            activeTournament.tournamentName = def.tournamentName;
            activeTournament.level = def.level;
            activeTournament.format = def.format;
            activeTournament.totalOvers = def.totalOvers;
            activeTournament.fixtures = new List<FixtureData>();
            activeTournament.standings = new List<TournamentStanding>();
            activeTournament.currentFixtureIndex = 0;
            activeTournament.isCompleted = false;

            standingsTable = new TournamentTable(def.teamNames);
            activeTournament.standings = standingsTable.standings;

            // Generate Round-Robin fixtures between 4 teams
            int fixId = 1;
            for (int i = 0; i < def.teamNames.Count; i++)
            {
                for (int j = i + 1; j < def.teamNames.Count; j++)
                {
                    FixtureData f = new FixtureData();
                    f.fixtureId = string.Format("FIX_{0}", fixId++);
                    f.homeTeamId = def.teamNames[i];
                    f.homeTeamName = def.teamNames[i];
                    f.awayTeamId = def.teamNames[j];
                    f.awayTeamName = def.teamNames[j];
                    f.format = def.format;
                    f.roundNumber = fixId <= 3 ? 1 : 2;
                    f.isCompleted = false;
                    f.winnerTeamId = string.Empty;
                    f.resultSummary = "Scheduled";
                    activeTournament.fixtures.Add(f);
                }
            }

            if (OnTournamentUpdated != null)
            {
                OnTournamentUpdated(activeTournament);
            }
        }

        public FixtureData GetNextUserFixture(string userTeamName)
        {
            if (activeTournament == null || activeTournament.fixtures == null) return null;

            for (int i = activeTournament.currentFixtureIndex; i < activeTournament.fixtures.Count; i++)
            {
                var f = activeTournament.fixtures[i];
                if (!f.isCompleted && (f.homeTeamName == userTeamName || f.awayTeamName == userTeamName))
                {
                    return f;
                }
            }
            return null;
        }

        public void RecordUserMatchResult(string fixtureId, string winnerTeam, int homeRuns, float homeOvers, int awayRuns, float awayOvers)
        {
            if (activeTournament == null) return;

            FixtureData match = null;
            for (int i = 0; i < activeTournament.fixtures.Count; i++)
            {
                if (activeTournament.fixtures[i].fixtureId == fixtureId)
                {
                    match = activeTournament.fixtures[i];
                    break;
                }
            }

            if (match != null)
            {
                match.isCompleted = true;
                match.winnerTeamId = winnerTeam;
                match.resultSummary = string.Format("{0} won the match", winnerTeam);

                if (standingsTable == null) standingsTable = new TournamentTable();
                standingsTable.standings = activeTournament.standings;
                standingsTable.RecordMatch(match.homeTeamId, homeRuns, homeOvers, match.awayTeamId, awayRuns, awayOvers, winnerTeam);

                // Simulate other fixtures in current round
                SimulateOtherPendingFixtures(fixtureId);

                activeTournament.currentFixtureIndex++;
                if (activeTournament.currentFixtureIndex >= activeTournament.fixtures.Count)
                {
                    activeTournament.isCompleted = true;
                    if (standingsTable.standings.Count > 0)
                    {
                        string champion = standingsTable.standings[0].teamName;
                        if (OnTournamentWon != null) OnTournamentWon(champion);
                    }
                }

                if (OnTournamentUpdated != null)
                {
                    OnTournamentUpdated(activeTournament);
                }
            }
        }

        private void SimulateOtherPendingFixtures(string completedFixtureId)
        {
            int seed = 100;
            for (int i = 0; i < activeTournament.fixtures.Count; i++)
            {
                var f = activeTournament.fixtures[i];
                if (!f.isCompleted && f.fixtureId != completedFixtureId && f.roundNumber == 1)
                {
                    TournamentSimulator.SimulateFixture(f, seed++);
                    standingsTable.RecordMatch(f.homeTeamId, 150, 20f, f.awayTeamId, 140, 20f, f.winnerTeamId);
                }
            }
        }
    }
}
