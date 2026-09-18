import { CareerLevel } from '../career/CareerLevel';
import { CareerStatus } from '../career/CareerStatus';

export enum PlayerRole {
  BATSMAN = 'Batsman',
  BOWLER = 'Bowler',
  ALL_ROUNDER = 'All-rounder',
  WICKETKEEPER = 'Wicketkeeper'
}

export enum BattingStyle {
  RIGHT_HAND = 'Right-hand bat',
  LEFT_HAND = 'Left-hand bat'
}

export enum BowlingStyle {
  RIGHT_ARM_FAST = 'Right-arm fast',
  LEFT_ARM_FAST = 'Left-arm fast',
  RIGHT_ARM_MEDIUM = 'Right-arm medium',
  LEFT_ARM_MEDIUM = 'Left-arm medium',
  RIGHT_ARM_OFF_SPIN = 'Right-arm off-spin',
  RIGHT_ARM_LEG_SPIN = 'Right-arm leg-spin',
  LEFT_ARM_ORTHODOX = 'Slow left-arm orthodox',
  LEFT_ARM_UNORTHODOX = 'Left-arm unorthodox',
  NONE = 'None'
}

export interface BattingAttributes {
  battingAbility: number;       // 1 - 100
  battingTechnique: number;     // 1 - 100
  timing: number;               // 1 - 100
  power: number;                // 1 - 100
  runningBetweenWickets: number;// 1 - 100
}

export interface BowlingAttributes {
  bowlingAbility: number;       // 1 - 100
  pace: number;                 // 1 - 100 (speed)
  swing: number;                // 1 - 100
  seam: number;                 // 1 - 100
  spin: number;                 // 1 - 100
  accuracy: number;             // 1 - 100
  variation: number;            // 1 - 100
}

export interface FieldingAttributes {
  fielding: number;             // 1 - 100
  catching: number;             // 1 - 100
  throwing: number;             // 1 - 100
  reflexes: number;             // 1 - 100
}

export interface MentalAttributes {
  confidence: number;           // 0 - 100
  fitness: number;              // 0 - 100
  form: number;                 // 0 - 100
  pressureHandling: number;     // 1 - 100
}

export interface PotentialAttributes {
  overallRating: number;        // 1 - 100
  potentialRating: number;      // 1 - 100
  developmentRate: number;      // Multiplier e.g. 1.0 - 2.0
}

export interface PlayerCareerSummary {
  currentCareerLevel: CareerLevel;
  currentTeam: string;
  currentTournament: string;
  careerStatus: CareerStatus;
  matchesPlayed: number;
  runs: number;
  wickets: number;
  catches: number;
  average: number;
  strikeRate: number;
  economy: number;
}

export interface PlayerData {
  id: string;
  firstName: string;
  lastName: string;
  age: number;
  nationality: string;
  jerseyNumber: number;
  role: PlayerRole;
  battingStyle: BattingStyle;
  bowlingStyle: BowlingStyle;

  batting: BattingAttributes;
  bowling: BowlingAttributes;
  fielding: FieldingAttributes;
  mental: MentalAttributes;
  potential: PotentialAttributes;
  career: PlayerCareerSummary;
}
