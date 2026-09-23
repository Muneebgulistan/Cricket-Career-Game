using System;
using UnityEngine;
using CricketGame.Tournaments;

namespace CricketGame.Career.Tournaments
{
    public static class TournamentSimulator
    {
        public static FixtureData SimulateFixture(FixtureData fixture, int randomSeed)
        {
            if (fixture == null) return null;

            System.Random rng = new System.Random(randomSeed);

            // Innings 1: 130 - 180 runs, 4 - 8 wickets
            int inn1Runs = rng.Next(130, 185);
            int inn1Wickets = rng.Next(4, 9);
            float inn1Overs = 20.0f;

            // Innings 2: chasing target = inn1Runs + 1
            int target = inn1Runs + 1;
            bool chaserWins = rng.Next(0, 100) < 52; // 52% chasing win rate in T20

            int inn2Runs;
            int inn2Wickets;
            float inn2Overs;
            string winnerId;
            string summary;

            if (chaserWins)
            {
                inn2Runs = target + rng.Next(0, 4);
                inn2Wickets = rng.Next(3, 8);
                inn2Overs = 18.0f + (float)(rng.NextDouble() * 1.8f);
                winnerId = fixture.awayTeamId;
                int wicketsRemaining = 10 - inn2Wickets;
                summary = string.Format("{0} won by {1} wickets ({2}/{3} vs {4}/{5})",
                    fixture.awayTeamName, wicketsRemaining, inn2Runs, inn2Wickets, inn1Runs, inn1Wickets);
            }
            else
            {
                inn2Runs = inn1Runs - rng.Next(4, 25);
                inn2Wickets = rng.Next(6, 10);
                inn2Overs = 20.0f;
                winnerId = fixture.homeTeamId;
                int runMargin = inn1Runs - inn2Runs;
                summary = string.Format("{0} won by {1} runs ({2}/{3} vs {4}/{5})",
                    fixture.homeTeamName, runMargin, inn1Runs, inn1Wickets, inn2Runs, inn2Wickets);
            }

            fixture.isCompleted = true;
            fixture.winnerTeamId = winnerId;
            fixture.resultSummary = summary;

            return fixture;
        }
    }
}
