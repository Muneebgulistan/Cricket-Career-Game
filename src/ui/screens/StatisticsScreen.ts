import { PlayerData } from '../../player/PlayerModel';
import { CareerStatisticsContainer } from '../../player/CareerStatistics';
import { Formatters } from '../../utilities/Formatters';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { AudioManager } from '../../audio/AudioManager';

export class StatisticsScreen {
  private static activeTab: 'allTime' | 'byLevel' | 'byTournament' = 'allTime';

  public static render(player: PlayerData, stats: CareerStatisticsContainer): string {
    const allTimeBat = stats.allTime.batting;
    const allTimeBowl = stats.allTime.bowling;
    const allTimeField = stats.allTime.fielding;

    // Render by-level table rows
    const levelRows = Object.values(stats.byLevel).map(item => `
      <tr>
        <td style="font-weight: 700; color: #fff;">${item.level}</td>
        <td>${item.batting.matches}</td>
        <td>${item.batting.innings}</td>
        <td style="color: var(--accent-emerald); font-weight: 700;">${item.batting.runs}</td>
        <td>${item.batting.highestScore}${item.batting.isHighestScoreNotOut ? '*' : ''}</td>
        <td>${Formatters.formatAverage(item.batting.average)}</td>
        <td>${item.batting.fifties}</td>
        <td>${item.batting.hundreds}</td>
        <td style="color: var(--accent-cyan); font-weight: 700;">${item.bowling.wickets}</td>
        <td>${Formatters.formatEconomy(item.bowling.economy)}</td>
        <td>${item.fielding.catches}</td>
      </tr>
    `).join('');

    // Render by-tournament table rows
    const tourneyRows = stats.byTournament.map(t => `
      <tr>
        <td style="font-weight: 700; color: #fff;">${t.tournamentName}</td>
        <td>${t.level}</td>
        <td>${t.batting.matches}</td>
        <td style="color: var(--accent-emerald); font-weight: 700;">${t.batting.runs}</td>
        <td>${Formatters.formatAverage(t.batting.average)}</td>
        <td style="color: var(--accent-cyan); font-weight: 700;">${t.bowling.wickets}</td>
        <td>${Formatters.formatEconomy(t.bowling.economy)}</td>
        <td>${t.fielding.catches}</td>
      </tr>
    `).join('');

    return `
      <div class="main-viewport">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem;">
          <div>
            <div class="badge badge-gold" style="margin-bottom: 0.4rem;">OFFICIAL RECORD BOOK</div>
            <h2 style="font-family: var(--font-display); font-size: 3rem; letter-spacing: 0.05em; line-height: 1;">
              CAREER <span style="color: var(--accent-emerald);">STATISTICS</span>
            </h2>
            <div style="font-size: 0.85rem; color: var(--text-dim); margin-top: 0.35rem;">
              Player: ${player.firstName} ${player.lastName} (${player.role}) • All-time historical records
            </div>
          </div>

          <button id="btn-stats-back" class="btn btn-secondary">
            ← Career Hub
          </button>
        </div>

        <!-- All-Time Career Cards -->
        <div class="grid-3" style="margin-bottom: 2rem;">
          <!-- Batting Record -->
          <div class="card">
            <h3 style="font-size: 1rem; font-weight: 700; color: var(--accent-emerald); margin-bottom: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
              🏏 BATTING STATISTICS
            </h3>

            <div style="display: flex; flex-direction: column; gap: 0.65rem; font-size: 0.85rem;">
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Matches / Innings:</span>
                <strong>${allTimeBat.matches} / ${allTimeBat.innings}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Total Runs:</span>
                <strong style="color: var(--accent-emerald); font-size: 1.1rem;">${allTimeBat.runs}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Highest Score:</span>
                <strong>${allTimeBat.highestScore}${allTimeBat.isHighestScoreNotOut ? '*' : ''}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Batting Average:</span>
                <strong style="color: var(--accent-gold);">${Formatters.formatAverage(allTimeBat.average)}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Strike Rate:</span>
                <strong>${Formatters.formatStrikeRate(allTimeBat.strikeRate)}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Centuries / Fifties:</span>
                <strong>${allTimeBat.hundreds} / ${allTimeBat.fifties}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Boundaries (4s / 6s):</span>
                <strong>${allTimeBat.fours} / ${allTimeBat.sixes}</strong>
              </div>
            </div>
          </div>

          <!-- Bowling Record -->
          <div class="card">
            <h3 style="font-size: 1rem; font-weight: 700; color: var(--accent-cyan); margin-bottom: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
              ⚡ BOWLING STATISTICS
            </h3>

            <div style="display: flex; flex-direction: column; gap: 0.65rem; font-size: 0.85rem;">
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Overs Bowled:</span>
                <strong>${allTimeBowl.overs} (${allTimeBowl.maidens} maidens)</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Total Wickets:</span>
                <strong style="color: var(--accent-cyan); font-size: 1.1rem;">${allTimeBowl.wickets}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Runs Conceded:</span>
                <strong>${allTimeBowl.runsConceded}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Best Bowling:</span>
                <strong>${Formatters.formatBestBowling(allTimeBowl.bestBowlingWickets, allTimeBowl.bestBowlingRuns)}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Bowling Average:</span>
                <strong>${Formatters.formatAverage(allTimeBowl.average)}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Economy Rate:</span>
                <strong style="color: var(--accent-gold);">${Formatters.formatEconomy(allTimeBowl.economy)}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">3-for / 5-for Hauls:</span>
                <strong>${allTimeBowl.threeWicketHauls} / ${allTimeBowl.fiveWicketHauls}</strong>
              </div>
            </div>
          </div>

          <!-- Fielding Record -->
          <div class="card">
            <h3 style="font-size: 1rem; font-weight: 700; color: var(--accent-gold); margin-bottom: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
              🧤 FIELDING RECORD
            </h3>

            <div style="display: flex; flex-direction: column; gap: 0.65rem; font-size: 0.85rem;">
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Catches Taken:</span>
                <strong style="font-size: 1.1rem; color: #fff;">${allTimeField.catches}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Run Outs:</span>
                <strong>${allTimeField.runOuts}</strong>
              </div>
              <div style="display: flex; justify-content: space-between;">
                <span style="color: var(--text-dim);">Stumpings:</span>
                <strong>${allTimeField.stumpings}</strong>
              </div>
            </div>
          </div>
        </div>

        <!-- Breakdown by Career Level -->
        <div class="card" style="margin-bottom: 2rem;">
          <h3 style="font-size: 1rem; font-weight: 700; margin-bottom: 1rem; color: #fff; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
            BREAKDOWN BY CAREER LEVEL
          </h3>
          <table class="cricket-table">
            <thead>
              <tr>
                <th>Tier Level</th>
                <th>Mat</th>
                <th>Inn</th>
                <th>Runs</th>
                <th>HS</th>
                <th>Avg</th>
                <th>50</th>
                <th>100</th>
                <th>Wkts</th>
                <th>Econ</th>
                <th>Ct</th>
              </tr>
            </thead>
            <tbody>
              ${levelRows || '<tr><td colspan="11" style="text-align: center; color: var(--text-dim);">No matches recorded at tier levels yet.</td></tr>'}
            </tbody>
          </table>
        </div>

        <!-- Breakdown by Tournament -->
        <div class="card">
          <h3 style="font-size: 1rem; font-weight: 700; margin-bottom: 1rem; color: #fff; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
            TOURNAMENT-BY-TOURNAMENT HISTORY
          </h3>
          <table class="cricket-table">
            <thead>
              <tr>
                <th>Tournament</th>
                <th>Level</th>
                <th>Matches</th>
                <th>Runs</th>
                <th>Bat Avg</th>
                <th>Wickets</th>
                <th>Econ</th>
                <th>Catches</th>
              </tr>
            </thead>
            <tbody>
              ${tourneyRows || '<tr><td colspan="8" style="text-align: center; color: var(--text-dim);">No tournament records yet.</td></tr>'}
            </tbody>
          </table>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const backBtn = document.getElementById('btn-stats-back');
    if (backBtn) {
      backBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      });
    }
  }
}
