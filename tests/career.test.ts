import { describe, it, expect } from 'vitest';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';
import { CareerLevel, CAREER_LEVEL_ORDER } from '../src/career/CareerLevel';
import { CareerStatus } from '../src/career/CareerStatus';
import { CareerProgression } from '../src/career/CareerProgression';
import { SelectionManager } from '../src/career/SelectionManager';
import { PerformanceEvaluator } from '../src/career/PerformanceEvaluator';

describe('Career Subsystem', () => {
  it('should maintain strict sequential career ladder order', () => {
    expect(CAREER_LEVEL_ORDER).toEqual([
      CareerLevel.UNDER_16,
      CareerLevel.UNDER_19,
      CareerLevel.DOMESTIC,
      CareerLevel.COUNTRY_LEAGUE,
      CareerLevel.INTERNATIONAL_HOME,
      CareerLevel.INTERNATIONAL_AWAY,
      CareerLevel.TEST,
      CareerLevel.T20_WORLD_CUP,
      CareerLevel.ODI_WORLD_CUP
    ]);
  });

  it('should return correct previous and next career levels', () => {
    expect(CareerProgression.getPreviousLevel(CareerLevel.UNDER_16)).toBeNull();
    expect(CareerProgression.getNextLevel(CareerLevel.UNDER_16)).toBe(CareerLevel.UNDER_19);
    expect(CareerProgression.getNextLevel(CareerLevel.UNDER_19)).toBe(CareerLevel.DOMESTIC);
    expect(CareerProgression.getNextLevel(CareerLevel.ODI_WORLD_CUP)).toBeNull();
  });

  it('should NOT automatically promote without meeting required performance criteria', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Test',
      lastName: 'Player',
      nationality: 'India',
      role: PlayerRole.BATSMAN,
      battingStyle: BattingStyle.RIGHT_HAND
    });

    // Zero matches played
    const check1 = CareerProgression.checkPromotion(player, 0, 0, 0);
    expect(check1.canPromote).toBe(false);
    expect(check1.missingRequirements.length).toBeGreaterThan(0);
  });

  it('should allow promotion when criteria are fully satisfied', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Star',
      lastName: 'Batsman',
      nationality: 'Australia',
      role: PlayerRole.BATSMAN,
      battingStyle: BattingStyle.RIGHT_HAND
    });

    // Fulfill Under-16 criteria: 4+ matches, 100+ runs, 45+ rating, 55+ form
    player.potential.overallRating = 50;
    player.mental.form = 70;

    const check = CareerProgression.checkPromotion(player, 5, 180, 0);
    expect(check.canPromote).toBe(true);

    const promoted = CareerProgression.promote(player);
    expect(promoted).toBe(true);
    expect(player.career.currentCareerLevel).toBe(CareerLevel.UNDER_19);
    expect(player.career.careerStatus).toBe(CareerStatus.PROMOTED);
  });

  it('should drop player if fitness and form drop severely below threshold', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Slump',
      lastName: 'Cricketer',
      nationality: 'England',
      role: PlayerRole.BATSMAN,
      battingStyle: BattingStyle.RIGHT_HAND
    });

    player.mental.form = 15;
    player.mental.confidence = 10;
    player.mental.fitness = 35;

    const report = SelectionManager.evaluateSelection(player, [2.5, 3.0, 2.8]);
    expect(report.isDropped).toBe(true);
    expect(report.isSelected).toBe(false);
    expect(report.status).toBe(CareerStatus.DROPPED);
  });

  it('should evaluate performance and compute match ratings, form, and confidence', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Hero',
      lastName: 'Player',
      nationality: 'Pakistan',
      role: PlayerRole.BATSMAN,
      battingStyle: BattingStyle.RIGHT_HAND
    });

    const evalResult = PerformanceEvaluator.evaluateMatch(player, {
      runsScored: 82,
      ballsFaced: 58,
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
    expect(evalResult.xpGained).toBeGreaterThan(0);
  });
});
