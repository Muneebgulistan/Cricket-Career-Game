import { CareerLevel } from '../career/CareerLevel';

export interface BattingStats {
  matches: number;
  innings: number;
  notOuts: number;
  runs: number;
  ballsFaced: number;
  highestScore: number;
  isHighestScoreNotOut: boolean;
  average: number;
  strikeRate: number;
  fifties: number;
  hundreds: number;
  fours: number;
  sixes: number;
  ducks: number;
}

export interface BowlingStats {
  matches: number;
  overs: number;
  maidens: number;
  runsConceded: number;
  wickets: number;
  bestBowlingRuns: number;
  bestBowlingWickets: number;
  average: number;
  economy: number;
  threeWicketHauls: number;
  fiveWicketHauls: number;
}

export interface FieldingStats {
  catches: number;
  runOuts: number;
  stumpings: number;
}

export interface LevelStatisticsBreakdown {
  level: CareerLevel;
  batting: BattingStats;
  bowling: BowlingStats;
  fielding: FieldingStats;
}

export interface TournamentStatisticsRecord {
  tournamentId: string;
  tournamentName: string;
  level: CareerLevel;
  year: number;
  batting: BattingStats;
  bowling: BowlingStats;
  fielding: FieldingStats;
}

export interface CareerStatisticsContainer {
  allTime: {
    batting: BattingStats;
    bowling: BowlingStats;
    fielding: FieldingStats;
  };
  byLevel: Record<string, LevelStatisticsBreakdown>;
  byTournament: TournamentStatisticsRecord[];
}

export class CareerStatisticsTracker {
  public static createEmptyBattingStats(): BattingStats {
    return {
      matches: 0,
      innings: 0,
      notOuts: 0,
      runs: 0,
      ballsFaced: 0,
      highestScore: 0,
      isHighestScoreNotOut: false,
      average: 0.0,
      strikeRate: 0.0,
      fifties: 0,
      hundreds: 0,
      fours: 0,
      sixes: 0,
      ducks: 0
    };
  }

  public static createEmptyBowlingStats(): BowlingStats {
    return {
      matches: 0,
      overs: 0,
      maidens: 0,
      runsConceded: 0,
      wickets: 0,
      bestBowlingRuns: 0,
      bestBowlingWickets: 0,
      average: 0.0,
      economy: 0.0,
      threeWicketHauls: 0,
      fiveWicketHauls: 0
    };
  }

  public static createEmptyFieldingStats(): FieldingStats {
    return {
      catches: 0,
      runOuts: 0,
      stumpings: 0
    };
  }

  public static createInitialContainer(): CareerStatisticsContainer {
    return {
      allTime: {
        batting: this.createEmptyBattingStats(),
        bowling: this.createEmptyBowlingStats(),
        fielding: this.createEmptyFieldingStats()
      },
      byLevel: {},
      byTournament: []
    };
  }

  /**
   * Applies match performance to all-time, level-specific, and tournament-specific stats.
   */
  public static recordMatch(
    container: CareerStatisticsContainer,
    level: CareerLevel,
    tournamentId: string,
    tournamentName: string,
    perf: {
      runs: number;
      balls: number;
      fours: number;
      sixes: number;
      isOut: boolean;
      didBat: boolean;
      overs: number;
      maidens: number;
      runsConceded: number;
      wickets: number;
      didBowl: boolean;
      catches: number;
      runOuts: number;
      stumpings: number;
    }
  ): void {
    // 1. Ensure Level Breakdown exists
    if (!container.byLevel[level]) {
      container.byLevel[level] = {
        level,
        batting: this.createEmptyBattingStats(),
        bowling: this.createEmptyBowlingStats(),
        fielding: this.createEmptyFieldingStats()
      };
    }
    const levelStats = container.byLevel[level];

    // 2. Ensure Tournament Record exists
    let tourneyRecord = container.byTournament.find(t => t.tournamentId === tournamentId);
    if (!tourneyRecord) {
      tourneyRecord = {
        tournamentId,
        tournamentName,
        level,
        year: new Date().getFullYear(),
        batting: this.createEmptyBattingStats(),
        bowling: this.createEmptyBowlingStats(),
        fielding: this.createEmptyFieldingStats()
      };
      container.byTournament.push(tourneyRecord);
    }

    // Helper to update Batting Stats
    const updateBatting = (b: BattingStats) => {
      b.matches++;
      if (perf.didBat) {
        b.innings++;
        b.runs += perf.runs;
        b.ballsFaced += perf.balls;
        b.fours += perf.fours;
        b.sixes += perf.sixes;

        if (!perf.isOut) {
          b.notOuts++;
        } else if (perf.runs === 0) {
          b.ducks++;
        }

        if (perf.runs > b.highestScore || (perf.runs === b.highestScore && !perf.isOut)) {
          b.highestScore = perf.runs;
          b.isHighestScoreNotOut = !perf.isOut;
        }

        if (perf.runs >= 100) {
          b.hundreds++;
        } else if (perf.runs >= 50) {
          b.fifties++;
        }

        const dismissals = b.innings - b.notOuts;
        b.average = dismissals > 0 ? Math.round((b.runs / dismissals) * 100) / 100 : b.runs;
        b.strikeRate = b.ballsFaced > 0 ? Math.round((b.runs / b.ballsFaced) * 10000) / 100 : 0.0;
      }
    };

    // Helper to update Bowling Stats
    const updateBowling = (bw: BowlingStats) => {
      bw.matches++;
      if (perf.didBowl) {
        bw.overs += perf.overs;
        bw.maidens += perf.maidens;
        bw.runsConceded += perf.runsConceded;
        bw.wickets += perf.wickets;

        if (perf.wickets >= 5) {
          bw.fiveWicketHauls++;
        } else if (perf.wickets >= 3) {
          bw.threeWicketHauls++;
        }

        // Check best bowling
        if (
          perf.wickets > bw.bestBowlingWickets ||
          (perf.wickets === bw.bestBowlingWickets && perf.runsConceded < bw.bestBowlingRuns) ||
          bw.bestBowlingWickets === 0
        ) {
          bw.bestBowlingWickets = perf.wickets;
          bw.bestBowlingRuns = perf.runsConceded;
        }

        bw.average = bw.wickets > 0 ? Math.round((bw.runsConceded / bw.wickets) * 100) / 100 : 0.0;
        bw.economy = bw.overs > 0 ? Math.round((bw.runsConceded / bw.overs) * 100) / 100 : 0.0;
      }
    };

    // Helper to update Fielding Stats
    const updateFielding = (f: FieldingStats) => {
      f.catches += perf.catches;
      f.runOuts += perf.runOuts;
      f.stumpings += perf.stumpings;
    };

    // Apply to all 3 scopes
    updateBatting(container.allTime.batting);
    updateBowling(container.allTime.bowling);
    updateFielding(container.allTime.fielding);

    updateBatting(levelStats.batting);
    updateBowling(levelStats.bowling);
    updateFielding(levelStats.fielding);

    updateBatting(tourneyRecord.batting);
    updateBowling(tourneyRecord.bowling);
    updateFielding(tourneyRecord.fielding);
  }
}
