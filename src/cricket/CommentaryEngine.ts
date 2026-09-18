import { DismissalType } from './CricketTypes';
import { BattingShot, ShotDirection, DeliveryLine, DeliveryLength } from '../match/BattingEngine';
import { BowlingVariation } from '../match/BowlingEngine';

export class CommentaryEngine {
  private static dotPhrases = [
    'Defended solidly right under the eyes back to the bowler.',
    'Pushed gently towards mid-on, no run taken.',
    'Beaten by pace! Whistles past the outside edge into the keeper\'s gloves.',
    'Leaves it alone outside off stump with classic restraint.',
    'Tapped towards cover point, fielder swoops in quickly to deny a run.',
    'Good length ball on the fourth stump line, left comfortably through to the keeper.',
    'Steered straight to backward point, sharp fielding cuts off any chance of a single.',
    'Forward defensive prodded straight down into the pitch.'
  ];

  private static singlePhrases = [
    'Tucked off the hips toward deep square leg for a sharp single.',
    'Pushed into the gap at extra cover, easy single taken.',
    'Glanced neatly down to third man to rotate the strike.',
    'Drives softly toward mid-off and scampers through for a quick single.',
    'Worked off the pads toward mid-wicket, good call between the batsmen.',
    'Punched off the back foot through point to get off the mark.'
  ];

  private static twoPhrases = [
    'Punched through the covers, outfield is fast but cut off in the deep for a brace.',
    'Clipped delicately into the vacant mid-wicket region, they hustle back for two.',
    'Turned softly towards fine leg, sharp sprint between the wickets converts one into two.',
    'Driven wide of mid-off, great running puts pressure on the deep sweeper.'
  ];

  private static fourPhrases = [
    'FOUR! Glorious cover drive! Pierces the infield and races across the lush turf.',
    'FOUR! Cracked through backward point with blistering hand speed!',
    'FOUR! Short and punished! Pulled ferociously through mid-wicket to the fence.',
    'FOUR! Exquisite timing! Just punched on the up straight down the ground.',
    'FOUR! Guided masterfully past the slip cordon to the third man fence!',
    'FOUR! Leans into the drive and beats the despairing dive at mid-off!'
  ];

  private static sixPhrases = [
    'SIX! HIGH, HANDSOME AND HUGE! Clears the long-on boundary with immense power!',
    'SIX! That is imperious! Down on one knee and launched into the top tier over deep mid-wicket!',
    'SIX! Pure elegance! Leans into the lofted drive and deposits it cleanly over long-off!',
    'SIX! Sweet as honey off the sweet spot! Flat, ferocious maximum over square leg!',
    'SIX! Pulled dismissively over deep fine leg into the ecstatic crowd!'
  ];

  private static wicketPhrases: Record<DismissalType, string[]> = {
    [DismissalType.BOWLED]: [
      'OUT! CLEAN BOWLED! Rattles the woodwork! Timber! Stumps sent cartwheeling!',
      'OUT! DRAGGED ON! Inside edge onto the off stump! Massive breakthrough!',
      'OUT! Beautiful seam movement pitches and rips back through the gate! Clean bowled!',
      'OUT! Yorker right at the base of leg stump! Defeated for sheer pace!'
    ],
    [DismissalType.CAUGHT]: [
      'OUT! EDGED AND TAKEN! Thick outside edge and swallowed by the keeper!',
      'OUT! CAUGHT IN THE DEEP! Holes out trying to clear the boundary ropes!',
      'OUT! Sharp catch at first slip! Reacted with lightning reflexes!',
      'OUT! Spliced high into the sky... settled underneath and caught comfortably!'
    ],
    [DismissalType.LBW]: [
      'OUT! LOUD APPEAL... AND GIVEN! Plumb in front! Trapped on the back foot!',
      'OUT! LBW! Struck right in front of middle stump! The umpire raises the finger without hesitation!',
      'OUT! Pitched in line, straightened and struck low on the front pad! Umpire says on your way!'
    ],
    [DismissalType.RUN_OUT]: [
      'OUT! DIRECT HIT AND HE IS GONE! Sensational piece of fielding! Caught short of the crease!',
      'OUT! Mix-up in the middle! Both batsmen stranded at one end! Easy run out!',
      'OUT! Diving for his life, but the throw shatters the stumps! Out by a whisker!'
    ],
    [DismissalType.STUMPED]: [
      'OUT! STUMPED! Dragged out of his crease by cunning flight, and the keeper whips off the bails in a flash!',
      'OUT! Beaten in flight and turn! Keeper does the rest in the blink of an eye!'
    ],
    [DismissalType.HIT_WICKET]: [
      'OUT! HIT WICKET! Stepped back too deep in the crease and dislodged the bails!'
    ],
    [DismissalType.NOT_OUT]: []
  };

  public static getBallCommentary(
    striker: string,
    bowler: string,
    runs: number,
    isWicket: boolean,
    dismissalType?: DismissalType
  ): string {
    if (isWicket && dismissalType) {
      const phrases = this.wicketPhrases[dismissalType] || this.wicketPhrases[DismissalType.BOWLED];
      const selected = phrases[Math.floor(Math.random() * phrases.length)];
      return `${bowler} to ${striker}: ${selected}`;
    }

    if (runs === 6) {
      const p = this.sixPhrases[Math.floor(Math.random() * this.sixPhrases.length)];
      return `${bowler} to ${striker}: ${p}`;
    }

    if (runs === 4) {
      const p = this.fourPhrases[Math.floor(Math.random() * this.fourPhrases.length)];
      return `${bowler} to ${striker}: ${p}`;
    }

    if (runs === 2 || runs === 3) {
      const p = this.twoPhrases[Math.floor(Math.random() * this.twoPhrases.length)];
      return `${bowler} to ${striker}: ${p}`;
    }

    if (runs === 1) {
      const p = this.singlePhrases[Math.floor(Math.random() * this.singlePhrases.length)];
      return `${bowler} to ${striker}: ${p}`;
    }

    const p = this.dotPhrases[Math.floor(Math.random() * this.dotPhrases.length)];
    return `${bowler} to ${striker}: ${p}`;
  }

  /**
   * Generates contextual commentary incorporating shot type, direction, speed, and timing.
   */
  public static getDetailedCommentary(params: {
    striker: string;
    bowler: string;
    runs: number;
    isWicket: boolean;
    dismissalType?: DismissalType;
    shot?: BattingShot;
    direction?: ShotDirection;
    paceKph?: number;
    timingGrade?: string;
    isPowerplay?: boolean;
    requiredRunRate?: number;
  }): string {
    const { striker, bowler, runs, isWicket, dismissalType, shot, direction, paceKph } = params;

    if (isWicket && dismissalType) {
      return this.getBallCommentary(striker, bowler, runs, true, dismissalType);
    }

    const pacePrefix = paceKph ? `[${paceKph} km/h] ` : '';
    const dirLabel = direction === ShotDirection.LEFT
      ? 'through the off-side'
      : (direction === ShotDirection.RIGHT ? 'through the leg-side' : 'straight down the ground');

    if (runs === 6) {
      return `${pacePrefix}${bowler} to ${striker}: SIX! Launched high and mighty ${dirLabel} with a tremendous ${shot || 'stroke'}!`;
    }

    if (runs === 4) {
      return `${pacePrefix}${bowler} to ${striker}: FOUR! Pierces the infield ${dirLabel} with an exquisite ${shot || 'stroke'}!`;
    }

    if (runs > 0) {
      return `${pacePrefix}${bowler} to ${striker}: Played softly ${dirLabel} for ${runs} run${runs > 1 ? 's' : ''}.`;
    }

    return `${pacePrefix}${bowler} to ${striker}: ${shot || 'Ball'} defended ${dirLabel}. No run taken.`;
  }
}
