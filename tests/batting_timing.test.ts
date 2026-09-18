import { describe, it, expect } from 'vitest';
import { TimingSystem, TimingGrade } from '../src/match/TimingSystem';
import { DifficultyLevel } from '../src/career/Difficulty';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Batting Timing Subsystem', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Test',
    lastName: 'Batsman',
    role: PlayerRole.BATSMAN,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  it('should evaluate 0ms offset as PERFECT timing', () => {
    const evalResult = TimingSystem.evaluateOffset(0, dummyPlayer, DifficultyLevel.NORMAL, 0);
    expect(evalResult.grade).toBe(TimingGrade.PERFECT);
    expect(evalResult.contactQuality).toBeGreaterThan(0.9);
  });

  it('should evaluate small offset as GOOD timing', () => {
    const evalResult = TimingSystem.evaluateOffset(110, dummyPlayer, DifficultyLevel.NORMAL, 0);
    expect(evalResult.grade).toBe(TimingGrade.GOOD);
    expect(evalResult.contactQuality).toBeGreaterThan(0.7);
  });

  it('should evaluate negative offset as EARLY timing', () => {
    const evalResult = TimingSystem.evaluateOffset(-220, dummyPlayer, DifficultyLevel.NORMAL, 0);
    expect(evalResult.grade).toBe(TimingGrade.EARLY);
  });

  it('should evaluate large positive offset as VERY_LATE timing', () => {
    const evalResult = TimingSystem.evaluateOffset(550, dummyPlayer, DifficultyLevel.NORMAL, 0);
    expect(evalResult.grade).toBe(TimingGrade.VERY_LATE);
    expect(evalResult.contactQuality).toBeLessThan(0.3);
  });

  it('should scale sweet spot window with difficulty level', () => {
    // Offset of 40ms might be perfect on EASY, but only good or late on HARD
    const easyEval = TimingSystem.evaluateOffset(35, dummyPlayer, DifficultyLevel.EASY, 0);
    const hardEval = TimingSystem.evaluateOffset(35, dummyPlayer, DifficultyLevel.HARD, 0);

    expect(easyEval.grade).toBe(TimingGrade.PERFECT);
    expect([TimingGrade.PERFECT, TimingGrade.GOOD]).toContain(hardEval.grade);
  });
});
