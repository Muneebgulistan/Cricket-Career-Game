import { PlayerRole, BattingStyle, BowlingStyle } from '../../player/PlayerModel';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { EventBus } from '../../core/EventBus';
import { AudioManager } from '../../audio/AudioManager';

export class PlayerCreationScreen {
  public static render(): string {
    return `
      <div class="main-viewport" style="max-width: 760px;">
        <div style="margin-bottom: 2rem; text-align: center;">
          <div class="badge badge-gold" style="margin-bottom: 0.5rem;">YOUTH ACADEMY ENROLLMENT</div>
          <h2 style="font-family: var(--font-display); font-size: 3rem; letter-spacing: 0.05em;">
            CREATE YOUR <span style="color: var(--accent-emerald);">CRICKETER</span>
          </h2>
          <p style="color: var(--text-muted); font-size: 0.95rem;">
            Step onto the field at age 15. Your journey begins at the regional Under-16 Youth Cup.
          </p>
        </div>

        <form id="player-creation-form" class="card" style="display: flex; flex-direction: column; gap: 1.5rem;">
          <div class="grid-2">
            <div>
              <label style="display: block; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.5rem; font-weight: 600;">FIRST NAME</label>
              <input type="text" id="input-first-name" required value="Zayn" class="form-input" style="width: 100%; padding: 0.75rem 1rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; color: #fff; font-size: 1rem;" />
            </div>
            <div>
              <label style="display: block; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.5rem; font-weight: 600;">LAST NAME</label>
              <input type="text" id="input-last-name" required value="Khan" class="form-input" style="width: 100%; padding: 0.75rem 1rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; color: #fff; font-size: 1rem;" />
            </div>
          </div>

          <div class="grid-2">
            <div>
              <label style="display: block; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.5rem; font-weight: 600;">NATIONALITY</label>
              <select id="select-nationality" style="width: 100%; padding: 0.75rem 1rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; color: #fff; font-size: 1rem;">
                <option value="India" selected>India</option>
                <option value="Australia">Australia</option>
                <option value="England">England</option>
                <option value="Pakistan">Pakistan</option>
                <option value="South Africa">South Africa</option>
                <option value="New Zealand">New Zealand</option>
                <option value="West Indies">West Indies</option>
                <option value="Sri Lanka">Sri Lanka</option>
              </select>
            </div>
            <div>
              <label style="display: block; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.5rem; font-weight: 600;">JERSEY NUMBER</label>
              <input type="number" id="input-jersey" min="1" max="99" value="18" style="width: 100%; padding: 0.75rem 1rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; color: #fff; font-size: 1rem;" />
            </div>
          </div>

          <div>
            <label style="display: block; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.5rem; font-weight: 600;">CRICKET ROLE</label>
            <div class="grid-4" id="role-selector">
              <label class="card" style="padding: 1rem; text-align: center; cursor: pointer; border-color: var(--accent-emerald);">
                <input type="radio" name="role" value="${PlayerRole.BATSMAN}" checked style="display: none;" />
                <div style="font-size: 1.5rem; margin-bottom: 0.25rem;">🏏</div>
                <strong style="display: block; font-size: 0.9rem;">Batsman</strong>
                <span style="font-size: 0.75rem; color: var(--text-dim);">Top-order anchor</span>
              </label>
              <label class="card" style="padding: 1rem; text-align: center; cursor: pointer;">
                <input type="radio" name="role" value="${PlayerRole.BOWLER}" style="display: none;" />
                <div style="font-size: 1.5rem; margin-bottom: 0.25rem;">⚡</div>
                <strong style="display: block; font-size: 0.9rem;">Bowler</strong>
                <span style="font-size: 0.75rem; color: var(--text-dim);">Wicket-taking weapon</span>
              </label>
              <label class="card" style="padding: 1rem; text-align: center; cursor: pointer;">
                <input type="radio" name="role" value="${PlayerRole.ALL_ROUNDER}" style="display: none;" />
                <div style="font-size: 1.5rem; margin-bottom: 0.25rem;">🔥</div>
                <strong style="display: block; font-size: 0.9rem;">All-Rounder</strong>
                <span style="font-size: 0.75rem; color: var(--text-dim);">Game changer</span>
              </label>
              <label class="card" style="padding: 1rem; text-align: center; cursor: pointer;">
                <input type="radio" name="role" value="${PlayerRole.WICKETKEEPER}" style="display: none;" />
                <div style="font-size: 1.5rem; margin-bottom: 0.25rem;">🧤</div>
                <strong style="display: block; font-size: 0.9rem;">Keeper-Bat</strong>
                <span style="font-size: 0.75rem; color: var(--text-dim);">Sharp behind stumps</span>
              </label>
            </div>
          </div>

          <div class="grid-2">
            <div>
              <label style="display: block; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.5rem; font-weight: 600;">BATTING STANCE</label>
              <select id="select-bat-style" style="width: 100%; padding: 0.75rem 1rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; color: #fff; font-size: 1rem;">
                <option value="${BattingStyle.RIGHT_HAND}" selected>${BattingStyle.RIGHT_HAND}</option>
                <option value="${BattingStyle.LEFT_HAND}">${BattingStyle.LEFT_HAND}</option>
              </select>
            </div>
            <div>
              <label style="display: block; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.5rem; font-weight: 600;">BOWLING STYLE</label>
              <select id="select-bowl-style" style="width: 100%; padding: 0.75rem 1rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; color: #fff; font-size: 1rem;">
                <option value="${BowlingStyle.RIGHT_ARM_FAST}" selected>${BowlingStyle.RIGHT_ARM_FAST}</option>
                <option value="${BowlingStyle.LEFT_ARM_FAST}">${BowlingStyle.LEFT_ARM_FAST}</option>
                <option value="${BowlingStyle.RIGHT_ARM_MEDIUM}">${BowlingStyle.RIGHT_ARM_MEDIUM}</option>
                <option value="${BowlingStyle.RIGHT_ARM_OFF_SPIN}">${BowlingStyle.RIGHT_ARM_OFF_SPIN}</option>
                <option value="${BowlingStyle.RIGHT_ARM_LEG_SPIN}">${BowlingStyle.RIGHT_ARM_LEG_SPIN}</option>
                <option value="${BowlingStyle.LEFT_ARM_ORTHODOX}">${BowlingStyle.LEFT_ARM_ORTHODOX}</option>
              </select>
            </div>
          </div>

          <div style="display: flex; justify-content: space-between; align-items: center; margin-top: 1rem;">
            <button type="button" id="btn-cancel-creation" class="btn btn-secondary">
              ← Back to Main Menu
            </button>
            <button type="submit" class="btn btn-primary" style="padding: 0.85rem 2rem; font-size: 1.05rem;">
              CONFIRM & BEGIN CAREER
            </button>
          </div>
        </form>
      </div>
    `;
  }

  public static bindEvents(): void {
    const roleLabels = document.querySelectorAll('#role-selector label');
    roleLabels.forEach(label => {
      label.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        roleLabels.forEach(l => (l as HTMLElement).style.borderColor = 'var(--border-color)');
        (label as HTMLElement).style.borderColor = 'var(--accent-emerald)';
      });
    });

    const cancelBtn = document.getElementById('btn-cancel-creation');
    if (cancelBtn) {
      cancelBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.MAIN_MENU);
      });
    }

    const form = document.getElementById('player-creation-form');
    if (form) {
      form.addEventListener('submit', (e) => {
        e.preventDefault();
        AudioManager.getInstance().playFanfare();

        const firstName = (document.getElementById('input-first-name') as HTMLInputElement).value;
        const lastName = (document.getElementById('input-last-name') as HTMLInputElement).value;
        const nationality = (document.getElementById('select-nationality') as HTMLSelectElement).value;
        const jerseyNumber = parseInt((document.getElementById('input-jersey') as HTMLInputElement).value, 10) || 18;

        const roleInput = document.querySelector('input[name="role"]:checked') as HTMLInputElement;
        const role = (roleInput ? roleInput.value : PlayerRole.BATSMAN) as PlayerRole;

        const battingStyle = (document.getElementById('select-bat-style') as HTMLSelectElement).value as BattingStyle;
        const bowlingStyle = (document.getElementById('select-bowl-style') as HTMLSelectElement).value as BowlingStyle;

        EventBus.getInstance().emit('player:create', {
          firstName,
          lastName,
          nationality,
          jerseyNumber,
          role,
          battingStyle,
          bowlingStyle
        });
      });
    }
  }
}
