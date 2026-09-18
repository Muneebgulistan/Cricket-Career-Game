import { describe, it, expect } from 'vitest';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle, BowlingStyle } from '../src/player/PlayerModel';
import { RatingCalculator } from '../src/player/RatingCalculator';
import { CareerLevel } from '../src/career/CareerLevel';
import { CareerStatus } from '../src/career/CareerStatus';

describe('Player Subsystem', () => {
  it('should initialize a young cricketer at Under-16 level with Youth Player status', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Zayn',
      lastName: 'Khan',
      nationality: 'Pakistan',
      role: PlayerRole.BATSMAN,
      battingStyle: BattingStyle.RIGHT_HAND,
      jerseyNumber: 18
    });

    expect(player.id).toBeDefined();
    expect(player.firstName).toBe('Zayn');
    expect(player.lastName).toBe('Khan');
    expect(player.age).toBe(15);
    expect(player.nationality).toBe('Pakistan');
    expect(player.jerseyNumber).toBe(18);
    expect(player.career.currentCareerLevel).toBe(CareerLevel.UNDER_16);
    expect(player.career.careerStatus).toBe(CareerStatus.YOUTH_PLAYER);
    expect(player.potential.overallRating).toBeGreaterThanOrEqual(40);
    expect(player.potential.potentialRating).toBeGreaterThan(player.potential.overallRating);
  });

  it('should calculate role-specific starting attributes accurately', () => {
    const batsman = PlayerFactory.createUnder16Player({
      firstName: 'Aarav',
      lastName: 'Patel',
      nationality: 'India',
      role: PlayerRole.BATSMAN,
      battingStyle: BattingStyle.RIGHT_HAND
    });

    const bowler = PlayerFactory.createUnder16Player({
      firstName: 'James',
      lastName: 'Anderson',
      nationality: 'England',
      role: PlayerRole.BOWLER,
      battingStyle: BattingStyle.RIGHT_HAND,
      bowlingStyle: BowlingStyle.RIGHT_ARM_FAST
    });

    expect(batsman.batting.battingAbility).toBeGreaterThan(bowler.batting.battingAbility);
    expect(bowler.bowling.bowlingAbility).toBeGreaterThan(batsman.bowling.bowlingAbility);
  });

  it('should calculate overall rating based on weighted attributes', () => {
    const overall = RatingCalculator.calculateOverall({
      role: PlayerRole.BATSMAN,
      batting: { battingAbility: 80, battingTechnique: 80, timing: 80, power: 75, runningBetweenWickets: 70 },
      bowling: { bowlingAbility: 20, pace: 20, swing: 20, seam: 20, spin: 20, accuracy: 20, variation: 20 },
      fielding: { fielding: 70, catching: 75, throwing: 70, reflexes: 70 },
      mental: { confidence: 80, fitness: 80, form: 80, pressureHandling: 80 }
    });

    expect(overall).toBeGreaterThanOrEqual(75);
    expect(overall).toBeLessThanOrEqual(85);
  });
});
