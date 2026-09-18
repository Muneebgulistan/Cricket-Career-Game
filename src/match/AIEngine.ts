import { DeliveryLine, DeliveryLength, BattingShot, ShotDirection } from './BattingEngine';
import { BowlingVariation } from './BowlingEngine';

export enum AIAggression {
  AGGRESSIVE = 'AGGRESSIVE',
  BALANCED = 'BALANCED',
  DEFENSIVE = 'DEFENSIVE'
}

export interface AIPersonality {
  id: string;
  name: string;
  battingAggression: AIAggression;
  bowlingAggression: AIAggression;
  skill: number;
}

export class AIEngine {
  /**
   * Generates a context-aware delivery from an AI bowler to a batter (e.g. user or AI).
   */
  public static decideBowlerDelivery(
    bowlerSkill: number,
    bowlerAggression: AIAggression,
    matchContext: {
      oversRemaining: number;
      wicketsDown: number;
      requiredRunRate?: number;
      isPowerplay?: boolean;
      batterAggression?: AIAggression;
    }
  ): { line: DeliveryLine; length: DeliveryLength; paceKph: number; variation: BowlingVariation } {
    const isDeathOvers = matchContext.oversRemaining <= 4;
    const isChasingTight = (matchContext.requiredRunRate ?? 0) > 8.5;
    const isPowerplay = matchContext.isPowerplay ?? (matchContext.oversRemaining >= 14); // overs 1-6
    const facingAggressiveBatter = matchContext.batterAggression === AIAggression.AGGRESSIVE;

    let line = DeliveryLine.OUTSIDE_OFF;
    let length = DeliveryLength.GOOD_LENGTH;
    let variation = BowlingVariation.NORMAL_PACE;
    let paceKph = 125 + Math.round((bowlerSkill - 50) * 0.4);

    const roll = Math.random();

    // Human-like mistake roll (5% chance of a loose delivery)
    if (Math.random() < 0.05) {
      if (Math.random() < 0.5) {
        return {
          line: DeliveryLine.DOWN_LEG,
          length: DeliveryLength.FULL,
          paceKph: Math.round(paceKph * 0.95),
          variation: BowlingVariation.NORMAL_PACE
        };
      } else {
        return {
          line: DeliveryLine.OUTSIDE_OFF,
          length: DeliveryLength.SHORT,
          paceKph: paceKph,
          variation: BowlingVariation.NORMAL_PACE
        };
      }
    }

    if (bowlerAggression === AIAggression.AGGRESSIVE || isDeathOvers || isChasingTight) {
      // Aggressive or Death Overs: attacks with yorkers, bouncers, and stump lines
      if (roll < 0.40) {
        length = DeliveryLength.YORKER;
        line = Math.random() < 0.7 ? DeliveryLine.MIDDLE : DeliveryLine.OUTSIDE_OFF;
        variation = Math.random() < 0.4 ? BowlingVariation.REVERSE_SWING : BowlingVariation.NORMAL_PACE;
      } else if (roll < 0.65) {
        length = DeliveryLength.BOUNCER;
        line = DeliveryLine.MIDDLE;
        variation = BowlingVariation.SEAM_UP;
      } else {
        length = DeliveryLength.GOOD_LENGTH;
        line = DeliveryLine.OUTSIDE_OFF;
        variation = Math.random() < 0.5 ? BowlingVariation.OUTSWING : BowlingVariation.SLOWER_BALL;
      }
    } else if (facingAggressiveBatter || isPowerplay) {
      // Countering an aggressive batter or powerplay field restrictions:
      // Mixes in wider off-stump channels, steep short balls, and cutters
      if (roll < 0.35) {
        length = DeliveryLength.GOOD_LENGTH;
        line = DeliveryLine.WIDE_OUTSIDE_OFF;
        variation = BowlingVariation.OFF_CUTTER;
      } else if (roll < 0.65) {
        length = DeliveryLength.SHORT;
        line = DeliveryLine.OUTSIDE_OFF;
        variation = BowlingVariation.SEAM_UP;
      } else {
        length = DeliveryLength.YORKER;
        line = DeliveryLine.MIDDLE;
        variation = BowlingVariation.SLOWER_BALL;
      }
    } else if (bowlerAggression === AIAggression.DEFENSIVE) {
      // Defensive: bowls tight 4th-stump channels and cutters
      length = Math.random() < 0.65 ? DeliveryLength.GOOD_LENGTH : DeliveryLength.FULL;
      line = DeliveryLine.OUTSIDE_OFF;
      variation = Math.random() < 0.4 ? BowlingVariation.OFF_CUTTER : BowlingVariation.NORMAL_PACE;
    } else {
      // Balanced
      if (roll < 0.45) {
        length = DeliveryLength.GOOD_LENGTH;
        line = DeliveryLine.OUTSIDE_OFF;
        variation = BowlingVariation.OUTSWING;
      } else if (roll < 0.75) {
        length = DeliveryLength.FULL;
        line = DeliveryLine.MIDDLE;
        variation = BowlingVariation.INSWING;
      } else {
        length = DeliveryLength.SHORT;
        line = DeliveryLine.OUTSIDE_OFF;
        variation = BowlingVariation.NORMAL_PACE;
      }
    }

    return { line, length, paceKph, variation };
  }

  /**
   * Evaluates AI batsman scoring attempt against a delivery.
   */
  public static evaluateAIBatting(
    batterSkill: number,
    batterAggression: AIAggression,
    bowlerSkill: number,
    delivery: { line: DeliveryLine; length: DeliveryLength; paceKph: number; variation: BowlingVariation },
    matchContext: { requiredRunRate?: number; oversRemaining: number; wicketsDown: number; isPowerplay?: boolean }
  ): { runs: number; isWicket: boolean; isBoundaryFour: boolean; isBoundarySix: boolean; shot?: BattingShot; direction?: ShotDirection } {
    const isPressureChasing = (matchContext.requiredRunRate ?? 0) > 9.0;
    const isBattingCollapse = matchContext.wicketsDown >= 6;
    const isPowerplay = matchContext.isPowerplay ?? (matchContext.oversRemaining >= 14);

    let boundaryChance = 0.12;
    let wicketChance = 0.045;
    let dotChance = 0.45;

    // Powerplay bonus (infield is up, so lofted or driven shots go for boundaries more easily)
    if (isPowerplay) {
      boundaryChance += 0.08;
      dotChance -= 0.08;
    }

    // Aggression adjustments
    if (batterAggression === AIAggression.AGGRESSIVE || isPressureChasing) {
      boundaryChance += 0.12;
      wicketChance += 0.04;
      dotChance -= 0.15;
    } else if (batterAggression === AIAggression.DEFENSIVE || isBattingCollapse) {
      boundaryChance -= 0.06;
      wicketChance -= 0.025;
      dotChance += 0.20;
    }

    // Tactical delivery counter: Yorker or bouncer increases dot/wicket chance
    if (delivery.length === DeliveryLength.YORKER) {
      wicketChance += 0.04;
      boundaryChance -= 0.05;
      dotChance += 0.10;
    } else if (delivery.length === DeliveryLength.BOUNCER) {
      if (batterAggression === AIAggression.AGGRESSIVE) {
        wicketChance += 0.05; // Top edge on pull
      } else {
        dotChance += 0.25;    // Ducked
      }
    }

    // Skill differential
    const skillDiff = (batterSkill - bowlerSkill) / 200;
    boundaryChance = Math.max(0.02, Math.min(0.40, boundaryChance + skillDiff));
    wicketChance = Math.max(0.01, Math.min(0.25, wicketChance - skillDiff * 0.5));

    const roll = Math.random();

    if (roll < wicketChance) {
      return { runs: 0, isWicket: true, isBoundaryFour: false, isBoundarySix: false };
    }

    const runRoll = Math.random();
    if (runRoll < dotChance) {
      return { runs: 0, isWicket: false, isBoundaryFour: false, isBoundarySix: false };
    } else if (runRoll < dotChance + 0.30) {
      return { runs: 1, isWicket: false, isBoundaryFour: false, isBoundarySix: false };
    } else if (runRoll < dotChance + 0.40) {
      return { runs: 2, isWicket: false, isBoundaryFour: false, isBoundarySix: false };
    } else if (runRoll < dotChance + 0.40 + boundaryChance * 0.75) {
      return { runs: 4, isWicket: false, isBoundaryFour: true, isBoundarySix: false };
    } else if (runRoll < dotChance + 0.40 + boundaryChance) {
      return { runs: 6, isWicket: false, isBoundaryFour: false, isBoundarySix: true };
    }

    return { runs: 0, isWicket: false, isBoundaryFour: false, isBoundarySix: false };
  }
}
