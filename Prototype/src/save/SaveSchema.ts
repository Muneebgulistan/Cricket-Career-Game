import { PlayerData } from '../player/PlayerModel';
import { TournamentInstance } from '../tournament/TournamentModel';
import { CareerStatisticsContainer } from '../player/CareerStatistics';
import { WeeklySchedule } from '../career/CareerCalendar';
import { FatigueState } from '../career/FatigueSystem';
import { ActiveInjury } from '../career/InjurySystem';
import { UnlockedAchievementRecord } from '../career/AchievementSystem';
import { DifficultyLevel } from '../career/Difficulty';

export interface GameSettings {
  soundEnabled: boolean;
  soundVolume: number;
  simulationSpeed: 'FAST' | 'NORMAL' | 'BALL_BY_BALL';
  commentaryEnabled: boolean;
  autoSave: boolean;
  difficulty: DifficultyLevel;
}

export interface MatchHistoryEntry {
  matchId: string;
  date: string;
  tournamentName: string;
  opponentName: string;
  resultSummary: string;
  playerRuns: number;
  playerWickets: number;
  matchRating: number;
}

export interface SaveDataV1 {
  version: 1;
  saveId: string;
  timestamp: number;
  saveName: string;
  player: PlayerData;
  tournament: TournamentInstance | null;
  statistics: CareerStatisticsContainer;
  matchHistory: MatchHistoryEntry[];
  recentRatings: number[];
  achievements: string[];
  settings: GameSettings;
}

export interface SaveDataV2 {
  version: 2;
  saveId: string;
  timestamp: number;
  saveName: string;
  player: PlayerData;
  tournament: TournamentInstance | null;
  statistics: CareerStatisticsContainer;
  matchHistory: MatchHistoryEntry[];
  recentRatings: number[];
  achievements: string[];
  unlockedAchievements: UnlockedAchievementRecord[];
  calendar: WeeklySchedule;
  fatigue: FatigueState;
  activeInjury: ActiveInjury | null;
  settings: GameSettings;
}

export type AnySaveData = SaveDataV1 | SaveDataV2;
export const CURRENT_SAVE_VERSION = 2;
