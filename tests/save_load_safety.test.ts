import { describe, it, expect, beforeEach } from 'vitest';
import { SaveSystem } from '../src/save/SaveSystem';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';
import { CareerLevel } from '../src/career/CareerLevel';
import { TournamentManager } from '../src/tournament/TournamentManager';
import { CareerStatisticsTracker } from '../src/player/CareerStatistics';

describe('Step 3 — Save / Load Mid-Career Safety & Version Migration', () => {
  const slotId = 'step3_safe_save_slot';

  beforeEach(() => {
    SaveSystem.deleteSave(slotId);
  });

  it('should cleanly serialize and deserialize a player career with full state', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Haris',
      lastName: 'Rauf',
      role: PlayerRole.BOWLER,
      battingStyle: BattingStyle.RIGHT_HAND,
      nationality: 'Pakistan',
    });
    const tournament = TournamentManager.createTournamentForLevel(CareerLevel.UNDER_16, 'Pakistan');
    const stats = CareerStatisticsTracker.createInitialContainer();

    const saved = SaveSystem.saveGame(slotId, {
      player,
      tournament,
      statistics: stats,
      matchHistory: [],
      recentRatings: [7.5, 8.2]
    });
    expect(saved).toBe(true);

    const loaded = SaveSystem.loadGame(slotId);
    expect(loaded).not.toBeNull();
    expect(loaded?.player.firstName).toBe('Haris');
    expect(loaded?.player.lastName).toBe('Rauf');
    expect(loaded?.recentRatings).toEqual([7.5, 8.2]);

    SaveSystem.deleteSave(slotId);
  });
});
