import { EventBus } from '../../core/EventBus';

export class DebugPanel {
  private static isOpen: boolean = false;

  public static render(): string {
    return `
      <button id="btn-toggle-debug" class="debug-panel-btn">
        🛠️ DEV & TEST TOOLS
      </button>

      <div id="debug-modal" class="debug-modal" style="display: ${this.isOpen ? 'block' : 'none'};">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem; border-bottom: 1px solid #334155; padding-bottom: 0.5rem;">
          <strong style="color: #38bdf8;">QA & Game State Debugger</strong>
          <button id="btn-close-debug" style="background: none; border: none; color: #94a3b8; cursor: pointer; font-size: 1.1rem;">✕</button>
        </div>

        <div style="display: flex; flex-direction: column; gap: 0.65rem;">
          <!-- Live In-Match Testing Controls -->
          <div>
            <div style="font-size: 0.75rem; color: #38bdf8; margin-bottom: 0.35rem; font-weight: 700;">MATCH PLAY OVERRIDES:</div>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 0.35rem;">
              <button class="btn btn-secondary" style="padding: 0.35rem; font-size: 0.7rem;" id="debug-force-four">⚡ Force 4</button>
              <button class="btn btn-secondary" style="padding: 0.35rem; font-size: 0.7rem;" id="debug-force-six">🚀 Force 6</button>
              <button class="btn btn-danger" style="padding: 0.35rem; font-size: 0.7rem;" id="debug-force-wkt">💥 Force Wicket</button>
              <button class="btn btn-gold" style="padding: 0.35rem; font-size: 0.7rem;" id="debug-force-catch">🧤 Force Catch</button>
              <button class="btn btn-secondary" style="padding: 0.35rem; font-size: 0.7rem;" id="debug-skip-over">⏩ Skip Over</button>
              <button class="btn btn-primary" style="padding: 0.35rem; font-size: 0.7rem;" id="debug-complete-inn">🏁 End Innings</button>
            </div>
          </div>

          <div>
            <div style="font-size: 0.75rem; color: #94a3b8; margin-bottom: 0.35rem; font-weight: 600;">FORM & CONFIDENCE:</div>
            <div style="display: flex; gap: 0.4rem;">
              <button class="btn btn-secondary" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-form-up">+15 Form</button>
              <button class="btn btn-secondary" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-form-down">-15 Form</button>
              <button class="btn btn-secondary" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-conf-up">+15 Conf</button>
              <button class="btn btn-secondary" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-conf-down">-15 Conf</button>
            </div>
          </div>

          <div>
            <div style="font-size: 0.75rem; color: #94a3b8; margin-bottom: 0.35rem; font-weight: 600;">MATCH & MILESTONES:</div>
            <div style="display: flex; gap: 0.4rem;">
              <button class="btn btn-secondary" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-add-runs">+50 Runs</button>
              <button class="btn btn-secondary" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-add-wkts">+3 Wkts</button>
              <button class="btn btn-primary" style="flex: 1.2; padding: 0.35rem; font-size: 0.75rem;" id="debug-sim-match">⚡ Quick Sim</button>
            </div>
          </div>

          <div>
            <div style="font-size: 0.75rem; color: #94a3b8; margin-bottom: 0.35rem; font-weight: 600;">CAREER LEVEL & STATUS:</div>
            <div style="display: flex; gap: 0.4rem;">
              <button class="btn btn-gold" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-force-promote">⬆️ Force Promote</button>
              <button class="btn btn-danger" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-force-demote">⬇️ Force Drop</button>
            </div>
          </div>

          <div>
            <div style="font-size: 0.75rem; color: #94a3b8; margin-bottom: 0.35rem; font-weight: 600;">PERSISTENCE & SAVE:</div>
            <div style="display: flex; gap: 0.4rem;">
              <button class="btn btn-secondary" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-save">💾 Save Game</button>
              <button class="btn btn-secondary" style="flex: 1; padding: 0.35rem; font-size: 0.75rem;" id="debug-load">📂 Load Game</button>
            </div>
          </div>

          <div id="debug-log-view" style="font-family: monospace; font-size: 0.7rem; color: #38bdf8; background: #020617; padding: 0.4rem; border-radius: 4px; max-height: 80px; overflow-y: auto;">
            [Debugger Ready]
          </div>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const toggleBtn = document.getElementById('btn-toggle-debug');
    const closeBtn = document.getElementById('btn-close-debug');
    const modal = document.getElementById('debug-modal');

    const logEl = document.getElementById('debug-log-view');
    const log = (msg: string) => {
      if (logEl) {
        logEl.innerText = `> ${msg}`;
      }
      console.log(`[QA DEBUG] ${msg}`);
    };

    if (toggleBtn && modal) {
      toggleBtn.addEventListener('click', () => {
        this.isOpen = !this.isOpen;
        modal.style.display = this.isOpen ? 'block' : 'none';
      });
    }

    if (closeBtn && modal) {
      closeBtn.addEventListener('click', () => {
        this.isOpen = false;
        modal.style.display = 'none';
      });
    }

    const wire = (id: string, eventName: string, param?: any) => {
      const el = document.getElementById(id);
      if (el) {
        el.addEventListener('click', () => {
          log(`Triggering event: ${eventName}`);
          EventBus.getInstance().emit(eventName, param);
        });
      }
    };

    // Match overrides
    wire('debug-force-four', 'debug:force-four');
    wire('debug-force-six', 'debug:force-six');
    wire('debug-force-wkt', 'debug:force-wkt');
    wire('debug-force-catch', 'debug:force-catch');
    wire('debug-skip-over', 'debug:skip-over');
    wire('debug-complete-inn', 'debug:complete-inn');

    // Attributes & status
    wire('debug-form-up', 'debug:modify-form', +15);
    wire('debug-form-down', 'debug:modify-form', -15);
    wire('debug-conf-up', 'debug:modify-conf', +15);
    wire('debug-conf-down', 'debug:modify-conf', -15);

    // Runs / Wkts
    wire('debug-add-runs', 'debug:add-runs', 50);
    wire('debug-add-wkts', 'debug:add-wkts', 3);
    wire('debug-sim-match', 'match:fast-forward');

    // Progression
    wire('debug-force-promote', 'debug:force-promote');
    wire('debug-force-demote', 'debug:force-demote');

    // Persistence
    wire('debug-save', 'game:manual-save', 'qa_debug_slot');
    wire('debug-load', 'menu:load-game', 'qa_debug_slot');
  }
}
