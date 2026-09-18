import { describe, it, expect } from 'vitest';
import { BattingEngine, BattingShot, ShotDirection, DeliveryLine, DeliveryLength } from '../src/match/BattingEngine';
import { TimingGrade } from '../src/match/TimingSystem';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Shot Outcome Distributions', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Fakhar',
    lastName: 'Zaman',
    role: PlayerRole.BATSMAN,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  it('should guarantee safe dot ball for Block / Dead Bat on reasonable delivery', () => {
    const timing = { grade: TimingGrade.GOOD, offsetMs: 30, contactQuality: 0.8, description: 'Good' };
    const result = BattingEngine.resolveShot(
      BattingShot.BLOCK,
      { line: DeliveryLine.MIDDLE, length: DeliveryLength.GOOD_LENGTH, paceKph: 130, bowlerName: 'Bowler' },
      timing,
      dummyPlayer,
      ShotDirection.CENTER
    );

    expect(result.runs).toBe(0);
    expect(result.isWicket).toBe(false);
    expect(result.commentary).toContain('dead bat');
  });

  it('should punish leaving the ball on the middle stump with clean bowled dismissal', () => {
    const timing = { grade: TimingGrade.GOOD, offsetMs: 0, contactQuality: 1.0, description: 'Left' };
    const result = BattingEngine.resolveShot(
      BattingShot.LEAVE_BALL,
      { line: DeliveryLine.MIDDLE, length: DeliveryLength.GOOD_LENGTH, paceKph: 135, bowlerName: 'Shaheen' },
      timing,
      dummyPlayer,
      ShotDirection.CENTER
    );

    expect(result.isWicket).toBe(true);
    expect(result.runs).toBe(0);
    expect(result.commentary).toContain('Clean bowled');
  });

  it('should yield high rewards for Lofted Drive when timed with PERFECT precision', () => {
    const perfectTiming = { grade: TimingGrade.PERFECT, offsetMs: 0, contactQuality: 1.0, description: 'Sweet' };
    const result = BattingEngine.resolveShot(
      BattingShot.LOFTED_DRIVE,
      { line: DeliveryLine.MIDDLE, length: DeliveryLength.FULL, paceKph: 125, bowlerName: 'Spinner' },
      perfectTiming,
      dummyPlayer,
      ShotDirection.CENTER
    );

    expect(result.runs).toBe(6);
    expect(result.isBoundarySix).toBe(true);
    expect(result.isWicket).toBe(false);
  });
});
