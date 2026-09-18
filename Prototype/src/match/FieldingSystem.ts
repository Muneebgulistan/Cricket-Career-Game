import { PlayerData, PlayerRole } from '../player/PlayerModel';
import { DifficultyLevel, DIFFICULTY_CONFIGS } from '../career/Difficulty';

export enum FieldingEventType {
  CATCH_OPPORTUNITY = 'CATCH_OPPORTUNITY',
  GROUND_FIELDING = 'GROUND_FIELDING',
  RUN_OUT_CHANCE = 'RUN_OUT_CHANCE',
  BOUNDARY_STOP = 'BOUNDARY_STOP'
}

export type ThrowTarget = 'KEEPER_END' | 'BOWLER_END' | 'DIRECT_HIT';

export interface FieldingEventPrompt {
  id: string;
  type: FieldingEventType;
  title: string;
  description: string;
  batsmanName: string;
  reactionWindowMs: number; // Duration user has to react
  difficulty: DifficultyLevel;
  throwOptions?: Array<{ target: ThrowTarget; urgency: number; label: string }>;
}

export interface FieldingEventOutcome {
  eventType: FieldingEventType;
  isSuccess: boolean;
  reactionTimeMs: number;
  runsSaved: number;
  isWicket: boolean;
  isCatch: boolean;
  isRunOut: boolean;
  confidenceDelta: number;
  commentary: string;
}

export class FieldingSystem {
  /**
   * Generates a random fielding event prompt for the user during AI simulation.
   */
  public static generateFieldingEvent(
    player: PlayerData,
    batsmanName: string,
    difficulty: DifficultyLevel = DifficultyLevel.NORMAL
  ): FieldingEventPrompt {
    const config = DIFFICULTY_CONFIGS[difficulty];
    const isKeeper = player.role === PlayerRole.WICKETKEEPER;

    // Attribute modifiers
    const reflexBonus = (player.fielding.reflexes - 50) * 4; // -100ms to +160ms
    const reactionWindowMs = Math.max(400, config.fieldingReactionWindowMs + reflexBonus);

    const id = `field_${Date.now()}`;
    const roll = Math.random();

    if (isKeeper) {
      if (roll < 0.65) {
        return {
          id,
          type: FieldingEventType.CATCH_OPPORTUNITY,
          title: '🧤 EDGED BEHIND! SNICKOMETER DETECTED!',
          description: `Thick edge flies towards your right glove! React to pouch the catch!`,
          batsmanName,
          reactionWindowMs,
          difficulty
        };
      } else {
        return {
          id,
          type: FieldingEventType.RUN_OUT_CHANCE,
          title: '⚡ STUMPING / RUN-OUT OPPORTUNITY!',
          description: `${batsmanName} is dragged out of the crease! React to whip off the bails!`,
          batsmanName,
          reactionWindowMs: Math.round(reactionWindowMs * 0.85),
          difficulty,
          throwOptions: [
            { target: 'KEEPER_END', urgency: 1, label: 'Safe Gather & Dislodge Bails' },
            { target: 'DIRECT_HIT', urgency: 3, label: 'Flash-Speed Blind Stumping' }
          ]
        };
      }
    }

    if (roll < 0.40) {
      return {
        id,
        type: FieldingEventType.CATCH_OPPORTUNITY,
        title: '🔥 CATCHING CHANCE! IN THE AIR!',
        description: `${batsmanName} slices high towards your position! Lock eyes and pouch it!`,
        batsmanName,
        reactionWindowMs,
        difficulty
      };
    } else if (roll < 0.70) {
      return {
        id,
        type: FieldingEventType.RUN_OUT_CHANCE,
        title: '🎯 RUN OUT CHANCE! CHOOSE TARGET & THROW!',
        description: `Hesitation between batsmen! Choose your target end and urgency to break the stumps!`,
        batsmanName,
        reactionWindowMs,
        difficulty,
        throwOptions: [
          { target: 'BOWLER_END', urgency: 1, label: '1: Controlled throw to Bowler' },
          { target: 'KEEPER_END', urgency: 2, label: '2: Firm throw to Keeper' },
          { target: 'DIRECT_HIT', urgency: 3, label: '3: Direct-Hit Rocket at the Stumps' }
        ]
      };
    } else {
      return {
        id,
        type: FieldingEventType.BOUNDARY_STOP,
        title: '⚡ BOUNDARY SAVE! DIVING STOP!',
        description: `Blistering drive racing toward the rope! Dive to cut off the boundary!`,
        batsmanName,
        reactionWindowMs: Math.round(reactionWindowMs * 1.1),
        difficulty
      };
    }
  }

  /**
   * Resolves the user's reaction to the fielding prompt.
   */
  public static resolveReaction(
    prompt: FieldingEventPrompt,
    reactionTimeMs: number, // 0 = didn't react in time
    player: PlayerData,
    selectedTarget: ThrowTarget = 'DIRECT_HIT'
  ): FieldingEventOutcome {
    const reactedInTime = reactionTimeMs > 0 && reactionTimeMs <= prompt.reactionWindowMs;

    switch (prompt.type) {
      case FieldingEventType.CATCH_OPPORTUNITY: {
        const catchSkill = player.fielding.catching;
        // Even if reacted in time, catch skill influences sticking chance
        const dropChance = catchSkill >= 90 ? 0 : Math.max(0.04, (100 - catchSkill) * 0.003);
        const caught = reactedInTime && Math.random() >= dropChance;

        if (caught) {
          return {
            eventType: prompt.type,
            isSuccess: true,
            reactionTimeMs,
            runsSaved: 0,
            isWicket: true,
            isCatch: true,
            isRunOut: false,
            confidenceDelta: +5,
            commentary: `SENSATIONAL CATCH! ${player.firstName} ${player.lastName} takes a beauty with lightning reaction (${reactionTimeMs}ms)! ${prompt.batsmanName} is OUT!`
          };
        } else {
          return {
            eventType: prompt.type,
            isSuccess: false,
            reactionTimeMs,
            runsSaved: 0,
            isWicket: false,
            isCatch: false,
            isRunOut: false,
            confidenceDelta: -4,
            commentary: `DROPPED! ${player.firstName} ${player.lastName} puts down a regulation catch! A painful missed opportunity!`
          };
        }
      }

      case FieldingEventType.RUN_OUT_CHANCE: {
        const throwSkill = player.fielding.throwing;
        let hitSuccessThreshold = throwSkill / 100;

        if (selectedTarget === 'DIRECT_HIT') {
          hitSuccessThreshold *= 0.70; // Harder to hit direct, but instant wicket
        } else {
          hitSuccessThreshold *= 0.90; // Easier throw to keeper/bowler
        }

        const hitStumps = reactedInTime && Math.random() < hitSuccessThreshold;

        if (hitStumps) {
          return {
            eventType: prompt.type,
            isSuccess: true,
            reactionTimeMs,
            runsSaved: 1,
            isWicket: true,
            isCatch: false,
            isRunOut: true,
            confidenceDelta: +6,
            commentary: selectedTarget === 'DIRECT_HIT'
              ? `DIRECT HIT! Bullseye from ${player.firstName} ${player.lastName}! ${prompt.batsmanName} is caught well short! OUT!`
              : `SHARP THROW to the stumps! Fielder breaks the wicket in time! ${prompt.batsmanName} is RUN OUT!`
          };
        } else {
          return {
            eventType: prompt.type,
            isSuccess: false,
            reactionTimeMs,
            runsSaved: 0,
            isWicket: false,
            isCatch: false,
            isRunOut: false,
            confidenceDelta: -1,
            commentary: `Throw misses the stumps by inches. Batsman scrambles home safely.`
          };
        }
      }

      case FieldingEventType.BOUNDARY_STOP:
      case FieldingEventType.GROUND_FIELDING: {
        const fieldSkill = player.fielding.fielding;
        const stopped = reactedInTime && Math.random() < (fieldSkill / 110);

        if (stopped) {
          return {
            eventType: prompt.type,
            isSuccess: true,
            reactionTimeMs,
            runsSaved: 2,
            isWicket: false,
            isCatch: false,
            isRunOut: false,
            confidenceDelta: +3,
            commentary: `MAGNIFICENT STOP! ${player.firstName} ${player.lastName} dives full length on the boundary rope, saving 2 crucial runs!`
          };
        } else {
          return {
            eventType: prompt.type,
            isSuccess: false,
            reactionTimeMs,
            runsSaved: 0,
            isWicket: false,
            isCatch: false,
            isRunOut: false,
            confidenceDelta: -2,
            commentary: `Fumbled! The ball slips through into the outfield.`
          };
        }
      }
    }
  }
}
