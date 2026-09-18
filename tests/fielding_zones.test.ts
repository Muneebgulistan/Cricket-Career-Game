import { describe, it, expect } from 'vitest';
import { FieldingSystem, FieldingEventType } from '../src/match/FieldingSystem';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';
import { DifficultyLevel } from '../src/career/Difficulty';

describe('Step 3 — Fielding Reaction & Throw Decision Engine', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Shadab',
    lastName: 'Khan',
    role: PlayerRole.ALL_ROUNDER,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  it('should generate fielding prompts with positive reaction windows', () => {
    const prompt = FieldingSystem.generateFieldingEvent(dummyPlayer, 'Striker Batsman', DifficultyLevel.NORMAL);
    expect(prompt.id).toBeDefined();
    expect(prompt.reactionWindowMs).toBeGreaterThan(300);
    expect(Object.values(FieldingEventType)).toContain(prompt.type);
  });

  it('should award a clean catch when player reacts within the reaction window', () => {
    const prompt = {
      id: 'test_catch_prompt',
      type: FieldingEventType.CATCH_OPPORTUNITY,
      title: 'Catch chance',
      description: 'Pouch it',
      batsmanName: 'Virat',
      reactionWindowMs: 800,
      difficulty: DifficultyLevel.NORMAL
    };

    dummyPlayer.fielding.catching = 95;
    const outcome = FieldingSystem.resolveReaction(prompt, 400, dummyPlayer);
    expect(outcome.isSuccess).toBe(true);
    expect(outcome.isCatch).toBe(true);
    expect(outcome.isWicket).toBe(true);
  });

  it('should register a dropped catch if reaction time exceeds window', () => {
    const prompt = {
      id: 'test_late_catch',
      type: FieldingEventType.CATCH_OPPORTUNITY,
      title: 'Catch chance',
      description: 'Pouch it',
      batsmanName: 'Smith',
      reactionWindowMs: 500,
      difficulty: DifficultyLevel.NORMAL
    };

    const outcome = FieldingSystem.resolveReaction(prompt, 900, dummyPlayer);
    expect(outcome.isSuccess).toBe(false);
    expect(outcome.isCatch).toBe(false);
    expect(outcome.commentary).toContain('DROPPED');
  });

  it('should support direct-hit target choice for run outs', () => {
    const prompt = {
      id: 'test_runout',
      type: FieldingEventType.RUN_OUT_CHANCE,
      title: 'Run out chance',
      description: 'Aim at stumps',
      batsmanName: 'Root',
      reactionWindowMs: 800,
      difficulty: DifficultyLevel.NORMAL
    };

    dummyPlayer.fielding.throwing = 99; // Top tier thrower
    const outcome = FieldingSystem.resolveReaction(prompt, 350, dummyPlayer, 'DIRECT_HIT');
    expect(outcome.reactionTimeMs).toBe(350);
  });
});
