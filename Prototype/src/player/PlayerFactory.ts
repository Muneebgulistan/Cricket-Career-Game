import {
  PlayerData,
  PlayerRole,
  BattingStyle,
  BowlingStyle,
  BattingAttributes,
  BowlingAttributes,
  FieldingAttributes,
  MentalAttributes,
  PotentialAttributes
} from './PlayerModel';
import { CareerLevel } from '../career/CareerLevel';
import { CareerStatus } from '../career/CareerStatus';
import { RatingCalculator } from './RatingCalculator';

export interface PlayerCreationParams {
  firstName: string;
  lastName: string;
  age?: number;
  nationality: string;
  jerseyNumber?: number;
  role: PlayerRole;
  battingStyle: BattingStyle;
  bowlingStyle?: BowlingStyle;
}

export class PlayerFactory {
  /**
   * Generates a realistic starting Under-16 prodigy based on user creation selections.
   */
  public static createUnder16Player(params: PlayerCreationParams): PlayerData {
    const id = 'player_' + Date.now() + '_' + Math.floor(Math.random() * 1000);
    const age = params.age ?? 15;
    const jerseyNumber = params.jerseyNumber ?? Math.floor(Math.random() * 99) + 1;
    const bowlingStyle = params.bowlingStyle ?? (
      params.role === PlayerRole.BOWLER || params.role === PlayerRole.ALL_ROUNDER
        ? BowlingStyle.RIGHT_ARM_FAST
        : BowlingStyle.NONE
    );

    // Baseline stats for a 15-year-old youth player
    let batting: BattingAttributes = {
      battingAbility: 38,
      battingTechnique: 36,
      timing: 38,
      power: 35,
      runningBetweenWickets: 50
    };

    let bowling: BowlingAttributes = {
      bowlingAbility: 25,
      pace: 25,
      swing: 25,
      seam: 25,
      spin: 20,
      accuracy: 25,
      variation: 20
    };

    let fielding: FieldingAttributes = {
      fielding: 45,
      catching: 46,
      throwing: 44,
      reflexes: 48
    };

    let mental: MentalAttributes = {
      confidence: 60,
      fitness: 75,
      form: 65,
      pressureHandling: 40
    };

    // Role-specific attribute boosts for starting archetype
    switch (params.role) {
      case PlayerRole.BATSMAN:
        batting = {
          battingAbility: 52,
          battingTechnique: 50,
          timing: 51,
          power: 48,
          runningBetweenWickets: 55
        };
        break;

      case PlayerRole.BOWLER:
        bowling = {
          bowlingAbility: 54,
          pace: bowlingStyle.includes('fast') ? 56 : 28,
          swing: bowlingStyle.includes('fast') ? 52 : 30,
          seam: bowlingStyle.includes('fast') ? 50 : 25,
          spin: bowlingStyle.includes('spin') || bowlingStyle.includes('orthodox') ? 56 : 15,
          accuracy: 50,
          variation: 46
        };
        break;

      case PlayerRole.ALL_ROUNDER:
        batting = {
          battingAbility: 48,
          battingTechnique: 46,
          timing: 46,
          power: 46,
          runningBetweenWickets: 52
        };
        bowling = {
          bowlingAbility: 48,
          pace: bowlingStyle.includes('fast') ? 48 : 25,
          swing: 45,
          seam: 45,
          spin: bowlingStyle.includes('spin') ? 48 : 20,
          accuracy: 47,
          variation: 44
        };
        break;

      case PlayerRole.WICKETKEEPER:
        batting = {
          battingAbility: 49,
          battingTechnique: 48,
          timing: 48,
          power: 44,
          runningBetweenWickets: 58
        };
        fielding = {
          fielding: 50,
          catching: 58,
          throwing: 48,
          reflexes: 58
        };
        break;
    }

    const overallRating = RatingCalculator.calculateOverall({
      role: params.role,
      batting,
      bowling,
      fielding,
      mental
    });

    const potential: PotentialAttributes = {
      overallRating,
      potentialRating: Math.min(95, overallRating + Math.floor(Math.random() * 8) + 32),
      developmentRate: 1.25
    };

    return {
      id,
      firstName: params.firstName.trim(),
      lastName: params.lastName.trim(),
      age,
      nationality: params.nationality.trim(),
      jerseyNumber,
      role: params.role,
      battingStyle: params.battingStyle,
      bowlingStyle,
      batting,
      bowling,
      fielding,
      mental,
      potential,
      career: {
        currentCareerLevel: CareerLevel.UNDER_16,
        currentTeam: `${params.nationality} Under-16s`,
        currentTournament: 'Under-16 Youth State Cup',
        careerStatus: CareerStatus.YOUTH_PLAYER,
        matchesPlayed: 0,
        runs: 0,
        wickets: 0,
        catches: 0,
        average: 0,
        strikeRate: 0,
        economy: 0
      }
    };
  }
}
