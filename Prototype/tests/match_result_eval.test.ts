import { describe, it, expect } from 'vitest';
import { MatchSimulator } from '../src/match/MatchSimulator';
import { TournamentManager } from '../src/tournament/TournamentManager';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';
import { CareerLevel } from '../src/career/CareerLevel';
import { PerformanceEvaluator } from '../src/career/PerformanceEvaluator';

describe('Step 3 — Match Result & Player Performance Evaluation', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Zayn',
    lastName: 'Malik',
    role: PlayerRole.BATSMAN,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  const tournament = TournamentManager.createTournamentForLevel(CareerLevel.UNDER_16, 'Pakistan');
  const fixture = TournamentManager.getNextFixture(tournament)!;

  it('should evaluate match performance and assign match ratings', () => {
    const evalResult = PerformanceEvaluator.evaluateMatch(dummyPlayer, {
      runsScored: 74,
      ballsFaced: 48,
      fours: 8,
      sixes: 2,
      isOut: false,
      oversBowled: 0,
      maidens: 0,
      runsConceded: 0,
      wicketsTaken: 0,
      catches: 1,
      runOuts: 0,
      stumpings: 0,
      matchResult: 'WIN',
      isManOfTheMatch: true
    });

    expect(evalResult.matchRating).toBeGreaterThanOrEqual(8.0);
    expect(evalResult.formDelta).toBeGreaterThan(0);
    expect(evalResult.confidenceDelta).toBeGreaterThan(0);
    expect(evalResult.xpGained).toBeGreaterThan(50);
  });

  it('should simulate full match and conclude with winner and scorecard', () => {
    const match = MatchSimulator.initializeMatch(dummyPlayer, tournament, fixture);
    MatchSimulator.simulateFullMatch(match, dummyPlayer);

    expect(match.isCompleted).toBe(true);
    expect(match.winnerTeamId).toBeDefined();
    expect(match.resultSummary).toBeDefined();
    expect(match.innings[0].isCompleted).toBe(true);
    expect(match.innings[1].isCompleted).toBe(true);
  });
});
