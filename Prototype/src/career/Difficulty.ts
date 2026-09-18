export enum DifficultyLevel {
  EASY = 'EASY',
  NORMAL = 'NORMAL',
  HARD = 'HARD'
}

export interface DifficultyConfig {
  name: DifficultyLevel;
  timingWindowMs: number;      // Milliseconds window for Perfect timing
  goodTimingWindowMs: number;  // Milliseconds window for Good timing
  aiSkillMultiplier: number;   // 0.8 to 1.2
  bowlingMarginOfError: number;// Degrees/variance in release
  fieldingReactionWindowMs: number;
}

export const DIFFICULTY_CONFIGS: Record<DifficultyLevel, DifficultyConfig> = {
  [DifficultyLevel.EASY]: {
    name: DifficultyLevel.EASY,
    timingWindowMs: 250,
    goodTimingWindowMs: 450,
    aiSkillMultiplier: 0.85,
    bowlingMarginOfError: 0.1,
    fieldingReactionWindowMs: 1400
  },
  [DifficultyLevel.NORMAL]: {
    name: DifficultyLevel.NORMAL,
    timingWindowMs: 160,
    goodTimingWindowMs: 320,
    aiSkillMultiplier: 1.0,
    bowlingMarginOfError: 0.2,
    fieldingReactionWindowMs: 1000
  },
  [DifficultyLevel.HARD]: {
    name: DifficultyLevel.HARD,
    timingWindowMs: 90,
    goodTimingWindowMs: 200,
    aiSkillMultiplier: 1.18,
    bowlingMarginOfError: 0.35,
    fieldingReactionWindowMs: 700
  }
};
