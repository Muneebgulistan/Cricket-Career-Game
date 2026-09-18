import { describe, it, expect, beforeEach } from 'vitest';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';
import { CareerLevel } from '../src/career/CareerLevel';
import { TournamentManager } from '../src/tournament/TournamentManager';
import { CareerStatisticsTracker } from '../src/player/CareerStatistics';
import { SaveSystem } from '../src/save/SaveSystem';

describe('Save / Load Subsystem', () => {
  const testSlot = 'test_save_slot_1';

  beforeEach(() => {
    SaveSystem.deleteSave(testSlot);
  });

  it('should save, load, and preserve complete player career state', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Babar',
      lastName: 'Azam',
      nationality: 'Pakistan',
      role: PlayerRole.BATSMAN,
      battingStyle: BattingStyle.RIGHT_HAND,
      jerseyNumber: 56
    });

    const tournament = TournamentManager.createTournamentForLevel(CareerLevel.UNDER_16, 'Pakistan');
    const statistics = CareerStatisticsTracker.createInitialContainer();

    statistics.allTime.batting.runs = 245;
    statistics.allTime.batting.highestScore = 104;
    statistics.allTime.batting.hundreds = 1;

    const saved = SaveSystem.saveGame(testSlot, {
      player,
      tournament,
      statistics,
      recentRatings: [8.5, 7.8, 9.1],
      achievements: ['FIRST_CENTURY']
    });

    expect(saved).toBe(true);
    expect(SaveSystem.saveExists(testSlot)).toBe(true);

    const loaded = SaveSystem.loadGame(testSlot);
    expect(loaded).not.toBeNull();
    expect(loaded?.player.firstName).toBe('Babar');
    expect(loaded?.player.lastName).toBe('Azam');
    expect(loaded?.player.jerseyNumber).toBe(56);
    expect(loaded?.player.career.currentCareerLevel).toBe(CareerLevel.UNDER_16);
    expect(loaded?.tournament?.name).toBe(tournament.name);
    expect(loaded?.statistics.allTime.batting.runs).toBe(245);
    expect(loaded?.statistics.allTime.batting.hundreds).toBe(1);
    expect(loaded?.recentRatings).toEqual([8.5, 7.8, 9.1]);
    expect(loaded?.achievements).toContain('FIRST_CENTURY');
  });

  it('should delete a save slot properly', () => {
    const player = PlayerFactory.createUnder16Player({
      firstName: 'Temp',
      lastName: 'Player',
      nationality: 'India',
      role: PlayerRole.BOWLER,
      battingStyle: BattingStyle.RIGHT_HAND
    });

    SaveSystem.saveGame(testSlot, {
      player,
      tournament: null,
      statistics: CareerStatisticsTracker.createInitialContainer()
    });

    expect(SaveSystem.saveExists(testSlot)).toBe(true);

    const deleted = SaveSystem.deleteSave(testSlot);
    expect(deleted).toBe(true);
    expect(SaveSystem.saveExists(testSlot)).toBe(false);
  });
});
