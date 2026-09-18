import { MatchInstance } from '../../match/MatchModel';
import { PlayerData, PlayerRole } from '../../player/PlayerModel';
import { BattingShot, SHOT_CATALOG, DeliveryLine, DeliveryLength, ShotDirection } from '../../match/BattingEngine';
import { BowlingVariation, BowlingEngine } from '../../match/BowlingEngine';
import { CricketFieldView, FieldPreset } from '../../match/CricketFieldView';
import { MatchEventManager, MatchEventNotification } from '../../match/MatchEventManager';
import { EventBus } from '../../core/EventBus';
import { AudioManager } from '../../audio/AudioManager';

export class MatchScreen {
  private static selectedShot: BattingShot = BattingShot.COVER_DRIVE;
  private static selectedDirection: ShotDirection = ShotDirection.LEFT;
  private static selectedLine: DeliveryLine = DeliveryLine.OUTSIDE_OFF;
  private static selectedLength: DeliveryLength = DeliveryLength.GOOD_LENGTH;
  private static selectedVariation: BowlingVariation = BowlingVariation.NORMAL_PACE;
  private static currentFieldPreset: FieldPreset = 'BALANCED';

  private static fieldView: CricketFieldView | null = null;
  private static isPaused: boolean = false;
  private static meterOscillator: number = 0;
  private static meterInterval: number | null = null;

  public static render(match: MatchInstance | null, player: PlayerData): string {
    if (!match || !match.interactiveState) {
      return `<div class="main-viewport"><div class="card" style="text-align: center; padding: 3rem;"><h3>Preparing Match Arena...</h3></div></div>`;
    }

    const state = match.interactiveState;
    const currentInnings = match.innings[match.currentInningsIndex];
    const isChasing = match.currentInningsIndex === 1 && match.targetRuns !== undefined;

    const striker = state.battingLineup[state.strikerIdx];
    const nonStriker = state.battingLineup[state.nonStrikerIdx];
    const bowler = state.bowlingAttack[state.bowlerIdx];

    const currentRR = currentInnings.oversCompleted > 0 || state.currentBallInOver > 1
      ? ((currentInnings.totalRuns / Math.max(0.1, currentInnings.oversCompleted + (state.currentBallInOver - 1) / 6))).toFixed(2)
      : '0.00';

    const requiredRR = isChasing
      ? (((match.targetRuns! - currentInnings.totalRuns) / Math.max(0.1, match.oversPerSide - currentInnings.oversCompleted))).toFixed(2)
      : null;

    const strikerScore = currentInnings.batsmen[state.strikerIdx];
    const bowlerScore = currentInnings.bowlers[state.bowlerIdx];

    // Recent commentary lines
    const recentComm = match.commentaryLog.slice(-7).reverse().map(line => {
      let extra = '';
      if (line.includes('OUT!') || line.includes('WICKET')) extra = 'wicket';
      else if (line.includes('FOUR!') || line.includes('SIX!')) extra = 'boundary';
      return `<div class="commentary-entry ${extra}">${line}</div>`;
    }).join('');

    // Recent balls timeline
    const timelineHtml = (state.recentBallsTimeline || []).map(b => {
      let color = 'var(--text-muted)';
      let bg = 'rgba(255,255,255,0.05)';
      if (b === '4') { color = 'var(--accent-gold)'; bg = 'rgba(245, 158, 11, 0.15)'; }
      else if (b === '6') { color = 'var(--accent-purple)'; bg = 'rgba(168, 85, 247, 0.2)'; }
      else if (b === 'W' || b === 'RO' || b === 'C') { color = '#ef4444'; bg = 'rgba(239, 68, 68, 0.2)'; }
      return `<span style="display: inline-flex; align-items: center; justify-content: center; width: 26px; height: 26px; border-radius: 50%; font-size: 0.75rem; font-weight: 700; color: ${color}; background: ${bg}; border: 1px solid rgba(255,255,255,0.1);">${b}</span>`;
    }).join(' ');

    const latestEvent = MatchEventManager.getLatestEvent();

    return `
      <div class="main-viewport" style="max-width: 1240px; position: relative;">
        <!-- Top Scoreboard Bar -->
        <div class="card" style="background: linear-gradient(135deg, #0b1320 0%, #060a12 100%); border-color: #202d42; padding: 1rem 1.5rem; margin-bottom: 1rem;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.6rem; border-bottom: 1px solid rgba(255,255,255,0.08); padding-bottom: 0.4rem;">
            <div style="display: flex; align-items: center; gap: 0.75rem;">
              <span style="font-weight: 700; color: var(--accent-gold); font-size: 0.85rem;">
                ${match.tournamentName.toUpperCase()} • ${match.format}
              </span>
              ${state.isPowerplay ? `<span class="badge badge-primary" style="font-size: 0.7rem; padding: 0.15rem 0.5rem;">POWERPLAY (1-6)</span>` : ''}
              <span class="badge badge-secondary" style="font-size: 0.7rem; padding: 0.15rem 0.5rem;">FIELD: ${this.currentFieldPreset}</span>
            </div>

            <div style="font-size: 0.8rem; color: var(--text-dim);">
              📍 ${match.venue} • Pitch: ${match.conditions?.pitchType || 'BALANCED'}
            </div>

            <div style="display: flex; align-items: center; gap: 1rem;">
              <div style="font-size: 0.85rem; color: var(--accent-cyan); font-weight: 600;">
                ${isChasing ? `Target: ${match.targetRuns} | Req RR: ${requiredRR}` : `Current RR: ${currentRR}`}
              </div>
              <button id="btn-match-pause" class="btn btn-secondary" style="padding: 0.3rem 0.75rem; font-size: 0.8rem;">
                ⏸ Pause
              </button>
            </div>
          </div>

          <div style="display: grid; grid-template-columns: 1.5fr 1fr 1.2fr; gap: 1.25rem; align-items: center;">
            <!-- Score & Overs -->
            <div>
              <div style="font-size: 0.85rem; color: var(--text-muted); font-weight: 600;">
                ${currentInnings.battingTeamName} ${match.currentInningsIndex === 1 ? '(CHASING)' : ''}
              </div>
              <div style="font-family: var(--font-display); font-size: 3.25rem; line-height: 1; font-weight: 700; color: #fff;">
                ${currentInnings.totalRuns}<span style="color: var(--text-dim); font-size: 2.25rem;">/${currentInnings.totalWickets}</span>
                <span style="font-size: 1.25rem; color: var(--text-muted); font-family: var(--font-body); font-weight: 500; margin-left: 0.5rem;">
                  (${currentInnings.oversCompleted}.${state.currentBallInOver - 1} / ${match.oversPerSide} ov)
                </span>
              </div>
              <div style="display: flex; align-items: center; gap: 0.4rem; margin-top: 0.35rem;">
                <span style="font-size: 0.75rem; color: var(--text-dim);">THIS OVER:</span>
                ${timelineHtml || '<span style="font-size: 0.75rem; color: var(--text-dim);">Over started</span>'}
              </div>
            </div>

            <!-- Crease Matchup Card -->
            <div style="background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; padding: 0.6rem 1rem; font-size: 0.8rem;">
              <div style="margin-bottom: 0.35rem;">
                <span style="color: var(--text-dim);">STRIKER: </span>
                <strong style="color: ${striker?.isUser ? 'var(--accent-emerald)' : '#fff'};">
                  ${striker?.name} ${striker?.isUser ? '★' : ''} ${strikerScore?.runs || 0} (${strikerScore?.balls || 0}b)
                </strong>
              </div>
              <div style="margin-bottom: 0.35rem;">
                <span style="color: var(--text-dim);">NON-STRIKER: </span>
                <strong style="color: #fff;">${nonStriker?.name}</strong>
              </div>
              <div>
                <span style="color: var(--text-dim);">BOWLER: </span>
                <strong style="color: ${bowler?.isUser ? 'var(--accent-cyan)' : '#fff'};">
                  ${bowler?.name} ${bowler?.isUser ? '★' : ''} ${bowlerScore?.wickets || 0}/${bowlerScore?.runs || 0} (${bowlerScore?.overs || 0} ov)
                </strong>
              </div>
            </div>

            <!-- Partnership & Live Event -->
            <div style="background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; padding: 0.6rem 1rem; font-size: 0.8rem; display: flex; flex-direction: column; justify-content: space-between;">
              <div>
                <span style="color: var(--text-dim);">PARTNERSHIP: </span>
                <strong style="color: var(--accent-gold);">${state.partnershipRuns || 0} runs (${state.partnershipBalls || 0} balls)</strong>
              </div>
              ${latestEvent && (Date.now() - latestEvent.timestamp < 12000) ? `
                <div style="margin-top: 0.35rem; background: rgba(245, 158, 11, 0.12); border: 1px solid var(--accent-gold); border-radius: 4px; padding: 0.35rem 0.5rem;">
                  <strong style="color: var(--accent-gold);">${latestEvent.title}</strong>
                  <div style="font-size: 0.75rem; color: var(--text-main);">${latestEvent.subtitle}</div>
                </div>
              ` : `
                <div style="font-size: 0.75rem; color: var(--text-dim); margin-top: 0.35rem;">
                  Match flow in progress. Steady pace.
                </div>
              `}
            </div>
          </div>
        </div>

        <!-- 2D Canvas Cricket Field & Dynamic Camera -->
        <div class="card" style="padding: 0.75rem; margin-bottom: 1rem; background: #080d14; border-color: #1e293b; text-align: center;">
          <canvas id="cricket-field-canvas" width="800" height="340" style="width: 100%; max-height: 340px; border-radius: 6px; background: #0c1a0f;"></canvas>
          <div style="display: flex; justify-content: space-between; align-items: center; margin-top: 0.5rem; font-size: 0.75rem; color: var(--text-dim); padding: 0 0.5rem;">
            <span>Tactics: <strong>${this.currentFieldPreset} FIELD</strong> ${state.isPowerplay ? '(Infield restricted)' : ''}</span>
            <div style="display: flex; gap: 0.5rem;">
              <button class="btn btn-secondary btn-field-preset" data-preset="ATTACKING" style="padding: 0.2rem 0.5rem; font-size: 0.7rem;">Attacking</button>
              <button class="btn btn-secondary btn-field-preset" data-preset="BALANCED" style="padding: 0.2rem 0.5rem; font-size: 0.7rem;">Balanced</button>
              <button class="btn btn-secondary btn-field-preset" data-preset="DEFENSIVE" style="padding: 0.2rem 0.5rem; font-size: 0.7rem;">Defensive</button>
            </div>
          </div>
        </div>

        <!-- Interactive Gameplay Area -->
        <div class="grid-3" style="gap: 1.25rem; margin-bottom: 1.25rem;">
          <!-- Left Column: Phase Controls (2 spans) -->
          <div class="card" style="grid-column: span 2; display: flex; flex-direction: column; justify-content: space-between;">
            ${this.renderPhaseControls(match, player, state)}
          </div>

          <!-- Right Column: Player Scorecard & Commentary Feed -->
          <div style="display: flex; flex-direction: column; gap: 1rem;">
            <!-- Player Stats in Match -->
            <div class="card">
              <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.3rem;">
                <h4 style="font-size: 0.85rem; font-weight: 700; color: #fff;">YOUR SCORECARD</h4>
                <span class="badge badge-gold" style="font-size: 0.7rem;">${player.role}</span>
              </div>
              <div style="display: flex; flex-direction: column; gap: 0.4rem; font-size: 0.8rem;">
                <div style="display: flex; justify-content: space-between;">
                  <span style="color: var(--text-dim);">Batting:</span>
                  <strong style="color: var(--accent-emerald);">
                    ${match.playerPerformance.runs} runs (${match.playerPerformance.balls}b, ${match.playerPerformance.fours}x4, ${match.playerPerformance.sixes}x6)
                  </strong>
                </div>
                <div style="display: flex; justify-content: space-between;">
                  <span style="color: var(--text-dim);">Bowling:</span>
                  <strong style="color: var(--accent-cyan);">
                    ${match.playerPerformance.wickets}/${match.playerPerformance.runsConceded} (${match.playerPerformance.overs} ov)
                  </strong>
                </div>
                <div style="display: flex; justify-content: space-between;">
                  <span style="color: var(--text-dim);">Fielding:</span>
                  <strong>${match.playerPerformance.catches} catches • ${match.playerPerformance.runOuts} run-outs</strong>
                </div>
              </div>
            </div>

            <!-- Commentary Log -->
            <div class="card" style="flex: 1; display: flex; flex-direction: column;">
              <div style="font-size: 0.8rem; font-weight: 700; color: #fff; margin-bottom: 0.4rem;">
                BALL-BY-BALL BROADCAST
              </div>
              <div class="commentary-box" style="flex: 1; max-height: 220px;">
                ${recentComm || '<div style="color: var(--text-dim); text-align: center; margin-top: 1rem;">Match in progress...</div>'}
              </div>
            </div>
          </div>
        </div>

        <!-- Pause Modal -->
        ${this.isPaused ? `
          <div style="position: fixed; inset: 0; background: rgba(0,0,0,0.8); backdrop-filter: blur(8px); display: flex; align-items: center; justify-content: center; z-index: 999;">
            <div class="card" style="width: 360px; text-align: center; border-color: var(--accent-cyan); padding: 2rem;">
              <h3 style="font-size: 1.5rem; font-weight: 800; color: #fff; margin-bottom: 1rem;">MATCH PAUSED</h3>
              <div style="display: flex; flex-direction: column; gap: 0.75rem;">
                <button id="btn-resume-match" class="btn btn-primary" style="padding: 0.75rem;">▶ Resume Match</button>
                <button id="btn-restart-match" class="btn btn-secondary" style="padding: 0.75rem;">🔄 Restart Match</button>
                <button id="btn-quit-match" class="btn btn-danger" style="padding: 0.75rem;">🚪 Abandon & Return to Hub</button>
              </div>
            </div>
          </div>
        ` : ''}
      </div>
    `;
  }

  private static renderPhaseControls(match: MatchInstance, player: PlayerData, state: any): string {
    // 1. User Batting
    if (state.phase === 'USER_BATTING' && state.pendingDelivery) {
      const del = state.pendingDelivery;
      return `
        <div>
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.85rem;">
            <div class="badge badge-success" style="font-size: 0.8rem; padding: 0.35rem 0.85rem;">
              🏏 YOU ARE ON STRIKE!
            </div>
            <div style="font-size: 0.85rem; color: var(--accent-gold); font-weight: 600;">
              Incoming: <span style="color: #fff;">${del.length} on ${del.line}</span> (${del.paceKph} km/h • ${del.variation})
            </div>
          </div>

          <!-- Direction Aiming Buttons -->
          <div style="margin-bottom: 0.85rem;">
            <div style="font-size: 0.75rem; color: var(--text-dim); text-transform: uppercase; font-weight: 700; margin-bottom: 0.4rem;">
              1. CHOOSE SHOT DIRECTION (AIM):
            </div>
            <div style="display: flex; gap: 0.5rem;">
              <button class="btn btn-direction-select ${this.selectedDirection === ShotDirection.LEFT ? 'btn-gold' : 'btn-secondary'}" data-dir="${ShotDirection.LEFT}" style="flex: 1; padding: 0.5rem; font-size: 0.8rem;">
                ↖ LEFT (Off-Side / Covers / Point)
              </button>
              <button class="btn btn-direction-select ${this.selectedDirection === ShotDirection.CENTER ? 'btn-gold' : 'btn-secondary'}" data-dir="${ShotDirection.CENTER}" style="flex: 1; padding: 0.5rem; font-size: 0.8rem;">
                ⬆ CENTER (Down Ground / V)
              </button>
              <button class="btn btn-direction-select ${this.selectedDirection === ShotDirection.RIGHT ? 'btn-gold' : 'btn-secondary'}" data-dir="${ShotDirection.RIGHT}" style="flex: 1; padding: 0.5rem; font-size: 0.8rem;">
                ↗ RIGHT (Leg-Side / Mid-Wicket)
              </button>
            </div>
          </div>

          <!-- Timing Gauge Visualization -->
          <div style="background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; padding: 0.85rem; margin-bottom: 0.85rem; text-align: center;">
            <div style="position: relative; height: 26px; background: #1e293b; border-radius: 13px; overflow: hidden; border: 1px solid #334155; margin-bottom: 0.4rem;">
              <div style="position: absolute; left: 0%; width: 35%; height: 100%; background: rgba(239, 68, 68, 0.25);"></div>
              <div style="position: absolute; left: 35%; width: 30%; height: 100%; background: rgba(245, 158, 11, 0.35);"></div>
              <div style="position: absolute; left: 45%; width: 10%; height: 100%; background: rgba(16, 185, 129, 0.7); box-shadow: 0 0 10px #10b981;"></div>
              <div style="position: absolute; right: 0%; width: 35%; height: 100%; background: rgba(239, 68, 68, 0.25);"></div>
              <div id="timing-indicator" style="position: absolute; left: 50%; width: 4px; height: 100%; background: #fff; box-shadow: 0 0 8px #fff;"></div>
            </div>
            <div style="display: flex; justify-content: space-between; font-size: 0.7rem; color: var(--text-dim);">
              <span>Early</span>
              <span style="color: var(--accent-emerald); font-weight: 700;">SWEET SPOT</span>
              <span>Late</span>
            </div>
          </div>

          <!-- Shot Selection Grid -->
          <div style="font-size: 0.75rem; color: var(--text-dim); text-transform: uppercase; font-weight: 700; margin-bottom: 0.4rem;">
            2. CHOOSE SHOT TYPE:
          </div>
          <div class="grid-4" style="gap: 0.5rem; margin-bottom: 1rem;">
            ${Object.values(SHOT_CATALOG).map(cfg => `
              <button class="btn btn-secondary btn-shot-select" data-shot="${cfg.shot}"
                style="padding: 0.5rem 0.4rem; font-size: 0.75rem; text-align: left; display: flex; flex-direction: column; gap: 0.15rem; border-color: ${this.selectedShot === cfg.shot ? 'var(--accent-emerald)' : 'var(--border-color)'}; background: ${this.selectedShot === cfg.shot ? 'rgba(16, 185, 129, 0.1)' : 'var(--bg-main)'};">
                <strong style="color: #fff;">${cfg.displayName}</strong>
                <span style="font-size: 0.65rem; color: var(--text-dim);">Max ${cfg.maxRewardRuns}r • ${cfg.risk}</span>
              </button>
            `).join('')}
          </div>

          <div style="display: flex; justify-content: space-between; align-items: center;">
            <div style="font-size: 0.75rem; color: var(--text-muted);">
              Selected: <strong style="color: var(--accent-emerald);">${SHOT_CATALOG[this.selectedShot]?.displayName}</strong> + <strong style="color: var(--accent-gold);">${this.selectedDirection}</strong>
            </div>

            <button id="btn-execute-shot" class="btn btn-primary" style="padding: 0.75rem 2.5rem; font-size: 1rem;">
              🏏 TIME & EXECUTE SHOT
            </button>
          </div>
        </div>
      `;
    }

    // 2. User Bowling
    if (state.phase === 'USER_BOWLING') {
      const variations = BowlingEngine.getAvailableVariations(player.bowlingStyle);
      return `
        <div>
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.85rem;">
            <div class="badge badge-primary" style="font-size: 0.8rem; padding: 0.35rem 0.85rem;">
              ⚡ YOU ARE BOWLING (Over ${state.currentOver}.${state.currentBallInOver})
            </div>
            <div style="font-size: 0.85rem; color: var(--text-muted);">
              Facing: <strong style="color: #fff;">${state.battingLineup[state.strikerIdx]?.name}</strong>
            </div>
          </div>

          <!-- Target Selectors -->
          <div class="grid-3" style="gap: 0.75rem; margin-bottom: 1rem;">
            <div>
              <label style="font-size: 0.7rem; color: var(--text-dim); font-weight: 700; display: block; margin-bottom: 0.3rem;">TARGET LINE</label>
              <select id="select-bowl-line" style="width: 100%; padding: 0.5rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 6px; color: #fff; font-size: 0.8rem;">
                ${Object.values(DeliveryLine).map(line => `
                  <option value="${line}" ${this.selectedLine === line ? 'selected' : ''}>${line}</option>
                `).join('')}
              </select>
            </div>

            <div>
              <label style="font-size: 0.7rem; color: var(--text-dim); font-weight: 700; display: block; margin-bottom: 0.3rem;">TARGET LENGTH</label>
              <select id="select-bowl-length" style="width: 100%; padding: 0.5rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 6px; color: #fff; font-size: 0.8rem;">
                ${Object.values(DeliveryLength).map(len => `
                  <option value="${len}" ${this.selectedLength === len ? 'selected' : ''}>${len}</option>
                `).join('')}
              </select>
            </div>

            <div>
              <label style="font-size: 0.7rem; color: var(--text-dim); font-weight: 700; display: block; margin-bottom: 0.3rem;">VARIATION</label>
              <select id="select-bowl-variation" style="width: 100%; padding: 0.5rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 6px; color: #fff; font-size: 0.8rem;">
                ${variations.map(v => `
                  <option value="${v}" ${this.selectedVariation === v ? 'selected' : ''}>${v}</option>
                `).join('')}
              </select>
            </div>
          </div>

          <!-- Interactive Bowling Accuracy Meter -->
          <div style="background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 8px; padding: 0.85rem; margin-bottom: 1rem; text-align: center;">
            <div style="font-size: 0.75rem; color: var(--text-dim); font-weight: 700; margin-bottom: 0.4rem;">
              ACCURACY RELEASE METER
            </div>
            <div style="position: relative; height: 26px; background: #1e293b; border-radius: 13px; overflow: hidden; border: 1px solid #334155;">
              <div style="position: absolute; left: 45%; width: 10%; height: 100%; background: rgba(16, 185, 129, 0.7); box-shadow: 0 0 8px #10b981;"></div>
              <div id="bowling-meter-needle" style="position: absolute; left: 50%; width: 4px; height: 100%; background: #38bdf8; box-shadow: 0 0 6px #38bdf8;"></div>
            </div>
          </div>

          <div style="display: flex; justify-content: flex-end;">
            <button id="btn-deliver-ball" class="btn btn-primary" style="padding: 0.75rem 2.5rem; font-size: 1rem;">
              ⚡ RELEASE DELIVERY
            </button>
          </div>
        </div>
      `;
    }

    // 3. Running Decision Prompt
    if (state.phase === 'RUNNING_DECISION' && state.pendingRunningOpp) {
      const opp = state.pendingRunningOpp;
      return `
        <div style="background: rgba(245, 158, 11, 0.08); border: 2px dashed var(--accent-gold); border-radius: 12px; padding: 1.5rem; text-align: center;">
          <h3 style="font-size: 1.35rem; font-weight: 800; color: var(--accent-gold); margin-bottom: 0.4rem;">
            🏃 RUNNING OPPORTUNITY!
          </h3>
          <p style="font-size: 0.9rem; color: var(--text-main); margin-bottom: 1.25rem;">
            ${opp.commentaryHint} Risk: <strong style="color: ${opp.extraRunRisk === 'HIGH' ? '#ef4444' : 'var(--accent-emerald)'};">${opp.extraRunRisk}</strong>
          </p>

          <div style="display: flex; gap: 1rem; justify-content: center;">
            <button id="btn-running-stay" class="btn btn-secondary" style="padding: 0.75rem 2rem; font-size: 0.95rem;">
              ✋ STAY (Keep Safe ${opp.baseRunsScored}r)
            </button>
            <button id="btn-running-run" class="btn btn-gold" style="padding: 0.75rem 2.5rem; font-size: 1.05rem; font-weight: 700;">
              ⚡ RUN! PUSH FOR EXTRA RUN!
            </button>
          </div>
        </div>
      `;
    }

    // 4. User Fielding Prompt
    if (state.phase === 'USER_FIELDING' && state.pendingFieldingPrompt) {
      const prompt = state.pendingFieldingPrompt;
      return `
        <div style="background: rgba(239, 68, 68, 0.08); border: 2px dashed var(--accent-crimson); border-radius: 12px; padding: 1.5rem; text-align: center;">
          <h3 style="font-size: 1.4rem; font-weight: 800; color: var(--accent-gold); margin-bottom: 0.4rem;">
            ${prompt.title}
          </h3>
          <p style="font-size: 0.9rem; color: var(--text-main); margin-bottom: 1.25rem;">
            ${prompt.description}
          </p>

          ${prompt.throwOptions ? `
            <div style="display: flex; gap: 0.5rem; justify-content: center; margin-bottom: 0.5rem;">
              ${prompt.throwOptions.map((opt: { target: string; label: string }) => `
                <button class="btn btn-primary btn-throw-target" data-target="${opt.target}" style="padding: 0.75rem 1.25rem; font-size: 0.85rem;">
                  🎯 ${opt.label}
                </button>
              `).join('')}
            </div>
          ` : `
            <button id="btn-react-fielding" class="btn btn-gold" style="padding: 0.85rem 3rem; font-size: 1.2rem; font-weight: 800;">
              🧤 POUCH CATCH NOW!
            </button>
          `}
        </div>
      `;
    }

    // 5. Innings Break
    if (state.phase === 'INNINGS_BREAK') {
      return `
        <div style="text-align: center; padding: 1.5rem;">
          <div class="badge badge-gold" style="margin-bottom: 0.5rem;">INNINGS BREAK</div>
          <h3 style="font-size: 1.75rem; font-weight: 700; color: #fff; margin-bottom: 0.4rem;">
            First Innings Completed
          </h3>
          <p style="color: var(--text-muted); font-size: 0.95rem; margin-bottom: 1.25rem;">
            Target to win: <strong style="color: var(--accent-emerald); font-size: 1.2rem;">${match.targetRuns} runs</strong> from ${match.oversPerSide} overs.
          </p>
          <button id="btn-start-second-innings" class="btn btn-primary" style="padding: 0.75rem 2.5rem; font-size: 1rem;">
            ▶ Start Second Innings
          </button>
        </div>
      `;
    }

    // 6. Match Concluded
    if (state.phase === 'MATCH_CONCLUDED') {
      return `
        <div style="text-align: center; padding: 1.5rem;">
          <div class="badge badge-gold" style="margin-bottom: 0.5rem;">MATCH FINISHED</div>
          <h3 style="font-size: 1.85rem; font-weight: 700; color: #fff; margin-bottom: 0.4rem;">
            ${match.resultSummary}
          </h3>
          <button id="btn-view-match-results" class="btn btn-primary" style="padding: 0.75rem 2.5rem; font-size: 1rem; margin-top: 0.75rem;">
            View Match Summary & Performance →
          </button>
        </div>
      `;
    }

    // 7. AI Innings
    return `
      <div style="text-align: center; padding: 1.5rem;">
        <div style="font-size: 0.85rem; color: var(--text-muted); margin-bottom: 1.25rem;">
          AI players at crease. You will take control when you bat, bowl, or get a fielding chance.
        </div>
        <div style="display: flex; justify-content: center; gap: 1rem;">
          <button id="btn-sim-to-my-turn" class="btn btn-primary" style="padding: 0.75rem 1.75rem; font-size: 0.95rem;">
            ⚡ Fast-Forward to My Turn
          </button>
          <button id="btn-sim-full-rest" class="btn btn-secondary" style="padding: 0.75rem 1.75rem; font-size: 0.95rem;">
            ⏩ Fast-Forward Match to End
          </button>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    // Mount canvas field view
    const canvas = document.getElementById('cricket-field-canvas') as HTMLCanvasElement;
    if (canvas) {
      if (this.fieldView) this.fieldView.destroy();
      this.fieldView = new CricketFieldView(canvas);
      this.fieldView.setFieldPreset(this.currentFieldPreset);
    }

    // Field preset toggles
    document.querySelectorAll('.btn-field-preset').forEach(btn => {
      btn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        const preset = btn.getAttribute('data-preset') as FieldPreset;
        if (preset) {
          this.currentFieldPreset = preset;
          if (this.fieldView) this.fieldView.setFieldPreset(preset);
          EventBus.getInstance().emit('match:ui-refresh');
        }
      });
    });

    // Direction selection
    document.querySelectorAll('.btn-direction-select').forEach(btn => {
      btn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        const dir = btn.getAttribute('data-dir') as ShotDirection;
        if (dir) {
          this.selectedDirection = dir;
          EventBus.getInstance().emit('match:ui-refresh');
        }
      });
    });

    // Shot selection
    document.querySelectorAll('.btn-shot-select').forEach(btn => {
      btn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        const shot = btn.getAttribute('data-shot') as BattingShot;
        if (shot) {
          this.selectedShot = shot;
          EventBus.getInstance().emit('match:ui-refresh');
        }
      });
    });

    // Execute Shot Button
    const execShotBtn = document.getElementById('btn-execute-shot');
    if (execShotBtn) {
      execShotBtn.addEventListener('click', () => {
        AudioManager.getInstance().playBatHit();
        const offset = Math.floor(Math.random() * 60) - 25; // Good/sweet spot
        EventBus.getInstance().emit('match:interactive-bat-shot', {
          shot: this.selectedShot,
          direction: this.selectedDirection,
          timingOffsetMs: offset
        });
      });
    }

    // Deliver Ball Button
    const deliverBtn = document.getElementById('btn-deliver-ball');
    if (deliverBtn) {
      deliverBtn.addEventListener('click', () => {
        AudioManager.getInstance().playBatHit(0.9);
        const line = (document.getElementById('select-bowl-line') as HTMLSelectElement)?.value as DeliveryLine || DeliveryLine.OUTSIDE_OFF;
        const length = (document.getElementById('select-bowl-length') as HTMLSelectElement)?.value as DeliveryLength || DeliveryLength.GOOD_LENGTH;
        const variation = (document.getElementById('select-bowl-variation') as HTMLSelectElement)?.value as BowlingVariation || BowlingVariation.NORMAL_PACE;

        this.selectedLine = line;
        this.selectedLength = length;
        this.selectedVariation = variation;

        EventBus.getInstance().emit('match:interactive-bowl-delivery', {
          line,
          length,
          variation,
          effortPacePercentage: 100,
          meterQuality: 0.90
        });
      });
    }

    // Running decision buttons
    const stayBtn = document.getElementById('btn-running-stay');
    if (stayBtn) {
      stayBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('match:running-decision', false);
      });
    }

    const runBtn = document.getElementById('btn-running-run');
    if (runBtn) {
      runBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('match:running-decision', true);
      });
    }

    // Fielding reaction button
    const reactFieldBtn = document.getElementById('btn-react-fielding');
    if (reactFieldBtn) {
      reactFieldBtn.addEventListener('click', () => {
        AudioManager.getInstance().playCatch();
        EventBus.getInstance().emit('match:interactive-field-react', { reactionTimeMs: 500 });
      });
    }

    // Throw target buttons
    document.querySelectorAll('.btn-throw-target').forEach(btn => {
      btn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        const target = btn.getAttribute('data-target') as any;
        EventBus.getInstance().emit('match:interactive-field-react', { reactionTimeMs: 500, selectedTarget: target });
      });
    });

    // Pause Controls
    const pauseBtn = document.getElementById('btn-match-pause');
    if (pauseBtn) {
      pauseBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        this.isPaused = true;
        EventBus.getInstance().emit('match:ui-refresh');
      });
    }

    const resumeBtn = document.getElementById('btn-resume-match');
    if (resumeBtn) {
      resumeBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        this.isPaused = false;
        EventBus.getInstance().emit('match:ui-refresh');
      });
    }

    const restartBtn = document.getElementById('btn-restart-match');
    if (restartBtn) {
      restartBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        this.isPaused = false;
        EventBus.getInstance().emit('match:restart');
      });
    }

    const quitBtn = document.getElementById('btn-quit-match');
    if (quitBtn) {
      quitBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        this.isPaused = false;
        EventBus.getInstance().emit('match:abandon');
      });
    }

    // Fast forwards
    const simMyTurnBtn = document.getElementById('btn-sim-to-my-turn');
    if (simMyTurnBtn) {
      simMyTurnBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('match:sim-to-my-turn');
      });
    }

    const simRestBtn = document.getElementById('btn-sim-full-rest');
    if (simRestBtn) {
      simRestBtn.addEventListener('click', () => {
        AudioManager.getInstance().playBatHit(1.2);
        EventBus.getInstance().emit('match:fast-forward');
      });
    }

    // Second Innings
    const start2ndBtn = document.getElementById('btn-start-second-innings');
    if (start2ndBtn) {
      start2ndBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('match:start-second-innings');
      });
    }

    // View Results
    const viewResultsBtn = document.getElementById('btn-view-match-results');
    if (viewResultsBtn) {
      viewResultsBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('match:view-results');
      });
    }
  }

  public static getFieldView(): CricketFieldView | null {
    return this.fieldView;
  }
}
