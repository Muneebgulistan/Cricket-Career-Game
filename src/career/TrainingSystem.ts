import { PlayerData } from '../player/PlayerModel';
import { RatingCalculator } from '../player/RatingCalculator';

export enum TrainingCategory {
  BATTING_TECHNIQUE = 'Batting Technique & Timing',
  BATTING_POWER = 'Power Hitting & Range',
  BOWLING_ACCURACY = 'Bowling Accuracy & Control',
  BOWLING_VARIATIONS = 'Swing, Seam & Variations',
  FIELDING_REFLEXES = 'Catching & Reflexes',
  FITNESS_STAMINA = 'Gym Conditioning & Stamina',
  MENTAL_COMPOSURE = 'Mental Composure & Pressure'
}

export interface TrainingDrillResult {
  category: TrainingCategory;
  attributeImproved: string;
  previousValue: number;
  newValue: number;
  fatigueCost: number;
  diminishingReturnsApplied: boolean;
  message: string;
}

export class TrainingSystem {
  /**
   * Applies training drill with realistic diminishing returns.
   * As an attribute approaches 90+, progression points required scale up dramatically.
   */
  public static executeDrill(player: PlayerData, category: TrainingCategory): TrainingDrillResult {
    let attrName = '';
    let prevVal = 0;
    let newVal = 0;
    let fatigueCost = 12;

    const applyDiminishingReturn = (current: number, potential: number): number => {
      if (current >= potential) return current; // Capped at potential
      if (current >= 85) return Math.random() < 0.25 ? current + 1 : current; // High diminishing returns
      if (current >= 70) return Math.random() < 0.50 ? current + 1 : current;
      return current + 1; // Direct improvement for lower tiers
    };

    switch (category) {
      case TrainingCategory.BATTING_TECHNIQUE:
        attrName = 'Batting Technique & Timing';
        prevVal = player.batting.battingTechnique;
        player.batting.battingTechnique = applyDiminishingReturn(prevVal, player.potential.potentialRating);
        player.batting.timing = applyDiminishingReturn(player.batting.timing, player.potential.potentialRating);
        newVal = player.batting.battingTechnique;
        fatigueCost = 10;
        break;

      case TrainingCategory.BATTING_POWER:
        attrName = 'Power & Boundary Hitting';
        prevVal = player.batting.power;
        player.batting.power = applyDiminishingReturn(prevVal, player.potential.potentialRating);
        newVal = player.batting.power;
        fatigueCost = 14;
        break;

      case TrainingCategory.BOWLING_ACCURACY:
        attrName = 'Bowling Accuracy';
        prevVal = player.bowling.accuracy;
        player.bowling.accuracy = applyDiminishingReturn(prevVal, player.potential.potentialRating);
        newVal = player.bowling.accuracy;
        fatigueCost = 12;
        break;

      case TrainingCategory.BOWLING_VARIATIONS:
        attrName = 'Swing & Variation';
        prevVal = player.bowling.variation;
        player.bowling.variation = applyDiminishingReturn(prevVal, player.potential.potentialRating);
        player.bowling.swing = applyDiminishingReturn(player.bowling.swing, player.potential.potentialRating);
        newVal = player.bowling.variation;
        fatigueCost = 12;
        break;

      case TrainingCategory.FIELDING_REFLEXES:
        attrName = 'Catching & Reflexes';
        prevVal = player.fielding.catching;
        player.fielding.catching = applyDiminishingReturn(prevVal, player.potential.potentialRating);
        player.fielding.reflexes = applyDiminishingReturn(player.fielding.reflexes, player.potential.potentialRating);
        newVal = player.fielding.catching;
        fatigueCost = 9;
        break;

      case TrainingCategory.FITNESS_STAMINA:
        attrName = 'Physical Conditioning';
        prevVal = player.mental.fitness;
        player.mental.fitness = Math.min(100, player.mental.fitness + 15);
        player.fielding.fielding = applyDiminishingReturn(player.fielding.fielding, player.potential.potentialRating);
        newVal = player.mental.fitness;
        fatigueCost = 8;
        break;

      case TrainingCategory.MENTAL_COMPOSURE:
        attrName = 'Pressure Handling';
        prevVal = player.mental.pressureHandling;
        player.mental.pressureHandling = applyDiminishingReturn(prevVal, player.potential.potentialRating);
        player.mental.confidence = Math.min(100, player.mental.confidence + 10);
        newVal = player.mental.pressureHandling;
        fatigueCost = 6;
        break;
    }

    // Recalculate dynamic overall rating
    player.potential.overallRating = RatingCalculator.calculateOverall(player);

    const gained = newVal > prevVal;
    return {
      category,
      attributeImproved: attrName,
      previousValue: prevVal,
      newValue: newVal,
      fatigueCost,
      diminishingReturnsApplied: !gained && prevVal >= 70,
      message: gained
        ? `Great drill! ${attrName} improved to ${newVal}. Overall rating is now ${player.potential.overallRating}.`
        : `Tough session. High training mastery means gains require persistent practice.`
    };
  }
}
