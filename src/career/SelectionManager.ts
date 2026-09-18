import { PlayerData } from '../player/PlayerModel';
import { CareerStatus } from './CareerStatus';
import { CAREER_LEVEL_CONFIG, CareerLevel } from './CareerLevel';

export interface SelectionReport {
  status: CareerStatus;
  isSelected: boolean;
  isDropped: boolean;
  isInjured: boolean;
  message: string;
  reasons: string[];
}

export class SelectionManager {
  /**
   * Evaluates if player is selected in the playing XI, benched, dropped, or injured.
   */
  public static evaluateSelection(player: PlayerData, recentRatings: number[] = []): SelectionReport {
    const levelConfig = CAREER_LEVEL_CONFIG[player.career.currentCareerLevel];
    const criteria = levelConfig.selectionCriteria;
    const reasons: string[] = [];

    // 1. Injury Check
    if (player.mental.fitness < 20) {
      return {
        status: CareerStatus.INJURED,
        isSelected: false,
        isDropped: false,
        isInjured: true,
        message: 'Ruled out due to severe fatigue and physical strain.',
        reasons: ['Fitness depleted below 20%']
      };
    }

    // 2. Form & Confidence checks
    if (player.mental.form < criteria.minForm) {
      reasons.push(`Form (${player.mental.form}) is below level threshold (${criteria.minForm})`);
    }

    if (player.mental.fitness < criteria.minFitness) {
      reasons.push(`Fitness (${player.mental.fitness}%) requires conditioning (minimum ${criteria.minFitness}%)`);
    }

    if (player.mental.confidence < criteria.minConfidence) {
      reasons.push(`Confidence is shaken (${player.mental.confidence}/${criteria.minConfidence})`);
    }

    // 3. Consecutive Slump Check
    if (recentRatings.length >= 3) {
      const last3 = recentRatings.slice(-3);
      const avgLast3 = last3.reduce((a, b) => a + b, 0) / 3;
      if (avgLast3 < 3.8) {
        reasons.push(`Struggling with form (averaged ${avgLast3.toFixed(1)} rating over last 3 matches)`);
      }
    }

    if (reasons.length >= 2) {
      return {
        status: CareerStatus.DROPPED,
        isSelected: false,
        isDropped: true,
        isInjured: false,
        message: 'Dropped from the squad due to a dip in form and readiness.',
        reasons
      };
    }

    if (reasons.length === 1) {
      return {
        status: CareerStatus.SUBSTITUTE,
        isSelected: false,
        isDropped: false,
        isInjured: false,
        message: 'Named as substitute on the bench. One strong showing can restore your spot.',
        reasons
      };
    }

    return {
      status: CareerStatus.SELECTED,
      isSelected: true,
      isDropped: false,
      isInjured: false,
      message: 'Selected in the Starting XI! The management trusts your current form.',
      reasons: ['Met all squad readiness benchmarks']
    };
  }
}
