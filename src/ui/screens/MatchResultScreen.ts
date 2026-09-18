import { MatchInstance } from '../../match/MatchModel';
import { PlayerData } from '../../player/PlayerModel';
import { EventBus } from '../../core/EventBus';
import { AudioManager } from '../../audio/AudioManager';
import { Formatters } from '../../utilities/Formatters';

export class MatchResultScreen {
  public static render(match: MatchInstance | null, player: PlayerData): string {
    if (!match || !match.playerPerformance.evaluation) {
      return `<div class="main-viewport"><h2>Match processing...</h2></div>`;
    }

    const evalResult = match.playerPerformance.evaluation;
    const inn1 = match.innings[0];
    const inn2 = match.innings[1];

    return `
      <div class="main-viewport" style="max-width: 950px;">
        <!-- Match Result Headline -->
        <div class="hero-banner" style="margin-bottom: 2rem;">
          <div>
            <div class="badge badge-gold" style="margin-bottom: 0.5rem;">MATCH CONCLUDED</div>
            <h2 style="font-family: var(--font-display); font-size: 3.25rem; letter-spacing: 0.05em; line-height: 1.05;">
              ${match.resultSummary}
            </h2>
            <div style="font-size: 0.95rem; color: var(--text-muted); margin-top: 0.75rem;">
              ${inn1.battingTeamName} ${inn1.totalRuns}/${inn1.totalWickets} (${inn1.oversCompleted} ov) vs ${inn2.battingTeamName} ${inn2.totalRuns}/${inn2.totalWickets} (${inn2.oversCompleted} ov)
            </div>
            ${match.manOfTheMatchPlayerName ? `
              <div style="margin-top: 0.75rem; font-size: 0.85rem; color: var(--accent-gold); font-weight: 600;">
                ⭐ Player of the Match: ${match.manOfTheMatchPlayerName}
              </div>
            ` : ''}
          </div>

          <div style="text-align: right; z-index: 2;">
            <div style="font-family: var(--font-display); font-size: 4.5rem; line-height: 1; color: var(--accent-emerald); font-weight: 700;">
              ${evalResult.matchRating.toFixed(1)}
            </div>
            <div style="font-size: 0.75rem; color: var(--text-dim); text-transform: uppercase; font-weight: 700;">
              YOUR MATCH RATING
            </div>
          </div>
        </div>

        <div class="grid-2" style="margin-bottom: 2rem;">
          <!-- Performance Evaluation Card -->
          <div class="card">
            <h3 style="font-size: 1rem; font-weight: 700; color: #fff; margin-bottom: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
              PERFORMANCE IMPACT & PROGRESSION
            </h3>

            <div style="font-style: italic; color: var(--text-muted); font-size: 0.9rem; margin-bottom: 1.25rem; background: var(--bg-card-alt); padding: 0.85rem; border-radius: 8px; border-left: 3px solid var(--accent-gold);">
              "${evalResult.summary}"
            </div>

            <div class="grid-2" style="gap: 0.75rem; margin-bottom: 1.25rem;">
              <div style="background: var(--bg-main); padding: 0.75rem; border-radius: 8px; border: 1px solid var(--border-color);">
                <div style="font-size: 0.75rem; color: var(--text-dim);">FORM IMPACT</div>
                <div style="font-size: 1.35rem; font-weight: 700; color: ${evalResult.formDelta >= 0 ? 'var(--accent-emerald)' : '#ef4444'};">
                  ${evalResult.formDelta >= 0 ? '+' : ''}${evalResult.formDelta} Form
                </div>
              </div>

              <div style="background: var(--bg-main); padding: 0.75rem; border-radius: 8px; border: 1px solid var(--border-color);">
                <div style="font-size: 0.75rem; color: var(--text-dim);">CONFIDENCE IMPACT</div>
                <div style="font-size: 1.35rem; font-weight: 700; color: ${evalResult.confidenceDelta >= 0 ? 'var(--accent-gold)' : '#ef4444'};">
                  ${evalResult.confidenceDelta >= 0 ? '+' : ''}${evalResult.confidenceDelta} Conf
                </div>
              </div>

              <div style="background: var(--bg-main); padding: 0.75rem; border-radius: 8px; border: 1px solid var(--border-color);">
                <div style="font-size: 0.75rem; color: var(--text-dim);">FATIGUE DRAIN</div>
                <div style="font-size: 1.35rem; font-weight: 700; color: var(--accent-crimson);">
                  ${evalResult.fitnessDelta}% Stamina
                </div>
              </div>

              <div style="background: var(--bg-main); padding: 0.75rem; border-radius: 8px; border: 1px solid var(--border-color);">
                <div style="font-size: 0.75rem; color: var(--text-dim);">EXPERIENCE GAINED</div>
                <div style="font-size: 1.35rem; font-weight: 700; color: var(--accent-cyan);">
                  +${evalResult.xpGained} XP
                </div>
              </div>
            </div>
          </div>

          <!-- Player Individual Match Stats -->
          <div class="card">
            <h3 style="font-size: 1rem; font-weight: 700; color: #fff; margin-bottom: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
              YOUR PERSONAL SCORECARD
            </h3>

            <table class="cricket-table" style="margin-bottom: 1.5rem;">
              <tbody>
                <tr>
                  <td style="color: var(--text-dim);">Batting</td>
                  <td style="font-weight: 700; font-size: 1rem; color: var(--accent-emerald);">
                    ${match.playerPerformance.runs} runs (${match.playerPerformance.balls} balls, ${match.playerPerformance.fours}x4, ${match.playerPerformance.sixes}x6)
                  </td>
                </tr>
                <tr>
                  <td style="color: var(--text-dim);">Dismissal</td>
                  <td>${match.playerPerformance.isOut ? match.playerPerformance.dismissal : 'Not Out'}</td>
                </tr>
                <tr>
                  <td style="color: var(--text-dim);">Bowling</td>
                  <td style="font-weight: 700; font-size: 1rem; color: var(--accent-cyan);">
                    ${match.playerPerformance.wickets} wickets for ${match.playerPerformance.runsConceded} runs (${match.playerPerformance.overs} overs)
                  </td>
                </tr>
                <tr>
                  <td style="color: var(--text-dim);">Fielding</td>
                  <td>${match.playerPerformance.catches} catches, ${match.playerPerformance.runOuts} run-outs</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div style="display: flex; justify-content: center; gap: 1rem;">
          <button id="btn-conclude-match" class="btn btn-primary" style="padding: 0.95rem 3rem; font-size: 1.1rem;">
            Continue to Career Hub →
          </button>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const concludeBtn = document.getElementById('btn-conclude-match');
    if (concludeBtn) {
      concludeBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('match:conclude');
      });
    }
  }
}
