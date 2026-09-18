import { describe, it, expect } from 'vitest';
import { DifficultyLevel, DIFFICULTY_CONFIGS } from '../src/career/Difficulty';
import { TimingSystem, TimingGrade } from '../src/match/TimingSystem';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Gameplay Difficulty Scaling', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Test',
    lastName: 'Player',
    role: PlayerRole.BATSMAN,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  it('should configure wider timing and reaction windows on EASY than HARD', () => {
    const easyCfg = DIFFICULTY_CONFIGS[DifficultyLevel.EASY];
    const normalCfg = DIFFICULTY_CONFIGS[DifficultyLevel.NORMAL];
    const hardCfg = DIFFICULTY_CONFIGS[DifficultyLevel.HARD];

    expect(easyCfg.timingWindowMs).toBeGreaterThan(normalCfg.timingWindowMs);
    expect(normalCfg.timingWindowMs).toBeGreaterThan(hardCfg.timingWindowMs);

    expect(easyCfg.fieldingReactionWindowMs).toBeGreaterThan(normalCfg.fieldingReactionWindowMs);
    expect(normalCfg.fieldingReactionWindowMs).toBeGreaterThan(hardCfg.fieldingReactionWindowMs);
  });

  it('should be more forgiving of slight timing offsets on EASY difficulty', () => {
    const easyEval = TimingSystem.evaluateOffset(70, dummyPlayer, DifficultyLevel.EASY);
    expect(easyEval.grade).toBe(TimingGrade.PERFECT);

    const hardEval = TimingSystem.evaluateOffset(70, dummyPlayer, DifficultyLevel.HARD);
    expect(hardEval.grade).not.toBe(TimingGrade.PERFECT);
  });
});
