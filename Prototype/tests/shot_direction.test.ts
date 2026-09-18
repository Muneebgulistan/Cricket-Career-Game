import { describe, it, expect } from 'vitest';
import { BattingEngine, BattingShot, ShotDirection, DeliveryLine, DeliveryLength } from '../src/match/BattingEngine';
import { TimingGrade } from '../src/match/TimingSystem';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Batting Directional Aiming', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Azam',
    lastName: 'Khan',
    role: PlayerRole.BATSMAN,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  const perfectTiming = {
    grade: TimingGrade.PERFECT,
    offsetMs: 0,
    contactQuality: 0.98,
    description: 'Perfect timing'
  };

  it('should award IDEAL suitability when shot, line, length, and direction all align', () => {
    // Cover drive on full outside off directed LEFT (off-side)
    const result = BattingEngine.resolveShot(
      BattingShot.COVER_DRIVE,
      { line: DeliveryLine.OUTSIDE_OFF, length: DeliveryLength.FULL, paceKph: 130, bowlerName: 'Bowler' },
      perfectTiming,
      dummyPlayer,
      ShotDirection.LEFT
    );

    expect(result.shotSuitability).toBe('IDEAL');
    expect(result.direction).toBe(ShotDirection.LEFT);
    expect(result.isBoundaryFour).toBe(true);
  });

  it('should penalize suitability when shot direction is mismatched with the stroke mechanics', () => {
    // Playing a cover drive towards the leg-side (RIGHT)
    const result = BattingEngine.resolveShot(
      BattingShot.COVER_DRIVE,
      { line: DeliveryLine.OUTSIDE_OFF, length: DeliveryLength.FULL, paceKph: 130, bowlerName: 'Bowler' },
      perfectTiming,
      dummyPlayer,
      ShotDirection.RIGHT
    );

    expect(result.shotSuitability).not.toBe('IDEAL');
  });

  it('should reward Pull Shot correctly when aimed through the leg-side (RIGHT)', () => {
    const result = BattingEngine.resolveShot(
      BattingShot.PULL_SHOT,
      { line: DeliveryLine.MIDDLE, length: DeliveryLength.SHORT, paceKph: 135, bowlerName: 'Bowler' },
      perfectTiming,
      dummyPlayer,
      ShotDirection.RIGHT
    );

    expect(result.shotSuitability).toBe('IDEAL');
    expect(result.runs).toBeGreaterThanOrEqual(4);
    expect(result.commentary).toContain('leg-side');
  });
});
