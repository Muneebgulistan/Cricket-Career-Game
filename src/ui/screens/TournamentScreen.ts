import { TournamentInstance } from '../../tournament/TournamentModel';
import { PlayerData } from '../../player/PlayerModel';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { AudioManager } from '../../audio/AudioManager';

export class TournamentScreen {
  public static render(tournament: TournamentInstance | null, player: PlayerData): string {
    if (!tournament) {
      return `
        <div class="main-viewport" style="text-align: center; padding: 4rem 1rem;">
          <h2>No active tournament</h2>
          <button id="btn-tourn-back" class="btn btn-secondary" style="margin-top: 1rem;">Return to Career Hub</button>
        </div>
      `;
    }

    const standingsRows = tournament.standings.map((s, idx) => {
      const isPlayerTeam = s.teamId === tournament.playerTeamId;
      return `
        <tr style="${isPlayerTeam ? 'background: rgba(16, 185, 129, 0.08); font-weight: 700;' : ''}">
          <td style="color: ${idx === 0 ? 'var(--accent-gold)' : 'var(--text-dim)'};">${idx + 1}</td>
          <td>
            ${s.teamName} ${isPlayerTeam ? '<span class="badge badge-success" style="font-size: 0.65rem; margin-left: 0.5rem;">YOUR TEAM</span>' : ''}
          </td>
          <td>${s.played}</td>
          <td style="color: var(--accent-emerald);">${s.won}</td>
          <td style="color: var(--accent-crimson);">${s.lost}</td>
          <td>${s.tied}</td>
          <td style="font-weight: 800; color: var(--accent-gold);">${s.points}</td>
        </tr>
      `;
    }).join('');

    const fixturesRows = tournament.fixtures.map(f => {
      const isPlayerFixture = f.teamAId === tournament.playerTeamId || f.teamBId === tournament.playerTeamId;
      return `
        <div style="display: flex; align-items: center; justify-content: space-between; padding: 0.85rem 1rem; background: ${isPlayerFixture ? 'rgba(56, 189, 248, 0.06)' : 'var(--bg-main)'}; border: 1px solid var(--border-color); border-radius: 8px; margin-bottom: 0.5rem;">
          <div style="font-size: 0.8rem; color: var(--text-dim); min-width: 70px;">
            Round ${f.roundNumber}
          </div>
          <div style="flex: 1; text-align: center; font-weight: 600; font-size: 0.95rem;">
            ${f.teamAName} <span style="color: var(--accent-gold); font-size: 0.75rem; margin: 0 0.5rem;">VS</span> ${f.teamBName}
          </div>
          <div style="min-width: 140px; text-align: right;">
            ${f.isPlayed ? `
              <span class="badge badge-success" style="font-size: 0.7rem;">${f.resultSummary || 'Completed'}</span>
            ` : `
              <span class="badge badge-warning" style="font-size: 0.7rem;">Scheduled</span>
            `}
          </div>
        </div>
      `;
    }).join('');

    return `
      <div class="main-viewport">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
          <div>
            <div class="badge badge-gold" style="margin-bottom: 0.4rem;">${tournament.trophyName}</div>
            <h2 style="font-family: var(--font-display); font-size: 2.75rem; letter-spacing: 0.05em; line-height: 1;">
              ${tournament.name.toUpperCase()}
            </h2>
            <div style="font-size: 0.85rem; color: var(--text-dim); margin-top: 0.35rem;">
              Format: ${tournament.format} • Status: ${tournament.status} • Completed: ${tournament.completedMatches} / ${tournament.totalMatches} matches
            </div>
          </div>

          <button id="btn-tourn-back-hub" class="btn btn-secondary">
            ← Career Hub
          </button>
        </div>

        <div class="grid-2">
          <!-- Standings Table -->
          <div class="card">
            <h3 style="font-size: 1rem; font-weight: 700; margin-bottom: 1rem; color: #fff; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
              STANDINGS & POINTS TABLE
            </h3>
            <table class="cricket-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Team</th>
                  <th>P</th>
                  <th>W</th>
                  <th>L</th>
                  <th>T</th>
                  <th>Pts</th>
                </tr>
              </thead>
              <tbody>
                ${standingsRows}
              </tbody>
            </table>
          </div>

          <!-- Fixtures & Results -->
          <div class="card">
            <h3 style="font-size: 1rem; font-weight: 700; margin-bottom: 1rem; color: #fff; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
              FIXTURES & SCHEDULE
            </h3>
            <div style="max-height: 440px; overflow-y: auto; padding-right: 0.5rem;">
              ${fixturesRows}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const backBtn = document.getElementById('btn-tourn-back-hub') || document.getElementById('btn-tourn-back');
    if (backBtn) {
      backBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      });
    }
  }
}
