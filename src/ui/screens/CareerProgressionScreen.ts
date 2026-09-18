import { PlayerData } from '../../player/PlayerModel';
import { CareerStatisticsContainer } from '../../player/CareerStatistics';
import { CAREER_LEVEL_ORDER, CAREER_LEVEL_CONFIG } from '../../career/CareerLevel';
import { CareerProgression } from '../../career/CareerProgression';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { EventBus } from '../../core/EventBus';
import { AudioManager } from '../../audio/AudioManager';

export class CareerProgressionScreen {
  public static render(player: PlayerData, stats: CareerStatisticsContainer): string {
    const currentLevel = player.career.currentCareerLevel;
    const currentIndex = CAREER_LEVEL_ORDER.indexOf(currentLevel);
    const levelStats = stats.byLevel[currentLevel] || {
      batting: stats.allTime.batting,
      bowling: stats.allTime.bowling,
      fielding: stats.allTime.fielding
    };

    const promoCheck = CareerProgression.checkPromotion(
      player,
      levelStats.batting.matches,
      levelStats.batting.runs,
      levelStats.bowling.wickets
    );

    const tiersHtml = CAREER_LEVEL_ORDER.map((level, idx) => {
      const meta = CAREER_LEVEL_CONFIG[level];
      const isCurrent = level === currentLevel;
      const isPast = idx < currentIndex;
      const isFuture = idx > currentIndex;

      let borderStyle = 'var(--border-color)';
      let badgeHtml = '<span class="badge badge-gray">LOCKED</span>';

      if (isCurrent) {
        borderStyle = 'var(--accent-gold)';
        badgeHtml = '<span class="badge badge-gold">ACTIVE TIER</span>';
      } else if (isPast) {
        borderStyle = 'var(--accent-emerald)';
        badgeHtml = '<span class="badge badge-success">COMPLETED ✓</span>';
      }

      return `
        <div class="card" style="border-color: ${borderStyle}; margin-bottom: 1.25rem; ${isFuture ? 'opacity: 0.75;' : ''}">
          <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.75rem;">
            <div>
              <div style="font-size: 0.75rem; color: var(--text-dim); text-transform: uppercase; font-weight: 700;">
                TIER ${meta.tier} • ${meta.defaultFormat}
              </div>
              <h3 style="font-size: 1.25rem; font-weight: 700; color: #fff; margin-top: 0.2rem;">
                ${meta.displayName}
              </h3>
            </div>
            ${badgeHtml}
          </div>

          <p style="font-size: 0.85rem; color: var(--text-muted); margin-bottom: 1rem;">
            ${meta.description}
          </p>

          <div style="background: var(--bg-main); border-radius: 8px; padding: 0.85rem; border: 1px solid var(--border-color); font-size: 0.8rem;">
            <div style="font-weight: 700; color: var(--text-dim); margin-bottom: 0.4rem;">
              PROMOTION REQUIREMENTS TO ADVANCE:
            </div>
            <div class="grid-2" style="gap: 0.5rem; color: var(--text-main);">
              <div>• Minimum Matches: <strong>${meta.promotionCriteria.minMatches}</strong></div>
              <div>• Minimum Overall Rating: <strong>${meta.promotionCriteria.minOverallRating}</strong></div>
              <div>• Minimum Form: <strong>${meta.promotionCriteria.minForm}</strong></div>
              ${meta.promotionCriteria.minRuns ? `<div>• Minimum Runs: <strong>${meta.promotionCriteria.minRuns}</strong></div>` : ''}
              ${meta.promotionCriteria.minWickets ? `<div>• Minimum Wickets: <strong>${meta.promotionCriteria.minWickets}</strong></div>` : ''}
            </div>
          </div>
        </div>
      `;
    }).join('');

    return `
      <div class="main-viewport" style="max-width: 950px;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem;">
          <div>
            <div class="badge badge-gold" style="margin-bottom: 0.4rem;">OFFICIAL CAREER LADDER</div>
            <h2 style="font-family: var(--font-display); font-size: 3rem; letter-spacing: 0.05em; line-height: 1;">
              CAREER <span style="color: var(--accent-emerald);">PROGRESSION</span>
            </h2>
            <p style="font-size: 0.9rem; color: var(--text-muted); margin-top: 0.35rem;">
              Perform consistently to earn scouting reports, national call-ups, and promotion to higher tiers.
            </p>
          </div>

          <button id="btn-prog-back" class="btn btn-secondary">
            ← Career Hub
          </button>
        </div>

        <!-- Current Tier Eligibility Evaluation -->
        <div class="card" style="border-color: ${promoCheck.canPromote ? 'var(--accent-gold)' : 'var(--border-color)'}; margin-bottom: 2rem; background: linear-gradient(135deg, #182236 0%, #101624 100%);">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem;">
            <h3 style="font-size: 1.15rem; font-weight: 700; color: #fff;">
              NEXT TIER STATUS: ${promoCheck.nextLevel ? promoCheck.nextLevel.replace('_', ' ') : 'PEAK'}
            </h3>
            <span class="badge ${promoCheck.canPromote ? 'badge-gold' : 'badge-warning'}">
              ${promoCheck.canPromote ? 'QUALIFIED FOR PROMOTION' : 'IN PROGRESS'}
            </span>
          </div>

          <div class="grid-2" style="gap: 1rem;">
            <div>
              <div style="font-size: 0.8rem; font-weight: 700; color: var(--accent-emerald); margin-bottom: 0.4rem;">
                CRITERIA ACHIEVED:
              </div>
              <ul style="padding-left: 1.25rem; font-size: 0.85rem; color: var(--text-muted);">
                ${promoCheck.achievedRequirements.map(a => `<li>${a}</li>`).join('') || '<li>No milestones reached yet.</li>'}
              </ul>
            </div>

            <div>
              <div style="font-size: 0.8rem; font-weight: 700; color: ${promoCheck.canPromote ? 'var(--accent-emerald)' : '#f87171'}; margin-bottom: 0.4rem;">
                PENDING CRITERIA:
              </div>
              <ul style="padding-left: 1.25rem; font-size: 0.85rem; color: var(--text-muted);">
                ${promoCheck.missingRequirements.length > 0
                  ? promoCheck.missingRequirements.map(m => `<li>${m}</li>`).join('')
                  : '<li>All requirements completed! You are eligible for promotion.</li>'}
              </ul>
            </div>
          </div>

          ${promoCheck.canPromote ? `
            <div style="margin-top: 1.5rem; text-align: center;">
              <button id="btn-accept-promotion" class="btn btn-gold" style="padding: 0.9rem 2.5rem; font-size: 1.1rem;">
                ⭐ ACCEPT PROMOTION TO ${promoCheck.nextLevel}
              </button>
            </div>
          ` : ''}
        </div>

        <!-- All Tiers List -->
        <div>
          ${tiersHtml}
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const backBtn = document.getElementById('btn-prog-back');
    if (backBtn) {
      backBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      });
    }

    const acceptBtn = document.getElementById('btn-accept-promotion');
    if (acceptBtn) {
      acceptBtn.addEventListener('click', () => {
        AudioManager.getInstance().playFanfare();
        EventBus.getInstance().emit('career:claim-promotion');
      });
    }
  }
}
