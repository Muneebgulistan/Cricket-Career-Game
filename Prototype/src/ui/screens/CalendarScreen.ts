import { WeeklySchedule, CalendarActivityType, CalendarDay } from '../../career/CareerCalendar';
import { FatigueState } from '../../career/FatigueSystem';
import { ActiveInjury } from '../../career/InjurySystem';
import { PlayerData } from '../../player/PlayerModel';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { EventBus } from '../../core/EventBus';
import { AudioManager } from '../../audio/AudioManager';

export class CalendarScreen {
  public static render(
    schedule: WeeklySchedule,
    fatigue: FatigueState,
    injury: ActiveInjury | null,
    player: PlayerData
  ): string {
    const currentDay = schedule.slots[schedule.currentDayIndex]?.day || CalendarDay.MONDAY;

    const daysHtml = schedule.slots.map((slot, idx) => {
      const isCurrent = idx === schedule.currentDayIndex;
      const isPast = idx < schedule.currentDayIndex;

      let border = 'var(--border-color)';
      let badge = '<span class="badge badge-gray">Upcoming</span>';

      if (isCurrent) {
        border = 'var(--accent-gold)';
        badge = '<span class="badge badge-gold">TODAY</span>';
      } else if (isPast) {
        border = 'var(--accent-emerald)';
        badge = '<span class="badge badge-success">Done ✓</span>';
      }

      return `
        <div class="card" style="border-color: ${border}; background: ${isCurrent ? 'rgba(245, 158, 11, 0.05)' : 'var(--bg-card)'};">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem;">
            <strong style="font-size: 1rem; color: #fff;">${slot.day}</strong>
            ${badge}
          </div>
          <div style="font-weight: 700; font-size: 0.95rem; color: var(--accent-cyan); margin-bottom: 0.25rem;">
            ${slot.activity}
          </div>
          <div style="font-size: 0.8rem; color: var(--text-muted);">
            ${slot.details || 'Standard routine'}
          </div>
        </div>
      `;
    }).join('');

    return `
      <div class="main-viewport" style="max-width: 1000px;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
          <div>
            <div class="badge badge-primary" style="margin-bottom: 0.4rem;">WEEKLY SCHEDULE</div>
            <h2 style="font-family: var(--font-display); font-size: 3rem; letter-spacing: 0.05em; line-height: 1;">
              CAREER <span style="color: var(--accent-emerald);">CALENDAR</span>
            </h2>
            <div style="font-size: 0.85rem; color: var(--text-dim); margin-top: 0.25rem;">
              Season ${schedule.seasonYear} • Week ${schedule.weekNumber} • Today: <strong style="color: var(--accent-gold);">${currentDay}</strong>
            </div>
          </div>

          <button id="btn-cal-back" class="btn btn-secondary">
            ← Career Hub
          </button>
        </div>

        <!-- Workload & Injury Status Bar -->
        <div class="grid-2" style="margin-bottom: 1.5rem;">
          <div class="card">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem;">
              <span style="font-size: 0.85rem; color: var(--text-dim); font-weight: 600;">FATIGUE WORKLOAD</span>
              <strong style="color: ${fatigue.fatigueLevel > 65 ? '#ef4444' : 'var(--accent-emerald)'}; font-size: 1.1rem;">
                ${fatigue.fatigueLevel} / 100
              </strong>
            </div>
            <div class="stat-meter">
              <div class="stat-meter-fill" style="width: ${fatigue.fatigueLevel}%; background: ${fatigue.fatigueLevel > 65 ? '#ef4444' : (fatigue.fatigueLevel > 40 ? 'var(--accent-gold)' : 'var(--accent-emerald)')};"></div>
            </div>
            <div style="font-size: 0.75rem; color: var(--text-dim); margin-top: 0.5rem;">
              Bowling Load: ${fatigue.matchOversBowled} ov • Weekly Strain: ${fatigue.weeklyTrainingLoad} pts
            </div>
          </div>

          <div class="card" style="border-color: ${injury ? 'var(--accent-crimson)' : 'var(--border-color)'};">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem;">
              <span style="font-size: 0.85rem; color: var(--text-dim); font-weight: 600;">MEDICAL & INJURY STATUS</span>
              <span class="badge ${injury ? 'badge-danger' : 'badge-success'}">
                ${injury ? injury.severity : 'MATCH FIT'}
              </span>
            </div>
            ${injury ? `
              <div style="font-weight: 700; color: var(--accent-crimson); font-size: 0.95rem;">${injury.name}</div>
              <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 0.2rem;">
                Recovery duration: ${injury.daysRemaining} days remaining (${injury.bodyPart})
              </div>
            ` : `
              <div style="color: var(--accent-emerald); font-weight: 600; font-size: 0.95rem;">No active injuries</div>
              <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 0.2rem;">
                Player is cleared for full training and competitive matches.
              </div>
            `}
          </div>
        </div>

        <!-- Weekly Activity Slots Grid -->
        <div class="grid-4" style="gap: 1rem; margin-bottom: 2rem;">
          ${daysHtml}
        </div>

        <!-- Daily Action Panel -->
        <div class="card" style="display: flex; justify-content: space-between; align-items: center; background: var(--bg-card-alt);">
          <div>
            <h3 style="font-size: 1.05rem; font-weight: 700; color: #fff;">
              Action for ${currentDay}: <span style="color: var(--accent-gold);">${schedule.slots[schedule.currentDayIndex]?.activity}</span>
            </h3>
            <div style="font-size: 0.85rem; color: var(--text-muted); margin-top: 0.25rem;">
              Complete the day's routine to develop skills, recover stamina, or advance toward the weekend match.
            </div>
          </div>

          <div style="display: flex; gap: 0.75rem;">
            <button id="btn-advance-calendar-day" class="btn btn-primary" style="padding: 0.85rem 1.75rem; font-size: 1rem;">
              ▶ Complete & Advance Day
            </button>
          </div>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const backBtn = document.getElementById('btn-cal-back');
    if (backBtn) {
      backBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      });
    }

    const advanceBtn = document.getElementById('btn-advance-calendar-day');
    if (advanceBtn) {
      advanceBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('calendar:advance-day');
      });
    }
  }
}
