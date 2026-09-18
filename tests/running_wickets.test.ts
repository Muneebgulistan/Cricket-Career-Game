import { describe, it, expect } from 'vitest';
import { RunningSystem } from '../src/match/RunningSystem';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Running Between Wickets', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Mohammad',
    lastName: 'Rizwan',
    role: PlayerRole.WICKETKEEPER,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  it('should identify extra run opportunity on strokes placed in the deep', () => {
    const opp = RunningSystem.evaluateRunningOpportunity(1, 'LEFT', false);
    expect(opp).not.toBeNull();
    expect(opp?.canTakeExtraRun).toBe(true);
    expect(opp?.baseRunsScored).toBe(1);
  });

  it('should NOT offer extra run on boundary shots', () => {
    const opp = RunningSystem.evaluateRunningOpportunity(4, 'LEFT', true);
    expect(opp).toBeNull();
  });

  it('should keep batter safe when deciding to STAY', () => {
    const opp = {
      baseRunsScored: 1,
      canTakeExtraRun: true,
      extraRunRisk: 'HIGH' as const,
      fielderDistanceMeters: 25,
      throwTargetEnd: 'STRIKER' as const,
      commentaryHint: 'Tight throw incoming'
    };

    const outcome = RunningSystem.resolveRunningDecision(opp, false, dummyPlayer);
    expect(outcome.runsCompleted).toBe(1);
    expect(outcome.isRunOut).toBe(false);
    expect(outcome.commentary).toContain('safe');
  });

  it('should resolve extra run push with either extra run scored or a run out', () => {
    const opp = {
      baseRunsScored: 1,
      canTakeExtraRun: true,
      extraRunRisk: 'LOW' as const,
      fielderDistanceMeters: 55,
      throwTargetEnd: 'NON_STRIKER' as const,
      commentaryHint: 'Fielder is deep'
    };

    const outcome = RunningSystem.resolveRunningDecision(opp, true, dummyPlayer);
    if (outcome.isRunOut) {
      expect(outcome.runsCompleted).toBe(1);
      expect(outcome.commentary).toContain('OUT');
    } else {
      expect(outcome.runsCompleted).toBe(2);
      expect(outcome.commentary).toContain('Superb hustle');
    }
  });
});
