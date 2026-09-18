import { SaveDataV1, SaveDataV2, CURRENT_SAVE_VERSION, GameSettings } from './SaveSchema';
import { PlayerData } from '../player/PlayerModel';
import { TournamentInstance } from '../tournament/TournamentModel';
import { CareerStatisticsContainer } from '../player/CareerStatistics';
import { MatchHistoryEntry } from './SaveSchema';
import { WeeklySchedule, CareerCalendar } from '../career/CareerCalendar';
import { FatigueState, FatigueSystem } from '../career/FatigueSystem';
import { ActiveInjury } from '../career/InjurySystem';
import { UnlockedAchievementRecord } from '../career/AchievementSystem';
import { DifficultyLevel } from '../career/Difficulty';

export interface SaveSlotMetadata {
  slotId: string;
  playerName: string;
  level: string;
  rating: number;
  timestamp: number;
  dateFormatted: string;
}

export class SaveSystem {
  private static readonly STORAGE_PREFIX = 'cricket_career_save_';
  private static readonly SLOTS_KEY = 'cricket_career_slots_index';
  private static readonly DEFAULT_SLOT = 'autosave';

  // In-memory fallback for test runners or environments where window.localStorage is absent
  private static memoryStore: Map<string, string> = new Map();

  private static getStorageItem(key: string): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return window.localStorage.getItem(key);
    }
    return this.memoryStore.get(key) || null;
  }

  private static setStorageItem(key: string, value: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      window.localStorage.setItem(key, value);
    } else {
      this.memoryStore.set(key, value);
    }
  }

  private static removeStorageItem(key: string): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      window.localStorage.removeItem(key);
    } else {
      this.memoryStore.delete(key);
    }
  }

  /**
   * Saves the entire game state into a specified slot.
   */
  public static saveGame(
    slotId: string = this.DEFAULT_SLOT,
    payload: {
      player: PlayerData;
      tournament: TournamentInstance | null;
      statistics: CareerStatisticsContainer;
      matchHistory?: MatchHistoryEntry[];
      recentRatings?: number[];
      achievements?: string[];
      unlockedAchievements?: UnlockedAchievementRecord[];
      calendar?: WeeklySchedule;
      fatigue?: FatigueState;
      activeInjury?: ActiveInjury | null;
      settings?: Partial<GameSettings>;
    }
  ): boolean {
    try {
      const defaultSettings: GameSettings = {
        soundEnabled: true,
        soundVolume: 0.7,
        simulationSpeed: 'NORMAL',
        commentaryEnabled: true,
        autoSave: true,
        difficulty: DifficultyLevel.NORMAL,
        ...payload.settings
      };

      const saveData: SaveDataV2 = {
        version: CURRENT_SAVE_VERSION,
        saveId: slotId,
        timestamp: Date.now(),
        saveName: `${payload.player.firstName} ${payload.player.lastName}`,
        player: payload.player,
        tournament: payload.tournament,
        statistics: payload.statistics,
        matchHistory: payload.matchHistory ?? [],
        recentRatings: payload.recentRatings ?? [],
        achievements: payload.achievements ?? [],
        unlockedAchievements: payload.unlockedAchievements ?? [],
        calendar: payload.calendar ?? CareerCalendar.createDefaultWeek(),
        fatigue: payload.fatigue ?? FatigueSystem.createInitialState(),
        activeInjury: payload.activeInjury ?? null,
        settings: defaultSettings
      };

      const serialized = JSON.stringify(saveData);
      const storageKey = this.STORAGE_PREFIX + slotId;
      this.setStorageItem(storageKey, serialized);

      // Update slot index
      this.registerSlot(slotId, {
        slotId,
        playerName: `${payload.player.firstName} ${payload.player.lastName}`,
        level: payload.player.career.currentCareerLevel,
        rating: payload.player.potential.overallRating,
        timestamp: saveData.timestamp,
        dateFormatted: new Date(saveData.timestamp).toLocaleString()
      });

      console.log(`[SaveSystem] Successfully saved slot: ${slotId}`);
      return true;
    } catch (err) {
      console.error(`[SaveSystem] Failed to save game for slot ${slotId}:`, err);
      return false;
    }
  }

  /**
   * Loads the game state from the specified slot, automatically migrating V1 saves to V2.
   */
  public static loadGame(slotId: string = this.DEFAULT_SLOT): SaveDataV2 | null {
    try {
      const storageKey = this.STORAGE_PREFIX + slotId;
      const raw = this.getStorageItem(storageKey);
      if (!raw) {
        console.warn(`[SaveSystem] No save file found for slot ${slotId}`);
        return null;
      }

      const parsed = JSON.parse(raw) as any;

      // Migration from Version 1 to Version 2
      if (parsed.version === 1) {
        console.log(`[SaveSystem] Migrating save from version 1 to ${CURRENT_SAVE_VERSION}`);
        const v1 = parsed as SaveDataV1;

        const migratedV2: SaveDataV2 = {
          version: 2,
          saveId: v1.saveId || slotId,
          timestamp: v1.timestamp || Date.now(),
          saveName: v1.saveName,
          player: v1.player,
          tournament: v1.tournament,
          statistics: v1.statistics,
          matchHistory: v1.matchHistory || [],
          recentRatings: v1.recentRatings || [],
          achievements: v1.achievements || [],
          unlockedAchievements: [],
          calendar: CareerCalendar.createDefaultWeek(),
          fatigue: FatigueSystem.createInitialState(),
          activeInjury: null,
          settings: {
            ...v1.settings,
            difficulty: DifficultyLevel.NORMAL
          }
        };

        // Persist migrated save immediately
        this.setStorageItem(storageKey, JSON.stringify(migratedV2));
        return migratedV2;
      }

      console.log(`[SaveSystem] Successfully loaded game: ${parsed.saveName} (Slot: ${slotId})`);
      return parsed as SaveDataV2;
    } catch (err) {
      console.error(`[SaveSystem] Failed to parse save file for slot ${slotId}:`, err);
      return null;
    }
  }

  /**
   * Deletes a save slot and clears it from the slot index.
   */
  public static deleteSave(slotId: string = this.DEFAULT_SLOT): boolean {
    try {
      const storageKey = this.STORAGE_PREFIX + slotId;
      this.removeStorageItem(storageKey);

      const slots = this.listSaves().filter(s => s.slotId !== slotId);
      this.setStorageItem(this.SLOTS_KEY, JSON.stringify(slots));
      console.log(`[SaveSystem] Deleted save slot: ${slotId}`);
      return true;
    } catch (err) {
      console.error(`[SaveSystem] Failed to delete save slot ${slotId}:`, err);
      return false;
    }
  }

  /**
   * Checks if a save exists for the given slot.
   */
  public static saveExists(slotId: string = this.DEFAULT_SLOT): boolean {
    const storageKey = this.STORAGE_PREFIX + slotId;
    return this.getStorageItem(storageKey) !== null;
  }

  /**
   * Lists all existing save slots with summary metadata.
   */
  public static listSaves(): SaveSlotMetadata[] {
    try {
      const raw = this.getStorageItem(this.SLOTS_KEY);
      if (!raw) return [];
      return JSON.parse(raw) as SaveSlotMetadata[];
    } catch {
      return [];
    }
  }

  private static registerSlot(slotId: string, meta: SaveSlotMetadata): void {
    const list = this.listSaves().filter(s => s.slotId !== slotId);
    list.unshift(meta);
    this.setStorageItem(this.SLOTS_KEY, JSON.stringify(list));
  }
}
