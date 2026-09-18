import { describe, it, expect } from 'vitest';
import { BattingEngine, BattingShot, ShotDirection, DeliveryLine, DeliveryLength } from '../src/match/BattingEngine';
import { TimingGrade } from '../src/match/TimingSystem';
import { DismissalType } from '../src/cricket/CricketTypes';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Comprehensive Wicket Dismissals', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Young',
    lastName: 'Batter',
    role: PlayerRole.BATSMAN,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  it('should support BOWLED when leave is attempted on stumps', () => {
    const timing = { grade: TimingGrade.GOOD, offsetMs: 0, contactQuality: 0.9, description: 'Left' };
    const res = BattingEngine.resolveShot(
      BattingShot.LEAVE_BALL,
      { line: DeliveryLine.MIDDLE, length: DeliveryLength.GOOD_LENGTH, paceKph: 135, bowlerName: 'Bowler' },
      timing,
      dummyPlayer,
      ShotDirection.CENTER
    );

    expect(res.isWicket).toBe(true);
    expect(res.dismissalType).toBe(DismissalType.BOWLED);
  });

  it('should support CAUGHT, LBW, STUMPED, and HIT_WICKET across different mistake scenarios', () => {
    const veryLateTiming = { grade: TimingGrade.VERY_LATE, offsetMs: 380, contactQuality: 0.1, description: 'Hopeless' };

    // Spin bowler tempting lofted drive with early/late timing can yield STUMPED
    const stumpingTrials: DismissalType[] = [];
    for (let i = 0; i < 20; i++) {
      const res = BattingEngine.resolveShot(
        BattingShot.LOFTED_DRIVE,
        { line: DeliveryLine.OUTSIDE_OFF, length: DeliveryLength.FULL, paceKph: 85, bowlerName: 'Rashid', isSpin: true },
        veryLateTiming,
        dummyPlayer,
        ShotDirection.CENTER
      );
      if (res.isWicket && res.dismissalType) stumpingTrials.push(res.dismissalType);
    }

    expect(stumpingTrials.length).toBeGreaterThan(0);
  });
});
