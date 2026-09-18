import { CareerLevel } from './CareerLevel';

export interface AchievementDefinition {
  id: string;
  name: string;
  description: string;
  icon: string;
  category: 'MATCH' | 'MILESTONE' | 'CAREER' | 'INTERNATIONAL';
}

export interface UnlockedAchievementRecord {
  id: string;
  name: string;
  description: string;
  dateAchieved: string;
  careerLevel: CareerLevel;
  tournamentName?: string;
}

export const MILESTONES_CATALOG: AchievementDefinition[] = [
  { id: 'first_match', name: 'First Match', description: 'Make your competitive cricket debut.', icon: '🏏', category: 'MATCH' },
  { id: 'first_run', name: 'Off the Mark', description: 'Score your first career run.', icon: '🏃', category: 'MATCH' },
  { id: 'first_boundary', name: 'To the Fence', description: 'Hit your first career boundary (4 or 6).', icon: '⚡', category: 'MATCH' },
  { id: 'first_50', name: 'Maiden Half-Century', description: 'Score your first 50 runs in an innings.', icon: '🔥', category: 'MILESTONE' },
  { id: 'first_100', name: 'Three Figures', description: 'Score a glorious century (100+ runs).', icon: '💯', category: 'MILESTONE' },
  { id: 'first_catch', name: 'Safe Hands', description: 'Take your first catch in the field.', icon: '🧤', category: 'MATCH' },
  { id: 'first_5_wickets', name: 'Fifer on the Board', description: 'Take 5 wickets in a single innings.', icon: '🎯', category: 'MILESTONE' },
  { id: 'first_motm', name: 'Match Winner', description: 'Awarded Player of the Match honors.', icon: '⭐', category: 'MATCH' },
  { id: 'first_promotion', name: 'Climbing the Ladder', description: 'Earn your first career tier promotion.', icon: '⬆️', category: 'CAREER' },
  { id: 'first_tournament_win', name: 'Champion of the Arena', description: 'Lift your first competitive tournament trophy.', icon: '🏆', category: 'CAREER' },
  { id: 'international_debut', name: 'National Cap', description: 'Make your senior International Debut.', icon: '🌍', category: 'INTERNATIONAL' },
  { id: 'first_intl_50', name: 'World Stage Fifty', description: 'Score your first half-century in international colors.', icon: '🎖️', category: 'INTERNATIONAL' },
  { id: 'first_intl_100', name: 'Masterclass on the Big Stage', description: 'Score your first international century.', icon: '🌟', category: 'INTERNATIONAL' },
  { id: 'first_intl_5_wickets', name: 'Five on the World Stage', description: 'Claim 5 wickets in an international match.', icon: '💥', category: 'INTERNATIONAL' },
  { id: 'first_world_cup_match', name: 'The Global Spotlight', description: 'Play your first ICC World Cup match.', icon: '🌐', category: 'INTERNATIONAL' },
  { id: 'world_cup_winner', name: 'World Champion', description: 'Lift the ICC World Cup trophy into the skies!', icon: '👑', category: 'INTERNATIONAL' }
];

export class AchievementSystem {
  /**
   * Checks if an achievement is already unlocked.
   */
  public static isUnlocked(unlockedList: UnlockedAchievementRecord[], id: string): boolean {
    return unlockedList.some(a => a.id === id);
  }

  /**
   * Unlocks an achievement if not already obtained and returns the newly unlocked record.
   */
  public static tryUnlock(
    unlockedList: UnlockedAchievementRecord[],
    id: string,
    currentLevel: CareerLevel,
    tournamentName?: string
  ): UnlockedAchievementRecord | null {
    if (this.isUnlocked(unlockedList, id)) return null;

    const def = MILESTONES_CATALOG.find(m => m.id === id);
    if (!def) return null;

    const record: UnlockedAchievementRecord = {
      id: def.id,
      name: def.name,
      description: def.description,
      dateAchieved: new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
      careerLevel: currentLevel,
      tournamentName
    };

    unlockedList.push(record);
    return record;
  }
}
