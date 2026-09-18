import { TimingGrade, TimingEvaluation } from './TimingSystem';
import { PlayerData } from '../player/PlayerModel';
import { DismissalType } from '../cricket/CricketTypes';

export enum DeliveryLine {
  WIDE_OUTSIDE_OFF = 'Wide Outside Off',
  OUTSIDE_OFF = 'Outside Off',
  MIDDLE = 'Middle Stump',
  LEG_STUMP = 'Leg Stump',
  DOWN_LEG = 'Down Leg'
}

export enum DeliveryLength {
  YORKER = 'Yorker',
  FULL = 'Full',
  GOOD_LENGTH = 'Good Length',
  SHORT = 'Short',
  BOUNCER = 'Bouncer'
}

export enum BattingShot {
  DEFENSIVE = 'Defensive Shot',
  STRAIGHT_DRIVE = 'Straight Drive',
  COVER_DRIVE = 'Cover Drive',
  SQUARE_CUT = 'Square Cut',
  PULL_SHOT = 'Pull Shot',
  HOOK_SHOT = 'Hook Shot',
  LEG_GLANCE = 'Leg Glance',
  LOFTED_DRIVE = 'Lofted Drive',
  LEAVE_BALL = 'Leave Ball',
  BLOCK = 'Block'
}

export enum ShotDirection {
  LEFT = 'LEFT',     // Off-side (for right hander) / covers / point / third-man
  CENTER = 'CENTER', // Down the ground / straight / mid-off / mid-on
  RIGHT = 'RIGHT'    // Leg-side (for right hander) / mid-wicket / square leg / fine leg
}

export interface ShotConfig {
  shot: BattingShot;
  displayName: string;
  preferredLines: DeliveryLine[];
  preferredLengths: DeliveryLength[];
  naturalDirections: ShotDirection[];
  risk: 'VERY_LOW' | 'LOW' | 'MEDIUM' | 'HIGH' | 'VERY_HIGH';
  maxRewardRuns: number; // 0, 1, 4, 6
  description: string;
}

export const SHOT_CATALOG: Record<BattingShot, ShotConfig> = {
  [BattingShot.DEFENSIVE]: {
    shot: BattingShot.DEFENSIVE,
    displayName: 'Defensive Shot',
    preferredLines: [DeliveryLine.MIDDLE, DeliveryLine.OUTSIDE_OFF, DeliveryLine.LEG_STUMP],
    preferredLengths: [DeliveryLength.GOOD_LENGTH, DeliveryLength.FULL],
    naturalDirections: [ShotDirection.CENTER, ShotDirection.LEFT, ShotDirection.RIGHT],
    risk: 'VERY_LOW',
    maxRewardRuns: 1,
    description: 'Presents the full face of the bat right under the eyes. Safe and controlled.'
  },
  [BattingShot.BLOCK]: {
    shot: BattingShot.BLOCK,
    displayName: 'Block / Dead Bat',
    preferredLines: [DeliveryLine.MIDDLE, DeliveryLine.OUTSIDE_OFF],
    preferredLengths: [DeliveryLength.YORKER, DeliveryLength.FULL, DeliveryLength.GOOD_LENGTH],
    naturalDirections: [ShotDirection.CENTER],
    risk: 'VERY_LOW',
    maxRewardRuns: 0,
    description: 'Soft hands drop the ball dead onto the pitch. Zero risk of edge.'
  },
  [BattingShot.LEAVE_BALL]: {
    shot: BattingShot.LEAVE_BALL,
    displayName: 'Leave Ball',
    preferredLines: [DeliveryLine.OUTSIDE_OFF, DeliveryLine.WIDE_OUTSIDE_OFF],
    preferredLengths: [DeliveryLength.GOOD_LENGTH, DeliveryLength.SHORT, DeliveryLength.BOUNCER],
    naturalDirections: [ShotDirection.CENTER],
    risk: 'LOW',
    maxRewardRuns: 0,
    description: 'Shoulders arms outside off stump. Dangerous if bowled straight on stumps!'
  },
  [BattingShot.COVER_DRIVE]: {
    shot: BattingShot.COVER_DRIVE,
    displayName: 'Cover Drive',
    preferredLines: [DeliveryLine.OUTSIDE_OFF, DeliveryLine.WIDE_OUTSIDE_OFF],
    preferredLengths: [DeliveryLength.FULL],
    naturalDirections: [ShotDirection.LEFT],
    risk: 'MEDIUM',
    maxRewardRuns: 4,
    description: 'Leaning forward into the pitch of the ball, driving through extra cover with elegance.'
  },
  [BattingShot.STRAIGHT_DRIVE]: {
    shot: BattingShot.STRAIGHT_DRIVE,
    displayName: 'Straight Drive',
    preferredLines: [DeliveryLine.MIDDLE, DeliveryLine.OUTSIDE_OFF],
    preferredLengths: [DeliveryLength.FULL],
    naturalDirections: [ShotDirection.CENTER],
    risk: 'LOW',
    maxRewardRuns: 4,
    description: 'The purist stroke past the non-striker right back down the ground.'
  },
  [BattingShot.SQUARE_CUT]: {
    shot: BattingShot.SQUARE_CUT,
    displayName: 'Square Cut',
    preferredLines: [DeliveryLine.OUTSIDE_OFF, DeliveryLine.WIDE_OUTSIDE_OFF],
    preferredLengths: [DeliveryLength.SHORT, DeliveryLength.GOOD_LENGTH],
    naturalDirections: [ShotDirection.LEFT],
    risk: 'HIGH',
    maxRewardRuns: 4,
    description: 'Flashing blade cutting hard behind point on the back foot.'
  },
  [BattingShot.PULL_SHOT]: {
    shot: BattingShot.PULL_SHOT,
    displayName: 'Pull Shot',
    preferredLines: [DeliveryLine.MIDDLE, DeliveryLine.LEG_STUMP],
    preferredLengths: [DeliveryLength.SHORT],
    naturalDirections: [ShotDirection.RIGHT],
    risk: 'HIGH',
    maxRewardRuns: 4,
    description: 'Rolling the wrists over the bouncing ball, hammering it through mid-wicket.'
  },
  [BattingShot.HOOK_SHOT]: {
    shot: BattingShot.HOOK_SHOT,
    displayName: 'Hook Shot',
    preferredLines: [DeliveryLine.MIDDLE, DeliveryLine.LEG_STUMP, DeliveryLine.OUTSIDE_OFF],
    preferredLengths: [DeliveryLength.BOUNCER],
    naturalDirections: [ShotDirection.RIGHT],
    risk: 'VERY_HIGH',
    maxRewardRuns: 6,
    description: 'Daring high-risk swipe at chest/helmet height down to fine leg.'
  },
  [BattingShot.LEG_GLANCE]: {
    shot: BattingShot.LEG_GLANCE,
    displayName: 'Leg Glance',
    preferredLines: [DeliveryLine.LEG_STUMP, DeliveryLine.DOWN_LEG],
    preferredLengths: [DeliveryLength.FULL, DeliveryLength.GOOD_LENGTH],
    naturalDirections: [ShotDirection.RIGHT],
    risk: 'LOW',
    maxRewardRuns: 4,
    description: 'Deft deflection off the hips guiding the ball down to fine leg or square.'
  },
  [BattingShot.LOFTED_DRIVE]: {
    shot: BattingShot.LOFTED_DRIVE,
    displayName: 'Lofted Drive',
    preferredLines: [DeliveryLine.MIDDLE, DeliveryLine.OUTSIDE_OFF],
    preferredLengths: [DeliveryLength.FULL],
    naturalDirections: [ShotDirection.CENTER, ShotDirection.LEFT, ShotDirection.RIGHT],
    risk: 'VERY_HIGH',
    maxRewardRuns: 6,
    description: 'Full uninhibited swing through the line to launch the ball over the boundary.'
  }
};

export interface BattingExecutionResult {
  shot: BattingShot;
  direction: ShotDirection;
  timing: TimingEvaluation;
  runs: number;
  isBoundaryFour: boolean;
  isBoundarySix: boolean;
  isWicket: boolean;
  dismissalType?: DismissalType;
  dismissalText?: string;
  commentary: string;
  shotSuitability: 'IDEAL' | 'ACCEPTABLE' | 'MISMATCHED';
}

export class BattingEngine {
  /**
   * Evaluates user's selected shot against delivery line & length, timing grade, directional aim, and player skills.
   */
  public static resolveShot(
    shot: BattingShot,
    delivery: { line: DeliveryLine; length: DeliveryLength; paceKph: number; bowlerName: string; isSpin?: boolean },
    timing: TimingEvaluation,
    player: PlayerData,
    direction: ShotDirection = ShotDirection.CENTER
  ): BattingExecutionResult {
    const config = SHOT_CATALOG[shot];
    const isPreferredLine = config.preferredLines.includes(delivery.line);
    const isPreferredLength = config.preferredLengths.includes(delivery.length);
    const isNaturalDirection = config.naturalDirections.includes(direction);

    let suitability: 'IDEAL' | 'ACCEPTABLE' | 'MISMATCHED' = 'MISMATCHED';
    if (isPreferredLine && isPreferredLength && isNaturalDirection) {
      suitability = 'IDEAL';
    } else if ((isPreferredLine || isPreferredLength) && isNaturalDirection) {
      suitability = 'ACCEPTABLE';
    } else if (isPreferredLine && isPreferredLength) {
      suitability = 'ACCEPTABLE';
    }

    // Special Case: Leaving on the stumps!
    if (shot === BattingShot.LEAVE_BALL && (delivery.line === DeliveryLine.MIDDLE || delivery.line === DeliveryLine.LEG_STUMP)) {
      if (delivery.length !== DeliveryLength.BOUNCER) {
        return {
          shot,
          direction,
          timing,
          runs: 0,
          isBoundaryFour: false,
          isBoundarySix: false,
          isWicket: true,
          dismissalType: DismissalType.BOWLED,
          dismissalText: 'b ' + delivery.bowlerName,
          commentary: 'DISASTER! Left the ball on middle stump! Stumps clattered into pieces! Clean bowled!',
          shotSuitability: 'MISMATCHED'
        };
      }
    }

    // Special Case: Block / Leave
    if (shot === BattingShot.BLOCK || shot === BattingShot.LEAVE_BALL) {
      return {
        shot,
        direction,
        timing,
        runs: 0,
        isBoundaryFour: false,
        isBoundarySix: false,
        isWicket: false,
        commentary: shot === BattingShot.LEAVE_BALL
          ? 'Leaves it alone outside off stump with classic restraint.'
          : 'Presents a solid dead bat right into the turf. Dot ball.',
        shotSuitability: suitability
      };
    }

    // Dismissal probability calculation
    let dismissalChance = 0.02;

    if (timing.grade === TimingGrade.VERY_LATE) {
      dismissalChance += (config.risk === 'HIGH' || config.risk === 'VERY_HIGH') ? 0.70 : 0.35;
    } else if (timing.grade === TimingGrade.LATE || timing.grade === TimingGrade.EARLY) {
      dismissalChance += (suitability === 'MISMATCHED' ? 0.30 : 0.12);
    } else if (timing.grade === TimingGrade.PERFECT) {
      dismissalChance = 0.005; // Almost impossible to get out with perfect timing
    }

    // Mismatched shot/direction penalty
    if (suitability === 'MISMATCHED') {
      dismissalChance += 0.20;
    }
    if (!isNaturalDirection) {
      dismissalChance += 0.10;
    }

    // Skill mitigation (technique reduces dismissal chance)
    dismissalChance *= Math.max(0.35, (120 - player.batting.battingTechnique) / 100);

    const roll = Math.random();

    if (roll < dismissalChance) {
      let dType = DismissalType.CAUGHT;
      let dText = 'c Fielder b ' + delivery.bowlerName;

      // Check specific dismissal conditions
      if ((shot === BattingShot.PULL_SHOT || shot === BattingShot.HOOK_SHOT) && timing.grade === TimingGrade.VERY_LATE && Math.random() < 0.2) {
        dType = DismissalType.HIT_WICKET;
        dText = 'hit wicket b ' + delivery.bowlerName;
      } else if (delivery.isSpin && shot === BattingShot.LOFTED_DRIVE && (timing.grade === TimingGrade.EARLY || timing.grade === TimingGrade.VERY_LATE) && Math.random() < 0.45) {
        dType = DismissalType.STUMPED;
        dText = 'st Wicketkeeper b ' + delivery.bowlerName;
      } else if (timing.grade === TimingGrade.EARLY) {
        dType = DismissalType.CAUGHT;
        dText = 'c Fielder b ' + delivery.bowlerName;
      } else if (timing.grade === TimingGrade.VERY_LATE || delivery.length === DeliveryLength.YORKER) {
        dType = Math.random() < 0.6 ? DismissalType.BOWLED : DismissalType.LBW;
        dText = dType === DismissalType.BOWLED ? `b ${delivery.bowlerName}` : `lbw b ${delivery.bowlerName}`;
      } else {
        dType = DismissalType.CAUGHT;
        dText = 'c Keeper b ' + delivery.bowlerName;
      }

      return {
        shot,
        direction,
        timing,
        runs: 0,
        isBoundaryFour: false,
        isBoundarySix: false,
        isWicket: true,
        dismissalType: dType,
        dismissalText: dText,
        commentary: `OUT! ${dText}! Mistimed the ${config.displayName} to the ${direction.toLowerCase()}! (${timing.grade})`,
        shotSuitability: suitability
      };
    }

    // Calculate runs scored
    let runs = 0;
    const power = player.batting.power;
    const directionBonus = isNaturalDirection ? 1 : 0.7;

    if (config.maxRewardRuns === 6) {
      if (timing.grade === TimingGrade.PERFECT && (suitability === 'IDEAL' || power >= 60)) {
        runs = 6;
      } else if (timing.grade === TimingGrade.GOOD) {
        runs = (Math.random() * directionBonus < 0.5) ? 4 : 2;
      } else {
        runs = Math.random() < 0.4 ? 1 : 0;
      }
    } else if (config.maxRewardRuns === 4) {
      if (timing.grade === TimingGrade.PERFECT) {
        runs = 4;
      } else if (timing.grade === TimingGrade.GOOD) {
        runs = (Math.random() * directionBonus < 0.65) ? 4 : (Math.random() < 0.8 ? 2 : 1);
      } else {
        runs = Math.random() < 0.4 ? 1 : 0;
      }
    } else {
      // Defensive / 1 run max
      if (timing.grade === TimingGrade.PERFECT || timing.grade === TimingGrade.GOOD) {
        runs = Math.random() < 0.4 ? 1 : 0;
      } else {
        runs = 0;
      }
    }

    const isFour = runs === 4;
    const isSix = runs === 6;

    let comm = '';
    const dirLabel = direction === ShotDirection.LEFT ? 'through the off-side' : (direction === ShotDirection.RIGHT ? 'through the leg-side' : 'straight down the ground');

    if (isSix) {
      comm = `SIX! Glorious ${config.displayName} ${dirLabel}! High, handsome and cleanly over the ropes! (${timing.grade})`;
    } else if (isFour) {
      comm = `FOUR! Pierces the field ${dirLabel} with a pristine ${config.displayName}! Races to the fence! (${timing.grade})`;
    } else if (runs > 0) {
      comm = `Controlled ${config.displayName} ${dirLabel} for ${runs} run${runs > 1 ? 's' : ''}. (${timing.grade})`;
    } else {
      comm = `Solid ${config.displayName} ${dirLabel}, but picked out the fielder. No run. (${timing.grade})`;
    }

    return {
      shot,
      direction,
      timing,
      runs,
      isBoundaryFour: isFour,
      isBoundarySix: isSix,
      isWicket: false,
      commentary: comm,
      shotSuitability: suitability
    };
  }
}
