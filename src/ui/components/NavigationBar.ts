import { GameStateManager, GameScreen } from '../../core/GameState';
import { PlayerData } from '../../player/PlayerModel';
import { AudioManager } from '../../audio/AudioManager';

export class NavigationBar {
  public static render(player: PlayerData | null, currentScreen: GameScreen): string {
    if (currentScreen === GameScreen.MAIN_MENU || currentScreen === GameScreen.PLAYER_CREATION) {
      return ''; // No top nav on intro/creation screens
    }

    const playerName = player ? `${player.firstName} ${player.lastName}` : 'Guest Cricketer';
    const rating = player ? player.potential.overallRating : 0;
    const level = player ? player.career.currentCareerLevel.replace('_', ' ') : '';

    return `
      <header class="app-nav">
        <div class="nav-brand" id="nav-brand-home">
          CRICKET <span>CAREER</span>
        </div>

        <nav class="nav-links">
          <button class="nav-btn ${currentScreen === GameScreen.CAREER_HUB ? 'active' : ''}" data-screen="${GameScreen.CAREER_HUB}">
            Career Hub
          </button>
          <button class="nav-btn ${currentScreen === GameScreen.TOURNAMENT ? 'active' : ''}" data-screen="${GameScreen.TOURNAMENT}">
            Tournament
          </button>
          <button class="nav-btn ${currentScreen === GameScreen.CAREER_PROGRESSION ? 'active' : ''}" data-screen="${GameScreen.CAREER_PROGRESSION}">
            Roadmap
          </button>
          <button class="nav-btn ${currentScreen === GameScreen.CALENDAR ? 'active' : ''}" data-screen="${GameScreen.CALENDAR}">
            Calendar
          </button>
          <button class="nav-btn ${currentScreen === GameScreen.MILESTONES ? 'active' : ''}" data-screen="${GameScreen.MILESTONES}">
            Milestones
          </button>
          <button class="nav-btn ${currentScreen === GameScreen.STATISTICS ? 'active' : ''}" data-screen="${GameScreen.STATISTICS}">
            Statistics
          </button>
          <button class="nav-btn ${currentScreen === GameScreen.SETTINGS ? 'active' : ''}" data-screen="${GameScreen.SETTINGS}">
            Settings
          </button>
        </nav>

        <div class="nav-actions">
          <div style="text-align: right;">
            <div style="font-weight: 700; font-size: 0.9rem;">${playerName}</div>
            <div style="font-size: 0.75rem; color: var(--text-muted);">${level}</div>
          </div>
          <div class="badge badge-gold" style="font-size: 0.85rem; padding: 0.35rem 0.65rem;">
            OVR ${rating}
          </div>
        </div>
      </header>
    `;
  }

  public static bindEvents(): void {
    const navButtons = document.querySelectorAll('.nav-btn');
    navButtons.forEach(btn => {
      btn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        const targetScreen = btn.getAttribute('data-screen') as GameScreen;
        if (targetScreen) {
          GameStateManager.getInstance().transitionTo(targetScreen);
        }
      });
    });

    const brand = document.getElementById('nav-brand-home');
    if (brand) {
      brand.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      });
    }
  }
}
