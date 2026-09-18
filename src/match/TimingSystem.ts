import { DifficultyLevel, DIFFICULTY_CONFIGS } from '../career/Difficulty';
import { PlayerData } from '../player/PlayerModel';

export enum TimingGrade {
  PERFECT = 'PERFECT',
  GOOD = 'GOOD',
  EARLY = 'EARLY',
  LATE = 'LATE',
  VERY_LATE = 'VERY_LATE'
}

export interface TimingEvaluation {
  grade: TimingGrade;
  offsetMs: number;
  contactQuality: number; // 0.0 to 1.0
  description: string;
}

export class TimingSystem {
  /**
   * Evaluates manual user timing based on the offset (in ms) from the ideal contact moment.
   * Negative offset = early, positive offset = late.
   */
  public static evaluateOffset(
    offsetMs: number,
    player: PlayerData,
    difficulty: DifficultyLevel = DifficultyLevel.NORMAL,
    fatigue: number = 0
  ): TimingEvaluation {
    const config = DIFFICULTY_CONFIGS[difficulty];

    // Skill & condition adjustments expand or contract the sweet spot
    const skillBonus = (player.batting.timing - 50) * 1.5; // -40ms to +70ms
    const formBonus = (player.mental.form - 50) * 1.0;     // -30ms to +45ms
    const fatiguePenalty = (fatigue / 100) * 50;           // 0ms to -50ms

    const adjustedPerfectWindow = Math.max(50, config.timingWindowMs + skillBonus + formBonus - fatiguePenalty);
    const adjustedGoodWindow = Math.max(100, config.goodTimingWindowMs + skillBonus + formBonus - fatiguePenalty);

    const absOffset = Math.abs(offsetMs);

    if (absOffset <= adjustedPerfectWindow / 2) {
      return {
        grade: TimingGrade.PERFECT,
        offsetMs,
        contactQuality: 0.95 + Math.random() * 0.05,
        description: 'Middle of the willow! Crisp, thunderous connection.'
      };
    }

    if (absOffset <= adjustedGoodWindow / 2) {
      return {
        grade: TimingGrade.GOOD,
        offsetMs,
        contactQuality: 0.75 + Math.random() * 0.15,
        description: 'Clean stroke. Well-controlled into the gap.'
      };
    }

    if (offsetMs < 0) {
      return {
        grade: TimingGrade.EARLY,
        offsetMs,
        contactQuality: 0.35 + Math.random() * 0.20,
        description: 'Played too early! Risk of leading edge.'
      };
    }

    if (absOffset <= adjustedGoodWindow * 1.4) {
      return {
        grade: TimingGrade.LATE,
        offsetMs,
        contactQuality: 0.30 + Math.random() * 0.20,
        description: 'Beaten for pace! Spliced off the thick edge.'
      };
    }

    return {
      grade: TimingGrade.VERY_LATE,
      offsetMs,
      contactQuality: 0.10 + Math.random() * 0.15,
      description: 'Hopelessly late! Completely deceived by line and pace.'
    };
  }

  /**
   * Generates a realistic timing offset when simulated or when player presses shot
   * based on attributes, form, and difficulty.
   */
  public static simulatePlayerTiming(
    player: PlayerData,
    difficulty: DifficultyLevel = DifficultyLevel.NORMAL,
    fatigue: number = 0
  ): TimingEvaluation {
    const config = DIFFICULTY_CONFIGS[difficulty];
    const timingSkill = player.batting.timing;
    const form = player.mental.form;

    // Center standard deviation around 0 (ideal contact)
    const stdDev = Math.max(40, (100 - timingSkill) * 3.5 + (100 - form) * 1.5 + fatigue * 1.2) * (config.aiSkillMultiplier > 1 ? 1.15 : 0.9);

    // Box-Muller transform for normal distribution
    const u1 = Math.random() || 0.0001;
    const u2 = Math.random() || 0.0001;
    const z0 = Math.sqrt(-2.0 * Math.log(u1)) * Math.cos(2.0 * Math.PI * u2);
    const offsetMs = Math.round(z0 * (stdDev / 3));

    return this.evaluateOffset(offsetMs, player, difficulty, fatigue);
  }
}
