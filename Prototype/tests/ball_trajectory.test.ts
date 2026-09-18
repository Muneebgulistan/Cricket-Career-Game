import { describe, it, expect } from 'vitest';
import { BallPhysics } from '../src/match/BallPhysics';
import { DeliveryLine, DeliveryLength } from '../src/match/BattingEngine';
import { BowlingVariation } from '../src/match/BowlingEngine';
import { BowlingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Ball Trajectory Engine', () => {
  it('should map delivery lines to precise pitch lateral coordinates', () => {
    expect(BallPhysics.lineToMeters(DeliveryLine.MIDDLE)).toBe(0.0);
    expect(BallPhysics.lineToMeters(DeliveryLine.OUTSIDE_OFF)).toBeLessThan(0);
    expect(BallPhysics.lineToMeters(DeliveryLine.WIDE_OUTSIDE_OFF)).toBeLessThan(BallPhysics.lineToMeters(DeliveryLine.OUTSIDE_OFF));
    expect(BallPhysics.lineToMeters(DeliveryLine.LEG_STUMP)).toBeGreaterThan(0);
  });

  it('should map delivery lengths to correct pitch bounce distances', () => {
    const yorkerDist = BallPhysics.lengthToMeters(DeliveryLength.YORKER);
    const goodLengthDist = BallPhysics.lengthToMeters(DeliveryLength.GOOD_LENGTH);
    const bouncerDist = BallPhysics.lengthToMeters(DeliveryLength.BOUNCER);

    // Yorker lands closest to batsman (20.12m), bouncer lands closest to bowler (0m)
    expect(yorkerDist).toBeGreaterThan(goodLengthDist);
    expect(goodLengthDist).toBeGreaterThan(bouncerDist);
  });

  it('should compute continuous sampled 3D points from bowler release to batsman crease', () => {
    const trajectory = BallPhysics.calculateTrajectory(
      DeliveryLine.OUTSIDE_OFF,
      DeliveryLength.GOOD_LENGTH,
      BowlingVariation.NORMAL_PACE,
      BowlingStyle.RIGHT_ARM_FAST,
      'BALANCED',
      140
    );

    expect(trajectory.points.length).toBeGreaterThan(15);

    // Initial point at release
    const first = trajectory.points[0];
    expect(first.y).toBe(0);
    expect(first.z).toBeGreaterThan(1.8);
    expect(first.isBounced).toBe(false);

    // Final point at batting crease
    const last = trajectory.points[trajectory.points.length - 1];
    expect(last.y).toBeGreaterThanOrEqual(BallPhysics.PITCH_LENGTH_METERS);
    expect(last.isBounced).toBe(true);

    // Bounce occurred at expected range
    expect(trajectory.bouncePoint.y).toBeGreaterThan(8);
    expect(trajectory.bouncePoint.y).toBeLessThan(18);
    expect(trajectory.bounceTimeMs).toBeGreaterThan(100);
    expect(trajectory.totalDurationMs).toBeGreaterThan(trajectory.bounceTimeMs);
  });
});
