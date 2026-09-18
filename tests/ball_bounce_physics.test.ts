import { describe, it, expect } from 'vitest';
import { BallPhysics } from '../src/match/BallPhysics';
import { DeliveryLine, DeliveryLength } from '../src/match/BattingEngine';
import { BowlingVariation } from '../src/match/BowlingEngine';
import { BowlingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Ball Bounce & Pitch Physics', () => {
  it('should differentiate styles: Fast bowlers have shorter reaction times than Spinners', () => {
    const fastTraj = BallPhysics.calculateTrajectory(
      DeliveryLine.MIDDLE,
      DeliveryLength.GOOD_LENGTH,
      BowlingVariation.NORMAL_PACE,
      BowlingStyle.RIGHT_ARM_FAST,
      'BALANCED',
      145
    );

    const spinTraj = BallPhysics.calculateTrajectory(
      DeliveryLine.MIDDLE,
      DeliveryLength.GOOD_LENGTH,
      BowlingVariation.NORMAL_SPIN,
      BowlingStyle.RIGHT_ARM_OFF_SPIN,
      'BALANCED',
      85
    );

    expect(fastTraj.totalDurationMs).toBeLessThan(spinTraj.totalDurationMs);
    expect(fastTraj.arrivalPaceKph).toBeGreaterThan(spinTraj.arrivalPaceKph);
  });

  it('should simulate outswing lateral curve during flight', () => {
    const outswing = BallPhysics.calculateTrajectory(
      DeliveryLine.MIDDLE,
      DeliveryLength.FULL,
      BowlingVariation.OUTSWING,
      BowlingStyle.RIGHT_ARM_FAST,
      'BALANCED',
      135
    );

    expect(outswing.swingAngleDeg).not.toBe(0);
  });

  it('should amplify spin turn on dusty dry pitches', () => {
    const normalSpin = BallPhysics.calculateTrajectory(
      DeliveryLine.OUTSIDE_OFF,
      DeliveryLength.GOOD_LENGTH,
      BowlingVariation.SHARP_TURN,
      BowlingStyle.RIGHT_ARM_OFF_SPIN,
      'BALANCED',
      88
    );

    const dustySpin = BallPhysics.calculateTrajectory(
      DeliveryLine.OUTSIDE_OFF,
      DeliveryLength.GOOD_LENGTH,
      BowlingVariation.SHARP_TURN,
      BowlingStyle.RIGHT_ARM_OFF_SPIN,
      'DUSTY_SPIN',
      88
    );

    expect(Math.abs(dustySpin.spinTurnDeg)).toBeGreaterThanOrEqual(Math.abs(normalSpin.spinTurnDeg));
  });

  it('should correctly detect if ball trajectory intersects stumps', () => {
    const hitTarget = { x: 0.05, y: 20.12, z: 0.35 };
    const missTargetWide = { x: 0.85, y: 20.12, z: 0.35 };
    const missTargetHigh = { x: 0.0, y: 20.12, z: 1.2 };

    expect(BallPhysics.isHittingStumps(hitTarget)).toBe(true);
    expect(BallPhysics.isHittingStumps(missTargetWide)).toBe(false);
    expect(BallPhysics.isHittingStumps(missTargetHigh)).toBe(false);
  });
});
