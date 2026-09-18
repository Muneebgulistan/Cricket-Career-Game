import { describe, it, expect } from 'vitest';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle, BowlingStyle } from '../src/player/PlayerModel';
import { CareerLevel } from '../src/career/CareerLevel';
import { CareerStatus } from '../src/career/CareerStatus';
import { TournamentManager } from '../src/tournament/TournamentManager';
import { MatchSimulator } from '../src/match/MatchSimulator';
import { CareerStatisticsTracker } from '../src/player/CareerStatistics';
import { CareerProgression } from '../src/career/CareerProgression';
import { SelectionManager } from '../src/career/SelectionManager';
import { SaveSystem } from '../src/save/SaveSystem';

describe('End-to-End Playable Career Loop', () => {
  it('should progress through the full player career loop seamlessly', () => {
    // 1. Create a 15-year-old All-Rounder
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Zayn',
      lastName: 'Khan',
      nationality: 'Pakistan',
      role: PlayerRole.ALL_ROUNDER,
      battingStyle: BattingStyle.RIGHT_HAND,
      bowlingStyle: BowlingStyle.RIGHT_ARM_FAST,
      jerseyNumber: 18
    });

    expect(player.career.currentCareerLevel).toBe(CareerLevel.UNDER_16);
    expect(player.career.careerStatus).toBe(CareerStatus.YOUTH_PLAYER);
    expect(player.age).toBe(15);

    // 2. Start Under-16 Tournament
    const tournament = TournamentManager.createTournamentForLevel(player.career.currentCareerLevel, player.nationality);
    expect(tournament.careerLevel).toBe(CareerLevel.UNDER_16);
    expect(tournament.fixtures.length).toBe(5);

    const stats = CareerStatisticsTracker.createInitialContainer();
    const recentRatings: number[] = [];

    // 3. Play / Simulate each match in the Under-16 tournament
    for (let i = 0; i < tournament.fixtures.length; i++) {
      const fixture = TournamentManager.getNextFixture(tournament);
      expect(fixture).not.toBeNull();
      if (!fixture) break;

      // Selection check before match
      const selection = SelectionManager.evaluateSelection(player, recentRatings);
      expect(selection.isSelected).toBe(true);

      // Match simulation
      const match = MatchSimulator.initializeMatch(player, tournament, fixture);
      MatchSimulator.simulateFullMatch(match, player);

      expect(match.isCompleted).toBe(true);
      const evalResult = match.playerPerformance.evaluation!;
      expect(evalResult).toBeDefined();

      recentRatings.push(evalResult.matchRating);

      // Update player mental state
      player.mental.form = Math.max(1, Math.min(99, player.mental.form + evalResult.formDelta));
      player.mental.confidence = Math.max(1, Math.min(99, player.mental.confidence + evalResult.confidenceDelta));

      // Update stats
      CareerStatisticsTracker.recordMatch(
        stats,
        player.career.currentCareerLevel,
        tournament.id,
        tournament.name,
        {
          runs: match.playerPerformance.runs,
          balls: match.playerPerformance.balls,
          fours: match.playerPerformance.fours,
          sixes: match.playerPerformance.sixes,
          isOut: match.playerPerformance.isOut,
          didBat: match.playerPerformance.didBat,
          overs: match.playerPerformance.overs,
          maidens: match.playerPerformance.maidens,
          runsConceded: match.playerPerformance.runsConceded,
          wickets: match.playerPerformance.wickets,
          didBowl: match.playerPerformance.didBowl,
          catches: match.playerPerformance.catches,
          runOuts: match.playerPerformance.runOuts,
          stumpings: match.playerPerformance.stumpings
        }
      );

      // Record in tournament
      TournamentManager.recordMatchResult(
        tournament,
        fixture.id,
        match.winnerTeamId || '',
        match.resultSummary || 'Finished'
      );
    }

    // 4. Tournament is completed
    expect(tournament.completedMatches).toBe(5);
    expect(tournament.status).toBe('COMPLETED');
    expect(stats.allTime.batting.matches).toBe(5);

    // 5. Verification of Career Statistics by level
    const u16Stats = stats.byLevel[CareerLevel.UNDER_16];
    expect(u16Stats).toBeDefined();
    expect(u16Stats.batting.matches).toBe(5);

    // 6. Test promotion eligibility check
    // Train player's rating & ensure form meets threshold
    player.potential.overallRating = 55;
    player.mental.form = 75;

    const promoCheck = CareerProgression.checkPromotion(
      player,
      u16Stats.batting.matches,
      Math.max(u16Stats.batting.runs, 120),
      Math.max(u16Stats.bowling.wickets, 5)
    );
    expect(promoCheck.canPromote).toBe(true);
    expect(promoCheck.nextLevel).toBe(CareerLevel.UNDER_19);

    // 7. Execute Promotion
    const promoted = CareerProgression.promote(player);
    expect(promoted).toBe(true);
    expect(player.career.currentCareerLevel).toBe(CareerLevel.UNDER_19);
    expect(player.career.careerStatus).toBe(CareerStatus.PROMOTED);
    expect(player.age).toBeGreaterThanOrEqual(17); // Age adjusted for national U19

    // 8. Verify previous statistics were NOT lost upon promotion
    expect(stats.allTime.batting.matches).toBe(5);
    expect(stats.byLevel[CareerLevel.UNDER_16].batting.matches).toBe(5);

    // 9. Save and Load verification
    const slot = 'e2e_career_test';
    const saved = SaveSystem.saveGame(slot, {
      player,
      tournament,
      statistics: stats,
      recentRatings
    });
    expect(saved).toBe(true);

    const loaded = SaveSystem.loadGame(slot);
    expect(loaded).not.toBeNull();
    expect(loaded?.player.career.currentCareerLevel).toBe(CareerLevel.UNDER_19);
    expect(loaded?.statistics.allTime.batting.matches).toBe(5);
    expect(loaded?.statistics.byLevel[CareerLevel.UNDER_16].batting.matches).toBe(5);

    SaveSystem.deleteSave(slot);
  });
});
