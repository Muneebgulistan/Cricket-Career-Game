import { CareerLevel, CAREER_LEVEL_ORDER, CAREER_LEVEL_CONFIG, CareerLevelMetadata } from './CareerLevel';
import { CareerStatus } from './CareerStatus';
import { PlayerData, PlayerRole } from '../player/PlayerModel';
import { RatingCalculator } from '../player/RatingCalculator';

export interface PromotionCheckResult {
  canPromote: boolean;
  currentLevel: CareerLevel;
  nextLevel: CareerLevel | null;
  missingRequirements: string[];
  achievedRequirements: string[];
}

export class CareerProgression {
  /**
   * Returns the previous level in the career ladder, or null if at grassroot.
   */
  public static getPreviousLevel(currentLevel: CareerLevel): CareerLevel | null {
    const index = CAREER_LEVEL_ORDER.indexOf(currentLevel);
    if (index > 0) {
      return CAREER_LEVEL_ORDER[index - 1];
    }
    return null;
  }

  /**
   * Returns the next possible level in the career ladder, or null if at the peak.
   */
  public static getNextLevel(currentLevel: CareerLevel): CareerLevel | null {
    const index = CAREER_LEVEL_ORDER.indexOf(currentLevel);
    if (index >= 0 && index < CAREER_LEVEL_ORDER.length - 1) {
      return CAREER_LEVEL_ORDER[index + 1];
    }
    return null;
  }

  /**
   * Retrieves full metadata for a given level.
   */
  public static getLevelMetadata(level: CareerLevel): CareerLevelMetadata {
    return CAREER_LEVEL_CONFIG[level];
  }

  /**
   * Evaluates whether the player has earned a promotion to the next tier.
   * Progression is NOT automatic: it evaluates actual matches played at current level,
   * runs/average or wickets/economy depending on role, overall rating, and current form.
   */
  public static checkPromotion(player: PlayerData, levelMatches: number, levelRuns: number, levelWickets: number): PromotionCheckResult {
    const current = player.career.currentCareerLevel;
    const next = this.getNextLevel(current);

    if (!next) {
      return {
        canPromote: false,
        currentLevel: current,
        nextLevel: null,
        missingRequirements: ['Already at the peak pinnacle level (ODI World Cup).'],
        achievedRequirements: ['All tiers completed.']
      };
    }

    const config = CAREER_LEVEL_CONFIG[current];
    const criteria = config.promotionCriteria;

    const missing: string[] = [];
    const achieved: string[] = [];

    // Check matches played at this level
    if (levelMatches >= criteria.minMatches) {
      achieved.push(`Matches played: ${levelMatches}/${criteria.minMatches}`);
    } else {
      missing.push(`Needs more experience: ${levelMatches}/${criteria.minMatches} matches`);
    }

    // Role-specific statistical benchmarks
    if (player.role === PlayerRole.BATSMAN || player.role === PlayerRole.WICKETKEEPER) {
      if (criteria.minRuns && levelRuns >= criteria.minRuns) {
        achieved.push(`Runs scored: ${levelRuns}/${criteria.minRuns}`);
      } else if (criteria.minRuns) {
        missing.push(`Runs milestone: ${levelRuns}/${criteria.minRuns} runs`);
      }
    } else if (player.role === PlayerRole.BOWLER) {
      if (criteria.minWickets && levelWickets >= criteria.minWickets) {
        achieved.push(`Wickets taken: ${levelWickets}/${criteria.minWickets}`);
      } else if (criteria.minWickets) {
        missing.push(`Wicket tally: ${levelWickets}/${criteria.minWickets} wickets`);
      }
    } else if (player.role === PlayerRole.ALL_ROUNDER) {
      // All-rounders need a balanced portion of runs and wickets
      const halfRuns = Math.round((criteria.minRuns ?? 100) * 0.65);
      const halfWickets = Math.round((criteria.minWickets ?? 4) * 0.65);
      if (levelRuns >= halfRuns && levelWickets >= halfWickets) {
        achieved.push(`All-round contribution: ${levelRuns} runs & ${levelWickets} wkts`);
      } else {
        missing.push(`All-rounder milestone: ${levelRuns}/${halfRuns} runs & ${levelWickets}/${halfWickets} wkts`);
      }
    }

    // Overall Rating check
    const currentRating = player.potential.overallRating;
    if (currentRating >= criteria.minOverallRating) {
      achieved.push(`Player Overall Rating: ${currentRating}/${criteria.minOverallRating}`);
    } else {
      missing.push(`Develop attributes: Rating ${currentRating}/${criteria.minOverallRating}`);
    }

    // Form check
    if (player.mental.form >= criteria.minForm) {
      achieved.push(`In-form performance: Form ${player.mental.form}/${criteria.minForm}`);
    } else {
      missing.push(`Form consistency: Current Form ${player.mental.form}/${criteria.minForm}`);
    }

    return {
      canPromote: missing.length === 0,
      currentLevel: current,
      nextLevel: next,
      missingRequirements: missing,
      achievedRequirements: achieved
    };
  }

  /**
   * Promotes the player to the next tier, updating their team, level, age if necessary,
   * boosting confidence, and setting status to PROMOTED.
   */
  public static promote(player: PlayerData): boolean {
    const next = this.getNextLevel(player.career.currentCareerLevel);
    if (!next) return false;

    player.career.currentCareerLevel = next;
    player.career.careerStatus = CareerStatus.PROMOTED;
    player.mental.confidence = Math.min(100, player.mental.confidence + 15);
    player.mental.form = Math.min(100, player.mental.form + 10);

    // Realistic age progression across major career jumps
    if (next === CareerLevel.UNDER_19 && player.age < 17) player.age = 17;
    else if (next === CareerLevel.DOMESTIC && player.age < 18) player.age = 18;
    else if (next === CareerLevel.COUNTRY_LEAGUE && player.age < 19) player.age = 19;
    else if (next === CareerLevel.INTERNATIONAL_HOME && player.age < 20) player.age = 20;

    return true;
  }

  /**
   * Demotes or drops the player if performance suffers persistently.
   */
  public static demote(player: PlayerData): boolean {
    const prev = this.getPreviousLevel(player.career.currentCareerLevel);
    if (!prev) {
      player.career.careerStatus = CareerStatus.DROPPED;
      return false;
    }

    player.career.currentCareerLevel = prev;
    player.career.careerStatus = CareerStatus.DROPPED;
    player.mental.confidence = Math.max(10, player.mental.confidence - 20);
    return true;
  }

  /**
   * Apply attribute development points / training.
   */
  public static train(player: PlayerData, focus: 'BATTING' | 'BOWLING' | 'FITNESS' | 'MENTAL'): void {
    switch (focus) {
      case 'BATTING':
        player.batting.battingTechnique = Math.min(99, player.batting.battingTechnique + 1);
        player.batting.timing = Math.min(99, player.batting.timing + 1);
        player.batting.battingAbility = Math.min(99, player.batting.battingAbility + 1);
        player.mental.fitness = Math.max(10, player.mental.fitness - 5);
        break;
      case 'BOWLING':
        player.bowling.bowlingAbility = Math.min(99, player.bowling.bowlingAbility + 1);
        player.bowling.accuracy = Math.min(99, player.bowling.accuracy + 1);
        player.bowling.variation = Math.min(99, player.bowling.variation + 1);
        player.mental.fitness = Math.max(10, player.mental.fitness - 5);
        break;
      case 'FITNESS':
        player.mental.fitness = Math.min(100, player.mental.fitness + 20);
        player.fielding.fielding = Math.min(99, player.fielding.fielding + 1);
        break;
      case 'MENTAL':
        player.mental.confidence = Math.min(100, player.mental.confidence + 15);
        player.mental.pressureHandling = Math.min(99, player.mental.pressureHandling + 2);
        break;
    }

    // Recalculate overall rating
    player.potential.overallRating = RatingCalculator.calculateOverall(player);
  }
}
