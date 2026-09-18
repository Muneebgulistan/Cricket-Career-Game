import { MILESTONES_CATALOG, UnlockedAchievementRecord } from '../../career/AchievementSystem';
import { PlayerData } from '../../player/PlayerModel';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { AudioManager } from '../../audio/AudioManager';

export class MilestonesScreen {
  public static render(unlocked: UnlockedAchievementRecord[], player: PlayerData): string {
    const unlockedMap = new Map<string, UnlockedAchievementRecord>();
    unlocked.forEach(u => unlockedMap.set(u.id, u));

    const totalCount = MILESTONES_CATALOG.length;
    const unlockedCount = unlocked.length;
    const percent = Math.round((unlockedCount / totalCount) * 100);

    const cardsHtml = MILESTONES_CATALOG.map(item => {
      const isUnlocked = unlockedMap.has(item.id);
      const record = unlockedMap.get(item.id);

      return `
        <div class="card" style="border-color: ${isUnlocked ? 'var(--accent-gold)' : 'var(--border-color)'}; background: ${isUnlocked ? 'rgba(245, 158, 11, 0.04)' : 'var(--bg-card)'}; ${!isUnlocked ? 'opacity: 0.65;' : ''}">
          <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.5rem;">
            <div style="font-size: 2rem;">${item.icon}</div>
            <span class="badge ${isUnlocked ? 'badge-gold' : 'badge-gray'}">
              ${isUnlocked ? 'UNLOCKED' : 'LOCKED'}
            </span>
          </div>

          <strong style="display: block; font-size: 1.05rem; color: #fff; margin-bottom: 0.25rem;">
            ${item.name}
          </strong>
          <p style="font-size: 0.8rem; color: var(--text-muted); margin-bottom: 0.75rem;">
            ${item.description}
          </p>

          ${isUnlocked && record ? `
            <div style="font-size: 0.75rem; color: var(--accent-emerald); border-top: 1px solid var(--border-color); pt-2; margin-top: 0.5rem; padding-top: 0.4rem;">
              ✓ Achieved on ${record.dateAchieved} (${record.careerLevel.replace('_', ' ')})
            </div>
          ` : `
            <div style="font-size: 0.75rem; color: var(--text-dim); border-top: 1px solid var(--border-color); padding-top: 0.4rem;">
              🔒 Yet to be achieved in career
            </div>
          `}
        </div>
      `;
    }).join('');

    return `
      <div class="main-viewport" style="max-width: 1000px;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
          <div>
            <div class="badge badge-gold" style="margin-bottom: 0.4rem;">TROPHY ROOM & ACHIEVEMENTS</div>
            <h2 style="font-family: var(--font-display); font-size: 3rem; letter-spacing: 0.05em; line-height: 1;">
              CAREER <span style="color: var(--accent-gold);">MILESTONES</span>
            </h2>
            <div style="font-size: 0.85rem; color: var(--text-dim); margin-top: 0.25rem;">
              ${player.firstName} ${player.lastName} • ${unlockedCount} of ${totalCount} Milestones Unlocked (${percent}%)
            </div>
          </div>

          <button id="btn-milestones-back" class="btn btn-secondary">
            ← Career Hub
          </button>
        </div>

        <!-- Overall Progress Meter -->
        <div class="card" style="margin-bottom: 2rem;">
          <div style="display: flex; justify-content: space-between; font-size: 0.85rem; margin-bottom: 0.4rem;">
            <span>Milestone Completion</span>
            <strong style="color: var(--accent-gold);">${unlockedCount} / ${totalCount} (${percent}%)</strong>
          </div>
          <div class="stat-meter">
            <div class="stat-meter-fill" style="width: ${percent}%; background: var(--accent-gold);"></div>
          </div>
        </div>

        <div class="grid-3" style="gap: 1.25rem;">
          ${cardsHtml}
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const backBtn = document.getElementById('btn-milestones-back');
    if (backBtn) {
      backBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      });
    }
  }
}
