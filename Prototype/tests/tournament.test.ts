import { describe, it, expect } from 'vitest';
import { TournamentManager } from '../src/tournament/TournamentManager';
import { CareerLevel } from '../src/career/CareerLevel';
import { TournamentStatus } from '../src/tournament/TournamentModel';

describe('Tournament Subsystem', () => {
  it('should generate a tournament instance for Under-16 level with fixtures and standings', () => {
    const tourney = TournamentManager.createTournamentForLevel(CareerLevel.UNDER_16, 'Pakistan');

    expect(tourney.id).toBeDefined();
    expect(tourney.careerLevel).toBe(CareerLevel.UNDER_16);
    expect(tourney.status).toBe(TournamentStatus.IN_PROGRESS);
    expect(tourney.fixtures.length).toBeGreaterThan(0);
    expect(tourney.standings.length).toBeGreaterThan(0);
    expect(tourney.completedMatches).toBe(0);
  });

  it('should retrieve next unplayed fixture accurately', () => {
    const tourney = TournamentManager.createTournamentForLevel(CareerLevel.UNDER_16, 'India');
    const nextFixture = TournamentManager.getNextFixture(tourney);

    expect(nextFixture).not.toBeNull();
    expect(nextFixture?.isPlayed).toBe(false);
    expect(nextFixture?.roundNumber).toBe(1);
  });

  it('should update standings and complete tournament when all matches finish', () => {
    const tourney = TournamentManager.createTournamentForLevel(CareerLevel.UNDER_16, 'India');
    const firstFixture = tourney.fixtures[0];

    TournamentManager.recordMatchResult(tourney, firstFixture.id, firstFixture.teamAId, 'Won by 25 runs');

    expect(firstFixture.isPlayed).toBe(true);
    expect(firstFixture.winnerTeamId).toBe(firstFixture.teamAId);
    expect(tourney.completedMatches).toBe(1);

    const winnerStanding = tourney.standings.find(s => s.teamId === firstFixture.teamAId);
    expect(winnerStanding?.won).toBe(1);
    expect(winnerStanding?.points).toBe(2);
  });
});
