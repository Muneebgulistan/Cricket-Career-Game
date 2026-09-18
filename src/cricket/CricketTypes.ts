export enum MatchFormat {
  YOUTH_40 = 'YOUTH_40',
  YOUTH_50 = 'YOUTH_50',
  T20 = 'T20',
  LIST_A = 'LIST_A',
  FIRST_CLASS = 'FIRST_CLASS',
  ODI = 'ODI',
  TEST = 'TEST'
}

export interface MatchFormatRules {
  name: string;
  totalOvers: number;
  maxOversPerBowler: number;
  inningsPerSide: number;
  powerplayOvers: number;
}

export const MATCH_FORMAT_RULES: Record<MatchFormat, MatchFormatRules> = {
  [MatchFormat.YOUTH_40]: {
    name: 'Youth 40 Overs',
    totalOvers: 40,
    maxOversPerBowler: 8,
    inningsPerSide: 1,
    powerplayOvers: 8
  },
  [MatchFormat.YOUTH_50]: {
    name: 'Youth 50 Overs',
    totalOvers: 50,
    maxOversPerBowler: 10,
    inningsPerSide: 1,
    powerplayOvers: 10
  },
  [MatchFormat.T20]: {
    name: 'Twenty20 (T20)',
    totalOvers: 20,
    maxOversPerBowler: 4,
    inningsPerSide: 1,
    powerplayOvers: 6
  },
  [MatchFormat.LIST_A]: {
    name: 'List-A (50 Overs)',
    totalOvers: 50,
    maxOversPerBowler: 10,
    inningsPerSide: 1,
    powerplayOvers: 10
  },
  [MatchFormat.FIRST_CLASS]: {
    name: 'First-Class (4 Days)',
    totalOvers: 90,
    maxOversPerBowler: 99,
    inningsPerSide: 2,
    powerplayOvers: 0
  },
  [MatchFormat.ODI]: {
    name: 'One Day International (ODI)',
    totalOvers: 50,
    maxOversPerBowler: 10,
    inningsPerSide: 1,
    powerplayOvers: 10
  },
  [MatchFormat.TEST]: {
    name: 'Test Match (5 Days)',
    totalOvers: 90,
    maxOversPerBowler: 99,
    inningsPerSide: 2,
    powerplayOvers: 0
  }
};

export enum DismissalType {
  NOT_OUT = 'Not Out',
  BOWLED = 'Bowled',
  CAUGHT = 'Caught',
  LBW = 'LBW',
  RUN_OUT = 'Run Out',
  STUMPED = 'Stumped',
  HIT_WICKET = 'Hit Wicket'
}

export interface BallOutcome {
  overNumber: number;
  ballInOver: number;
  runs: number;
  isExtra: boolean;
  extraType?: 'wide' | 'no-ball' | 'bye' | 'leg-bye';
  isWicket: boolean;
  dismissalType?: DismissalType;
  dismissedPlayerName?: string;
  bowlerName: string;
  strikerName: string;
  commentary: string;
  isBoundaryFour: boolean;
  isBoundarySix: boolean;
}
