import { DeliveryLine, DeliveryLength } from './BattingEngine';
import { PlayerData, BowlingStyle, PlayerRole } from '../player/PlayerModel';
import { DismissalType } from '../cricket/CricketTypes';

export enum BowlingVariation {
  // Pace variations
  NORMAL_PACE = 'Stock Delivery',
  OUTSWING = 'Outswing',
  INSWING = 'Inswing',
  REVERSE_SWING = 'Reverse Swing',
  SLOWER_BALL = 'Slower Ball (Back of Hand)',
  OFF_CUTTER = 'Off Cutter',
  LEG_CUTTER = 'Leg Cutter',
  SEAM_UP = 'Wobbly Seam',

  // Spin variations
  NORMAL_SPIN = 'Stock Spin Delivery',
  FLIGHTED = 'Loopy Flight & Dip',
  SHARP_TURN = 'Sharp Gripping Turn',
  ARM_BALL = 'Skidding Arm Ball',
  TOP_SPINNER = 'Bouncing Top Spinner',
  WRIST_VARIATION = 'Googly / Mystery Ball',
  QUICKER_BALL = 'Flat Quicker Delivery'
}

export interface BowlingDeliveryInput {
  line: DeliveryLine;
  length: DeliveryLength;
  variation: BowlingVariation;
  effortPacePercentage: number; // 80 - 105%
  meterQuality?: number; // 0.0 to 1.0 (from interactive accuracy/power meter)
}

export interface BowlingDeliveryResult {
  intendedLine: DeliveryLine;
  actualLine: DeliveryLine;
  intendedLength: DeliveryLength;
  actualLength: DeliveryLength;
  variation: BowlingVariation;
  paceKph: number;
  runsConceded: number;
  isWicket: boolean;
  dismissalType?: DismissalType;
  dismissalText?: string;
  isExtra: boolean;
  extraType?: 'wide' | 'no-ball';
  commentary: string;
}

export class BowlingEngine {
  /**
   * Returns valid bowling variations strictly tailored to the player's bowling style.
   */
  public static getAvailableVariations(style: BowlingStyle): BowlingVariation[] {
    if (style === BowlingStyle.NONE) return [BowlingVariation.NORMAL_PACE];

    if (
      style === BowlingStyle.RIGHT_ARM_FAST ||
      style === BowlingStyle.LEFT_ARM_FAST ||
      style === BowlingStyle.RIGHT_ARM_MEDIUM ||
      style === BowlingStyle.LEFT_ARM_MEDIUM
    ) {
      return [
        BowlingVariation.NORMAL_PACE,
        BowlingVariation.OUTSWING,
        BowlingVariation.INSWING,
        BowlingVariation.SEAM_UP,
        BowlingVariation.SLOWER_BALL,
        BowlingVariation.OFF_CUTTER,
        BowlingVariation.LEG_CUTTER,
        BowlingVariation.REVERSE_SWING
      ];
    }

    // Spin bowlers
    return [
      BowlingVariation.NORMAL_SPIN,
      BowlingVariation.FLIGHTED,
      BowlingVariation.SHARP_TURN,
      BowlingVariation.ARM_BALL,
      BowlingVariation.TOP_SPINNER,
      BowlingVariation.WRIST_VARIATION,
      BowlingVariation.QUICKER_BALL
    ];
  }

  /**
   * Resolves the delivery bowled by the user against an AI batsman.
   */
  public static executeDelivery(
    input: BowlingDeliveryInput,
    player: PlayerData,
    batter: { name: string; ability: number; aggression: 'AGGRESSIVE' | 'BALANCED' | 'DEFENSIVE' },
    pitchType: string = 'BALANCED',
    fatigue: number = 0
  ): BowlingDeliveryResult {
    // 1. Accuracy and Execution Error Check
    const accuracy = player.bowling.accuracy;
    const meterQuality = input.meterQuality !== undefined ? input.meterQuality : 0.85;

    // Meter quality heavily impacts error roll
    const errorChance = Math.max(
      0.02,
      (100 - accuracy) * 0.003 + (fatigue / 100) * 0.08 + (1 - meterQuality) * 0.35
    );

    let actualLine = input.line;
    let actualLength = input.length;
    let isExtra = false;
    let extraType: 'wide' | 'no-ball' | undefined;

    // Check release error (e.g. spray down leg, wide outside off, or high beamer/no-ball)
    if (Math.random() < errorChance) {
      if (input.line === DeliveryLine.WIDE_OUTSIDE_OFF || Math.random() < 0.35) {
        isExtra = true;
        extraType = 'wide';
        return {
          intendedLine: input.line,
          actualLine: DeliveryLine.WIDE_OUTSIDE_OFF,
          intendedLength: input.length,
          actualLength: input.length,
          variation: input.variation,
          paceKph: Math.round(player.bowling.pace * 1.3),
          runsConceded: 1,
          isWicket: false,
          isExtra: true,
          extraType: 'wide',
          commentary: 'WIDE! Sprayed too far outside off stump. Umpire signals wide.'
        };
      } else if (input.line === DeliveryLine.DOWN_LEG) {
        isExtra = true;
        extraType = 'wide';
        return {
          intendedLine: input.line,
          actualLine: DeliveryLine.DOWN_LEG,
          intendedLength: input.length,
          actualLength: input.length,
          variation: input.variation,
          paceKph: Math.round(player.bowling.pace * 1.3),
          runsConceded: 1,
          isWicket: false,
          isExtra: true,
          extraType: 'wide',
          commentary: 'WIDE! Slips down leg side beyond the reach of keeper and batsman.'
        };
      } else {
        // Minor deviation in length/line
        if (input.length === DeliveryLength.YORKER) actualLength = DeliveryLength.FULL;
        else if (input.length === DeliveryLength.GOOD_LENGTH) actualLength = DeliveryLength.SHORT;
      }
    }

    // 2. Base Pace calculation
    let basePace = 80;
    const isPacer = player.bowlingStyle.includes('fast') || player.bowlingStyle.includes('medium');
    if (isPacer) {
      basePace = 115 + (player.bowling.pace * 0.35); // 120 - 150 kph
    } else {
      basePace = 75 + (player.bowling.pace * 0.20);  // 80 - 95 kph
    }

    if (input.variation === BowlingVariation.SLOWER_BALL) basePace *= 0.82;
    if (input.variation === BowlingVariation.QUICKER_BALL) basePace *= 1.12;

    const finalPace = Math.round(basePace * (input.effortPacePercentage / 100));

    // 3. Wicket & Dot ball chances
    let wicketChance = 0.05;
    let dotChance = 0.45;

    // Meter accuracy bonus
    if (meterQuality >= 0.85) {
      wicketChance += 0.04;
      dotChance += 0.15;
    }

    // Tactical advantages (e.g. Yorker on middle, Good length outside off, Bouncer against aggressive batter)
    if (actualLength === DeliveryLength.YORKER && (actualLine === DeliveryLine.MIDDLE || actualLine === DeliveryLine.LEG_STUMP)) {
      wicketChance += 0.08;
      dotChance += 0.20;
    } else if (actualLength === DeliveryLength.GOOD_LENGTH && actualLine === DeliveryLine.OUTSIDE_OFF) {
      wicketChance += 0.06;
      dotChance += 0.15;
    } else if (actualLength === DeliveryLength.BOUNCER && batter.aggression === 'AGGRESSIVE') {
      wicketChance += 0.07; // Hook top-edge risk
    }

    // Bowler skill & pitch conditions
    wicketChance += (player.bowling.bowlingAbility / 300);
    if (pitchType === 'GREEN_SEAM' && (input.variation === BowlingVariation.SEAM_UP || input.variation === BowlingVariation.OUTSWING)) {
      wicketChance += 0.04;
    } else if (pitchType === 'DUSTY_SPIN' && !isPacer) {
      wicketChance += 0.05;
    }

    // Batter skill reduces wicket chance
    wicketChance = Math.max(0.02, wicketChance - (batter.ability / 400));

    const roll = Math.random();

    // 4. Resolve Wicket
    if (roll < wicketChance) {
      let dType = DismissalType.BOWLED;
      let dText = `b ${player.firstName} ${player.lastName}`;

      if (actualLength === DeliveryLength.YORKER) {
        dType = DismissalType.BOWLED;
        dText = `b ${player.firstName} ${player.lastName}`;
      } else if (actualLine === DeliveryLine.OUTSIDE_OFF) {
        dType = Math.random() < 0.6 ? DismissalType.CAUGHT : DismissalType.LBW;
        dText = dType === DismissalType.CAUGHT ? `c Keeper b ${player.lastName}` : `lbw b ${player.lastName}`;
      } else if (actualLength === DeliveryLength.BOUNCER) {
        dType = DismissalType.CAUGHT;
        dText = `c Deep Fielder b ${player.lastName}`;
      }

      return {
        intendedLine: input.line,
        actualLine,
        intendedLength: input.length,
        actualLength,
        variation: input.variation,
        paceKph: finalPace,
        runsConceded: 0,
        isWicket: true,
        dismissalType: dType,
        dismissalText: dText,
        isExtra: false,
        commentary: `OUT! ${dText}! Sensational execution of the ${input.length} ${input.variation}! ${batter.name} is dismissed!`
      };
    }

    // 5. Resolve Runs Conceded
    const runRoll = Math.random();
    let runs = 0;

    if (runRoll < dotChance) {
      runs = 0;
    } else if (runRoll < dotChance + 0.30) {
      runs = 1;
    } else if (runRoll < dotChance + 0.40) {
      runs = 2;
    } else if (runRoll < dotChance + 0.48) {
      runs = 4;
    } else if (runRoll < dotChance + 0.52 && batter.aggression === 'AGGRESSIVE') {
      runs = 6;
    } else {
      runs = 0;
    }

    let comm = '';
    if (runs === 0) {
      comm = `${input.length} on ${actualLine} at ${finalPace} km/h. Defended solidly. Dot ball.`;
    } else if (runs === 4) {
      comm = `FOUR! Batter capitalizes on the ${actualLength} ball, driving through the gap.`;
    } else if (runs === 6) {
      comm = `SIX! Clears the ropes off a slightly missed length at ${finalPace} km/h.`;
    } else {
      comm = `Pushed into the outfield for ${runs} run${runs > 1 ? 's' : ''}.`;
    }

    return {
      intendedLine: input.line,
      actualLine,
      intendedLength: input.length,
      actualLength,
      variation: input.variation,
      paceKph: finalPace,
      runsConceded: runs,
      isWicket: false,
      isExtra: false,
      commentary: comm
    };
  }
}
