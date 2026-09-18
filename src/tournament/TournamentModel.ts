import { CareerLevel } from '../career/CareerLevel';
import { MatchFormat } from '../cricket/CricketTypes';

export enum TournamentStatus {
  UPCOMING = 'UPCOMING',
  IN_PROGRESS = 'IN_PROGRESS',
  COMPLETED = 'COMPLETED'
}

export interface TeamStanding {
  teamId: string;
  teamName: string;
  played: number;
  won: number;
  lost: number;
  tied: number;
  points: number;
  netRunRate: number;
}

export interface TournamentFixture {
  id: string;
  roundNumber: number;
  teamAId: string;
  teamAName: string;
  teamBId: string;
  teamBName: string;
  venueName: string;
  isPlayed: boolean;
  matchId?: string;
  winnerTeamId?: string;
  resultSummary?: string;
}

export interface TournamentInstance {
  id: string;
  templateId: string;
  name: string;
  type: 'CUP' | 'LEAGUE' | 'BILATERAL_SERIES' | 'WORLD_CUP';
  careerLevel: CareerLevel;
  format: MatchFormat;
  status: TournamentStatus;
  playerTeamId: string;
  totalMatches: number;
  completedMatches: number;
  currentRound: number;
  fixtures: TournamentFixture[];
  standings: TeamStanding[];
  championTeamId?: string;
  championTeamName?: string;
  trophyName: string;
  minimumPerformanceRequirement: number;
}
