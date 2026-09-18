import { describe, it, expect } from 'vitest';
import { CareerStatisticsTracker } from '../src/player/CareerStatistics';
import { CareerLevel } from '../src/career/CareerLevel';

describe('Step 3 — Career Statistics Integration', () => {
  it('should update all-time and tournament records accurately post-match', () => {
    const stats = CareerStatisticsTracker.createInitialContainer();

    CareerStatisticsTracker.recordMatch(
      stats,
      CareerLevel.UNDER_16,
      'u16_tourn_1',
      'Under-16 Championship',
      {
        runs: 54,
        balls: 38,
        fours: 6,
        sixes: 1,
        isOut: true,
        didBat: true,
        overs: 3,
        maidens: 0,
        runsConceded: 21,
        wickets: 2,
        didBowl: true,
        catches: 1,
        runOuts: 0,
        stumpings: 0
      }
    );

    expect(stats.allTime.batting.matches).toBe(1);
    expect(stats.allTime.batting.runs).toBe(54);
    expect(stats.allTime.batting.fifties).toBe(1);
    expect(stats.allTime.batting.highestScore).toBe(54);
    expect(stats.allTime.bowling.wickets).toBe(2);
    expect(stats.allTime.fielding.catches).toBe(1);

    // Specific tournament bucket
    const tournStats = stats.byTournament.find(t => t.tournamentId === 'u16_tourn_1');
    expect(tournStats).toBeDefined();
    expect(tournStats?.batting.runs).toBe(54);
  });
});
