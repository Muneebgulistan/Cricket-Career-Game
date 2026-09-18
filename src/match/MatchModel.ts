import { MatchFormat } from '../cricket/CricketTypes';
import { PerformanceEvaluationResult } from '../career/PerformanceEvaluator';
import { DeliveryLine, DeliveryLength, BattingShot, ShotDirection } from './BattingEngine';
import { BowlingVariation } from './BowlingEngine';
import { FieldingEventPrompt } from './FieldingSystem';
import { DeliveryTrajectory } from './BallPhysics';
import { RunningOpportunity } from './RunningSystem';
import { MatchConditions, TossResult } from './MatchPresentation';
import { FieldPreset } from './CricketFieldView';

export interface BatsmanInningsEntry {
  playerId: string;
  name: string;
  runs: number;
  balls: number;
  fours: number;
  sixes: number;
  isOut: boolean;
  dismissalText: string;
  bowlerName?: string;
  fielderName?: string;
}

export interface BowlerInningsEntry {
  playerId: string;
  name: string;
  overs: number;
  maidens: number;
  runs: number;
  wickets: number;
  economy: number;
}

export interface InningsScorecard {
  battingTeamId: string;
  battingTeamName: string;
  bowlingTeamId: string;
  bowlingTeamName: string;
  totalRuns: number;
  totalWickets: number;
  oversCompleted: number;
  ballsInCurrentOver: number;
  isCompleted: boolean;
  batsmen: BatsmanInningsEntry[];
  bowlers: BowlerInningsEntry[];
  extras: {
    wides: number;
    noBalls: number;
    byes: number;
    legByes: number;
    total: number;
  };
}

export interface PlayerMatchPerformance {
  playerId: string;
  playerName: string;
  isPlayerTeamWinner: boolean;

  // Batting
  didBat: boolean;
  runs: number;
  balls: number;
  fours: number;
  sixes: number;
  isOut: boolean;
  dismissal: string;

  // Bowling
  didBowl: boolean;
  overs: number;
  maidens: number;
  runsConceded: number;
  wickets: number;

  // Fielding
  catches: number;
  runOuts: number;
  stumpings: number;

  // Evaluation
  evaluation?: PerformanceEvaluationResult;
}

export type InteractivePhase =
  | 'USER_BATTING'
  | 'USER_BOWLING'
  | 'USER_FIELDING'
  | 'RUNNING_DECISION'
  | 'AI_INNINGS'
  | 'INNINGS_BREAK'
  | 'MATCH_CONCLUDED';

export interface InteractiveMatchState {
  phase: InteractivePhase;
  strikerIdx: number;
  nonStrikerIdx: number;
  nextBatsmanIdx: number;
  bowlerIdx: number;
  currentOver: number;
  currentBallInOver: number;
  runsInCurrentOver: number;
  battingLineup: Array<{ id: string; name: string; isUser: boolean; ability: number }>;
  bowlingAttack: Array<{ id: string; name: string; isUser: boolean; ability: number; variation: BowlingVariation }>;
  pendingDelivery?: {
    line: DeliveryLine;
    length: DeliveryLength;
    paceKph: number;
    bowlerName: string;
    variation: BowlingVariation;
    trajectory?: DeliveryTrajectory;
  };
  pendingFieldingPrompt?: FieldingEventPrompt;
  pendingRunningOpp?: RunningOpportunity;
  lastDeliverySummary?: string;
  isUserBattingTeam: boolean;
  isUserBowlingTeam: boolean;
  currentFieldPreset?: FieldPreset;
  isPowerplay?: boolean;
  partnershipRuns?: number;
  partnershipBalls?: number;
  recentBallsTimeline?: string[];
}

export interface MatchInstance {
  id: string;
  tournamentId: string;
  tournamentName: string;
  fixtureId: string;
  teamA: { id: string; name: string; shortName: string };
  teamB: { id: string; name: string; shortName: string };
  venue: string;
  date: string;
  format: MatchFormat;
  oversPerSide: number;
  currentInningsIndex: number; // 0 or 1
  innings: [InningsScorecard, InningsScorecard];
  targetRuns?: number;
  resultSummary?: string;
  winnerTeamId?: string;
  isCompleted: boolean;
  manOfTheMatchPlayerName?: string;
  playerPerformance: PlayerMatchPerformance;
  commentaryLog: string[];
  interactiveState?: InteractiveMatchState;
  conditions?: MatchConditions;
  tossResult?: TossResult;
}
