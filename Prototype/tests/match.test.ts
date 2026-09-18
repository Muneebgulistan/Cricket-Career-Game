import { describe, it, expect } from 'vitest';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';
import { CareerLevel } from '../src/career/CareerLevel';
import { TournamentManager } from '../src/tournament/TournamentManager';
import { MatchSimulator } from '../src/match/MatchSimulator';
import { CareerStatisticsTracker } from '../src/player/CareerStatistics';

describe('Match Subsystem', () => {
  it('should initialize and simulate a match with valid scorecards and player performance', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Rohan',
      lastName: 'Gavaskar',
      nationality: 'India',
      role: PlayerRole.ALL_ROUNDER,
      battingStyle: BattingStyle.LEFT_HAND
    });

    const tournament = TournamentManager.createTournamentForLevel(CareerLevel.UNDER_16, 'India');
    const fixture = TournamentManager.getNextFixture(tournament)!;

    const match = MatchSimulator.initializeMatch(player, tournament, fixture);
    expect(match.id).toBeDefined();
    expect(match.isCompleted).toBe(false);

    MatchSimulator.simulateFullMatch(match, player);

    expect(match.isCompleted).toBe(true);
    expect(match.winnerTeamId).toBeDefined();
    expect(match.resultSummary).toBeDefined();
    expect(match.innings[0].isCompleted).toBe(true);
    expect(match.innings[1].isCompleted).toBe(true);
    expect(match.commentaryLog.length).toBeGreaterThan(0);

    // Player performance was tracked and evaluated
    expect(match.playerPerformance.evaluation).toBeDefined();
    expect(match.playerPerformance.evaluation?.matchRating).toBeGreaterThanOrEqual(1.0);
    expect(match.playerPerformance.evaluation?.matchRating).toBeLessThanOrEqual(10.0);
  });

  it('should accurately update career statistics across multiple matches', () => {
    const statsContainer = CareerStatisticsTracker.createInitialContainer();

    // Match 1: 54 runs, 1 wicket
    CareerStatisticsTracker.recordMatch(
      statsContainer,
      CareerLevel.UNDER_16,
      'tourn_1',
      'Under-16 Cup',
      {
        runs: 54,
        balls: 45,
        fours: 6,
        sixes: 1,
        isOut: true,
        didBat: true,
        overs: 4,
        maidens: 0,
        runsConceded: 22,
        wickets: 1,
        didBowl: true,
        catches: 1,
        runOuts: 0,
        stumpings: 0
      }
    );

    // Match 2: 72 runs not out, 2 wickets
    CareerStatisticsTracker.recordMatch(
      statsContainer,
      CareerLevel.UNDER_16,
      'tourn_1',
      'Under-16 Cup',
      {
        runs: 72,
        balls: 50,
        fours: 8,
        sixes: 2,
        isOut: false,
        didBat: true,
        overs: 4,
        maidens: 1,
        runsConceded: 18,
        wickets: 2,
        didBowl: true,
        catches: 0,
        runOuts: 0,
        stumpings: 0
      }
    );

    expect(statsContainer.allTime.batting.matches).toBe(2);
    expect(statsContainer.allTime.batting.runs).toBe(126);
    expect(statsContainer.allTime.batting.highestScore).toBe(72);
    expect(statsContainer.allTime.batting.isHighestScoreNotOut).toBe(true);
    expect(statsContainer.allTime.batting.fifties).toBe(2);
    expect(statsContainer.allTime.batting.average).toBe(126); // 126 runs / 1 dismissal = 126.0
    expect(statsContainer.allTime.bowling.wickets).toBe(3);
    expect(statsContainer.allTime.fielding.catches).toBe(1);

    // Level-specific breakdown verification
    expect(statsContainer.byLevel[CareerLevel.UNDER_16]).toBeDefined();
    expect(statsContainer.byLevel[CareerLevel.UNDER_16].batting.runs).toBe(126);

    // Tournament breakdown verification
    expect(statsContainer.byTournament.length).toBe(1);
    expect(statsContainer.byTournament[0].batting.runs).toBe(126);
  });
});
