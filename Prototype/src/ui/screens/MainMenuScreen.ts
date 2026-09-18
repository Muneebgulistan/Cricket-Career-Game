import { GameStateManager, GameScreen } from '../../core/GameState';
import { SaveSystem } from '../../save/SaveSystem';
import { AudioManager } from '../../audio/AudioManager';
import { EventBus } from '../../core/EventBus';

export class MainMenuScreen {
  public static render(): string {
    const hasSave = SaveSystem.saveExists('autosave');
    const saves = SaveSystem.listSaves();
    const latestSave = saves[0];

    return `
      <div class="main-viewport" style="display: flex; flex-direction: column; justify-content: center; align-items: center; min-height: 85vh; text-align: center;">
        <div style="margin-bottom: 2.5rem;">
          <div class="badge badge-success" style="margin-bottom: 1rem; font-size: 0.85rem; padding: 0.4rem 1rem;">
            🏏 SINGLE-PLAYER OFFLINE CRICKET CAREER
          </div>
          <h1 style="font-family: var(--font-display); font-size: 4.75rem; line-height: 0.95; letter-spacing: 0.05em; font-weight: 700;">
            ROAD TO <span style="color: var(--accent-emerald);">GLORY</span>
          </h1>
          <p style="color: var(--text-muted); font-size: 1.15rem; max-width: 600px; margin: 1rem auto 0 auto;">
            From Under-16 grassroots trials to lifting the World Cup. Your skill, timing, and composure determine your destiny.
          </p>
        </div>

        <div style="display: flex; flex-direction: column; gap: 1rem; width: 100%; max-width: 380px;">
          ${hasSave && latestSave ? `
            <button id="menu-btn-continue" class="btn btn-gold" style="padding: 1rem; font-size: 1.1rem;">
              ▶ CONTINUE CAREER
              <span style="font-size: 0.8rem; opacity: 0.85; display: block; font-weight: 500;">
                ${latestSave.playerName} (OVR ${latestSave.rating})
              </span>
            </button>
          ` : ''}

          <button id="menu-btn-new-career" class="btn btn-primary" style="padding: 1rem; font-size: 1.1rem;">
            ⭐ START NEW CAREER (UNDER-16)
          </button>

          <button id="menu-btn-settings" class="btn btn-secondary" style="padding: 0.85rem;">
            ⚙️ SETTINGS & SAVE SLOTS
          </button>
        </div>

        <div style="margin-top: 3.5rem; display: flex; gap: 2rem; color: var(--text-dim); font-size: 0.8rem;">
          <div>✓ Offline-First Architecture</div>
          <div>✓ Realistic Match Simulation</div>
          <div>✓ Performance-Driven Selection</div>
          <div>✓ 9 Career Tiers</div>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const btnNew = document.getElementById('menu-btn-new-career');
    if (btnNew) {
      btnNew.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.PLAYER_CREATION);
      });
    }

    const btnContinue = document.getElementById('menu-btn-continue');
    if (btnContinue) {
      btnContinue.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('menu:load-game', 'autosave');
      });
    }

    const btnSettings = document.getElementById('menu-btn-settings');
    if (btnSettings) {
      btnSettings.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.SETTINGS);
      });
    }
  }
}
