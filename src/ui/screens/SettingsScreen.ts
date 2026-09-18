import { SaveSystem } from '../../save/SaveSystem';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { EventBus } from '../../core/EventBus';
import { AudioManager } from '../../audio/AudioManager';

export class SettingsScreen {
  public static render(): string {
    const slots = SaveSystem.listSaves();

    const slotsHtml = slots.map(slot => `
      <div style="display: flex; justify-content: space-between; align-items: center; padding: 1rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; margin-bottom: 0.75rem;">
        <div>
          <div style="font-weight: 700; font-size: 1rem; color: #fff;">${slot.playerName}</div>
          <div style="font-size: 0.8rem; color: var(--text-dim); margin-top: 0.2rem;">
            Slot: <span style="color: var(--accent-cyan);">${slot.slotId}</span> • Tier: ${slot.level} • OVR: ${slot.rating}
          </div>
          <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.2rem;">
            Saved: ${slot.dateFormatted}
          </div>
        </div>

        <div style="display: flex; gap: 0.5rem;">
          <button class="btn btn-secondary btn-load-slot" data-slot="${slot.slotId}" style="padding: 0.4rem 0.85rem; font-size: 0.8rem;">
            Load
          </button>
          <button class="btn btn-danger btn-delete-slot" data-slot="${slot.slotId}" style="padding: 0.4rem 0.85rem; font-size: 0.8rem;">
            Delete
          </button>
        </div>
      </div>
    `).join('');

    return `
      <div class="main-viewport" style="max-width: 800px;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem;">
          <div>
            <div class="badge badge-primary" style="margin-bottom: 0.4rem;">PREFERENCES & STORAGE</div>
            <h2 style="font-family: var(--font-display); font-size: 3rem; letter-spacing: 0.05em; line-height: 1;">
              GAME <span style="color: var(--accent-emerald);">SETTINGS</span>
            </h2>
          </div>

          <button id="btn-settings-back" class="btn btn-secondary">
            ← Career Hub
          </button>
        </div>

        <div class="card" style="margin-bottom: 2rem;">
          <h3 style="font-size: 1rem; font-weight: 700; color: #fff; margin-bottom: 1.25rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
            OFFLINE SAVE SLOTS
          </h3>

          <div style="margin-bottom: 1.5rem;">
            ${slotsHtml || '<div style="color: var(--text-dim); font-size: 0.9rem; padding: 1rem; text-align: center;">No saved careers found.</div>'}
          </div>

          <div style="border-top: 1px solid var(--border-color); padding-top: 1.25rem; display: flex; gap: 0.75rem;">
            <button id="btn-save-slot-1" class="btn btn-secondary" style="flex: 1; font-size: 0.85rem;">
              💾 Save to Slot 1
            </button>
            <button id="btn-save-slot-2" class="btn btn-secondary" style="flex: 1; font-size: 0.85rem;">
              💾 Save to Slot 2
            </button>
            <button id="btn-save-slot-3" class="btn btn-secondary" style="flex: 1; font-size: 0.85rem;">
              💾 Save to Slot 3
            </button>
          </div>
        </div>

        <div class="card">
          <h3 style="font-size: 1rem; font-weight: 700; color: #fff; margin-bottom: 1.25rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
            AUDIO & GAMEPLAY PREFERENCES
          </h3>

          <div style="display: flex; flex-direction: column; gap: 1rem; font-size: 0.9rem;">
            <label style="display: flex; justify-content: space-between; align-items: center;">
              <span>Audio Sound Effects (Web Audio API Synthesizer)</span>
              <input type="checkbox" id="chk-sound" checked style="transform: scale(1.3);" />
            </label>

            <label style="display: flex; justify-content: space-between; align-items: center;">
              <span>Ball-by-ball Commentary Feed</span>
              <input type="checkbox" id="chk-commentary" checked style="transform: scale(1.3);" />
            </label>

            <label style="display: flex; justify-content: space-between; align-items: center;">
              <span>Auto-Save after every match</span>
              <input type="checkbox" id="chk-autosave" checked style="transform: scale(1.3);" />
            </label>

            <div style="display: flex; justify-content: space-between; align-items: center; border-top: 1px solid var(--border-color); padding-top: 0.75rem;">
              <div>
                <span style="font-weight: 600;">Gameplay Difficulty</span>
                <span style="display: block; font-size: 0.75rem; color: var(--text-dim);">Affects batting timing sweet-spot, AI discipline, and fielding reaction windows.</span>
              </div>
              <select id="select-difficulty" style="padding: 0.4rem 0.8rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 6px; color: #fff; font-size: 0.85rem;">
                <option value="EASY">EASY (Generous timing)</option>
                <option value="NORMAL" selected>NORMAL (Balanced)</option>
                <option value="HARD">HARD (True Test Pro)</option>
              </select>
            </div>
          </div>

          <div style="margin-top: 2rem; border-top: 1px solid var(--border-color); padding-top: 1.25rem; display: flex; justify-content: space-between;">
            <button id="btn-return-main-menu" class="btn btn-danger" style="font-size: 0.85rem;">
              Exit to Main Menu
            </button>
          </div>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const backBtn = document.getElementById('btn-settings-back');
    if (backBtn) {
      backBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      });
    }

    const mainMenuBtn = document.getElementById('btn-return-main-menu');
    if (mainMenuBtn) {
      mainMenuBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.MAIN_MENU);
      });
    }

    const soundChk = document.getElementById('chk-sound') as HTMLInputElement;
    if (soundChk) {
      soundChk.addEventListener('change', () => {
        AudioManager.getInstance().setMuted(!soundChk.checked);
      });
    }

    const saveSlot = (slotName: string) => {
      AudioManager.getInstance().playClick();
      EventBus.getInstance().emit('game:manual-save', slotName);
      GameStateManager.getInstance().transitionTo(GameScreen.SETTINGS);
    };

    document.getElementById('btn-save-slot-1')?.addEventListener('click', () => saveSlot('slot_1'));
    document.getElementById('btn-save-slot-2')?.addEventListener('click', () => saveSlot('slot_2'));
    document.getElementById('btn-save-slot-3')?.addEventListener('click', () => saveSlot('slot_3'));

    // Wire Load Slot buttons
    document.querySelectorAll('.btn-load-slot').forEach(btn => {
      btn.addEventListener('click', () => {
        const slot = btn.getAttribute('data-slot');
        if (slot) {
          AudioManager.getInstance().playClick();
          EventBus.getInstance().emit('menu:load-game', slot);
        }
      });
    });

    // Wire Delete Slot buttons
    document.querySelectorAll('.btn-delete-slot').forEach(btn => {
      btn.addEventListener('click', () => {
        const slot = btn.getAttribute('data-slot');
        if (slot && confirm(`Are you sure you want to delete save slot "${slot}"?`)) {
          AudioManager.getInstance().playClick();
          SaveSystem.deleteSave(slot);
          GameStateManager.getInstance().transitionTo(GameScreen.SETTINGS);
        }
      });
    });
  }
}
