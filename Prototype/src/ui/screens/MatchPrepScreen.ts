import { TournamentInstance } from '../../tournament/TournamentModel';
import { PlayerData, PlayerRole } from '../../player/PlayerModel';
import { TournamentManager } from '../../tournament/TournamentManager';
import { SelectionManager } from '../../career/SelectionManager';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { EventBus } from '../../core/EventBus';
import { AudioManager } from '../../audio/AudioManager';
import { MatchPresentation, MatchConditions, TossResult } from '../../match/MatchPresentation';

export class MatchPrepScreen {
  private static cachedConditions: Record<string, MatchConditions> = {};
  private static tossResolved: boolean = false;
  private static tossResult: TossResult | null = null;
  private static selectedCall: 'HEADS' | 'TAILS' = 'HEADS';

  public static render(tournament: TournamentInstance | null, player: PlayerData, recentRatings: number[] = []): string {
    const fixture = tournament ? TournamentManager.getNextFixture(tournament) : null;
    if (!fixture) {
      return `
        <div class="main-viewport" style="text-align: center; padding: 4rem;">
          <h2>No upcoming fixture found.</h2>
          <button id="btn-prep-back" class="btn btn-secondary" style="margin-top: 1rem;">Back to Career Hub</button>
        </div>
      `;
    }

    if (!this.cachedConditions[fixture.id]) {
      this.cachedConditions[fixture.id] = MatchPresentation.generateMatchConditions(fixture.venueName);
      this.tossResolved = false;
      this.tossResult = null;
    }

    const conditions = this.cachedConditions[fixture.id];
    const selectionReport = SelectionManager.evaluateSelection(player, recentRatings);

    return `
      <div class="main-viewport" style="max-width: 950px;">
        <!-- Match Introduction Banner -->
        <div style="margin-bottom: 1.5rem; text-align: center;">
          <div class="badge badge-gold" style="margin-bottom: 0.5rem;">
            ${tournament?.name.toUpperCase()} • ROUND ${fixture.roundNumber}
          </div>
          <h2 style="font-family: var(--font-display); font-size: 3.25rem; letter-spacing: 0.05em; line-height: 1;">
            ${fixture.teamAName} <span style="color: var(--accent-gold);">VS</span> ${fixture.teamBName}
          </h2>
          <div style="font-size: 0.9rem; color: var(--text-muted); margin-top: 0.5rem;">
            📍 ${fixture.venueName} • Format: ${tournament?.format} • Weather: ${conditions.weather} (${conditions.temperatureC}°C)
          </div>
        </div>

        <div class="grid-2" style="gap: 1.25rem; margin-bottom: 1.5rem;">
          <!-- Pitch & Environmental Report -->
          <div class="card">
            <h3 style="font-size: 1rem; font-weight: 700; color: #fff; margin-bottom: 0.85rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.4rem;">
              🏟️ PITCH & GROUND CONDITIONS
            </h3>

            <div style="display: flex; flex-direction: column; gap: 0.75rem; font-size: 0.85rem;">
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Pitch Type:</span>
                <strong style="color: var(--accent-emerald);">${conditions.pitchType}</strong>
              </div>
              <p style="font-size: 0.8rem; color: var(--text-muted); background: var(--bg-card-alt); padding: 0.6rem; border-radius: 6px; border-left: 3px solid var(--accent-cyan);">
                ${conditions.pitchDescription}
              </p>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Outfield Pace:</span>
                <strong style="color: #fff;">${conditions.outfieldSpeed}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Your Role:</span>
                <strong style="color: var(--accent-gold);">${player.role} (${player.role.includes('Bowler') ? 'Pace Spearhead' : 'Batting #4'})</strong>
              </div>
            </div>
          </div>

          <!-- Interactive Coin Toss Ceremony -->
          <div class="card" style="border-color: ${this.tossResolved ? 'var(--accent-emerald)' : 'var(--accent-gold)'};">
            <h3 style="font-size: 1rem; font-weight: 700; color: #fff; margin-bottom: 0.85rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.4rem;">
              🪙 OFFICIAL COIN TOSS
            </h3>

            ${!this.tossResolved ? `
              <p style="font-size: 0.85rem; color: var(--text-main); margin-bottom: 1rem;">
                The two captains meet at the center with the match referee. Call the toss:
              </p>

              <div style="display: flex; gap: 0.75rem; margin-bottom: 1.25rem;">
                <button id="btn-call-heads" class="btn ${this.selectedCall === 'HEADS' ? 'btn-gold' : 'btn-secondary'}" style="flex: 1; padding: 0.75rem;">
                  🪙 HEADS
                </button>
                <button id="btn-call-tails" class="btn ${this.selectedCall === 'TAILS' ? 'btn-gold' : 'btn-secondary'}" style="flex: 1; padding: 0.75rem;">
                  🪙 TAILS
                </button>
              </div>

              <button id="btn-flip-coin" class="btn btn-primary" style="width: 100%; padding: 0.75rem;">
                FLIP COIN →
              </button>
            ` : `
              <div style="background: rgba(16, 185, 129, 0.08); border: 1px solid var(--accent-emerald); border-radius: 8px; padding: 1rem; margin-bottom: 0.75rem; text-align: center;">
                <div style="font-size: 1.75rem; margin-bottom: 0.25rem;">🪙</div>
                <div style="font-size: 0.8rem; color: var(--text-dim); text-transform: uppercase; font-weight: 700;">
                  Coin Landed: ${this.tossResult?.coinLanded}
                </div>
                <div style="font-size: 1.05rem; font-weight: 700; color: #fff; margin-top: 0.25rem;">
                  ${this.tossResult?.summaryText}
                </div>
              </div>

              <div style="font-size: 0.8rem; color: var(--text-muted); text-align: center;">
                Both captains have confirmed their playing XIs.
              </div>
            `}
          </div>
        </div>

        <!-- Selection Report Banner -->
        <div class="card" style="margin-bottom: 1.5rem; display: flex; justify-content: space-between; align-items: center;">
          <div>
            <div style="font-size: 0.75rem; color: var(--text-dim); text-transform: uppercase; font-weight: 700;">TEAM SHEET</div>
            <strong style="color: ${selectionReport.isSelected ? 'var(--accent-emerald)' : '#ef4444'}; font-size: 1.05rem;">
              ${selectionReport.isSelected ? '✅ NAMED IN PLAYING XI' : '⚠️ ON BENCH'}
            </strong>
            <span style="font-size: 0.85rem; color: var(--text-muted); margin-left: 0.5rem;">
              ${selectionReport.message}
            </span>
          </div>

          <div class="badge badge-primary">${player.role}</div>
        </div>

        <!-- Action Buttons -->
        <div style="display: flex; gap: 1rem; justify-content: center;">
          <button id="btn-prep-cancel" class="btn btn-secondary">
            ← Back to Hub
          </button>
          <button id="btn-start-match-sim" class="btn btn-primary" style="padding: 0.9rem 3rem; font-size: 1.1rem;">
            🏏 Enter Match Arena →
          </button>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const cancelBtn = document.getElementById('btn-prep-cancel') || document.getElementById('btn-prep-back');
    if (cancelBtn) {
      cancelBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      });
    }

    const headsBtn = document.getElementById('btn-call-heads');
    if (headsBtn) {
      headsBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        this.selectedCall = 'HEADS';
        EventBus.getInstance().emit('match:prep-refresh');
      });
    }

    const tailsBtn = document.getElementById('btn-call-tails');
    if (tailsBtn) {
      tailsBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        this.selectedCall = 'TAILS';
        EventBus.getInstance().emit('match:prep-refresh');
      });
    }

    const flipBtn = document.getElementById('btn-flip-coin');
    if (flipBtn) {
      flipBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        this.tossResolved = true;
        this.tossResult = {
          winnerTeamId: 'player_team',
          winnerTeamName: 'Your Team',
          electedTo: 'BAT',
          callChoice: this.selectedCall,
          coinLanded: this.selectedCall,
          summaryText: `Your team won the toss and elected to BAT first!`
        };
        AudioManager.getInstance().playFanfare();
        EventBus.getInstance().emit('match:prep-refresh');
      });
    }

    const startBtn = document.getElementById('btn-start-match-sim');
    if (startBtn) {
      startBtn.addEventListener('click', () => {
        AudioManager.getInstance().playBatHit();
        EventBus.getInstance().emit('match:start', {
          tossResult: this.tossResult
        });
      });
    }
  }
}
