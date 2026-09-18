import { GameStateManager, GameScreen } from '../core/GameState';
import { EventBus } from '../core/EventBus';
import { PlayerData } from '../player/PlayerModel';
import { TournamentInstance } from '../tournament/TournamentModel';
import { CareerStatisticsContainer } from '../player/CareerStatistics';
import { MatchInstance } from '../match/MatchModel';
import { WeeklySchedule } from '../career/CareerCalendar';
import { FatigueState } from '../career/FatigueSystem';
import { ActiveInjury } from '../career/InjurySystem';
import { UnlockedAchievementRecord } from '../career/AchievementSystem';

import { NavigationBar } from './components/NavigationBar';
import { DebugPanel } from './components/DebugPanel';
import { MainMenuScreen } from './screens/MainMenuScreen';
import { PlayerCreationScreen } from './screens/PlayerCreationScreen';
import { CareerHubScreen } from './screens/CareerHubScreen';
import { TournamentScreen } from './screens/TournamentScreen';
import { MatchPrepScreen } from './screens/MatchPrepScreen';
import { MatchScreen } from './screens/MatchScreen';
import { MatchResultScreen } from './screens/MatchResultScreen';
import { CareerProgressionScreen } from './screens/CareerProgressionScreen';
import { StatisticsScreen } from './screens/StatisticsScreen';
import { SettingsScreen } from './screens/SettingsScreen';
import { CalendarScreen } from './screens/CalendarScreen';
import { MilestonesScreen } from './screens/MilestonesScreen';

export class UIManager {
  private static instance: UIManager;
  private container: HTMLElement | null = null;

  private player: PlayerData | null = null;
  private tournament: TournamentInstance | null = null;
  private statistics: CareerStatisticsContainer | null = null;
  private currentMatch: MatchInstance | null = null;
  private recentRatings: number[] = [];
  private calendar: WeeklySchedule | null = null;
  private fatigue: FatigueState | null = null;
  private activeInjury: ActiveInjury | null = null;
  private unlockedAchievements: UnlockedAchievementRecord[] = [];

  private constructor() {}

  public static getInstance(): UIManager {
    if (!UIManager.instance) {
      UIManager.instance = new UIManager();
    }
    return UIManager.instance;
  }

  public init(containerId: string = 'app'): void {
    this.container = document.getElementById(containerId);
    if (!this.container) {
      console.error(`[UIManager] Target container #${containerId} not found.`);
      return;
    }

    // Subscribe to screen changes
    EventBus.getInstance().on('game:screen-changed', () => {
      this.render();
    });

    this.render();
  }

  public updateContext(data: {
    player?: PlayerData | null;
    tournament?: TournamentInstance | null;
    statistics?: CareerStatisticsContainer | null;
    currentMatch?: MatchInstance | null;
    recentRatings?: number[];
    calendar?: WeeklySchedule;
    fatigue?: FatigueState;
    activeInjury?: ActiveInjury | null;
    unlockedAchievements?: UnlockedAchievementRecord[];
  }): void {
    if (data.player !== undefined) this.player = data.player;
    if (data.tournament !== undefined) this.tournament = data.tournament;
    if (data.statistics !== undefined) this.statistics = data.statistics;
    if (data.currentMatch !== undefined) this.currentMatch = data.currentMatch;
    if (data.recentRatings !== undefined) this.recentRatings = data.recentRatings;
    if (data.calendar !== undefined) this.calendar = data.calendar;
    if (data.fatigue !== undefined) this.fatigue = data.fatigue;
    if (data.activeInjury !== undefined) this.activeInjury = data.activeInjury;
    if (data.unlockedAchievements !== undefined) this.unlockedAchievements = data.unlockedAchievements;
    this.render();
  }

  public render(): void {
    if (!this.container) return;

    const currentScreen = GameStateManager.getInstance().getCurrentScreen();
    const navHtml = NavigationBar.render(this.player, currentScreen);
    let screenHtml = '';

    switch (currentScreen) {
      case GameScreen.MAIN_MENU:
        screenHtml = MainMenuScreen.render();
        break;
      case GameScreen.PLAYER_CREATION:
        screenHtml = PlayerCreationScreen.render();
        break;
      case GameScreen.CAREER_HUB:
        if (this.player && this.statistics) {
          screenHtml = CareerHubScreen.render(
            this.player,
            this.tournament,
            this.statistics,
            this.fatigue ?? undefined,
            this.activeInjury,
            this.calendar ?? undefined,
            this.unlockedAchievements.length
          );
        } else {
          screenHtml = MainMenuScreen.render();
        }
        break;
      case GameScreen.TOURNAMENT:
        if (this.player) {
          screenHtml = TournamentScreen.render(this.tournament, this.player);
        }
        break;
      case GameScreen.MATCH_PREPARATION:
        if (this.player) {
          screenHtml = MatchPrepScreen.render(this.tournament, this.player, this.recentRatings);
        }
        break;
      case GameScreen.MATCH:
        if (this.player) {
          screenHtml = MatchScreen.render(this.currentMatch, this.player);
        }
        break;
      case GameScreen.MATCH_RESULT:
        if (this.player) {
          screenHtml = MatchResultScreen.render(this.currentMatch, this.player);
        }
        break;
      case GameScreen.CAREER_PROGRESSION:
        if (this.player && this.statistics) {
          screenHtml = CareerProgressionScreen.render(this.player, this.statistics);
        }
        break;
      case GameScreen.STATISTICS:
        if (this.player && this.statistics) {
          screenHtml = StatisticsScreen.render(this.player, this.statistics);
        }
        break;
      case GameScreen.SETTINGS:
        screenHtml = SettingsScreen.render();
        break;
      case GameScreen.CALENDAR:
        if (this.calendar && this.fatigue && this.player) {
          screenHtml = CalendarScreen.render(this.calendar, this.fatigue, this.activeInjury, this.player);
        } else if (this.player && this.statistics) {
          screenHtml = CareerHubScreen.render(this.player, this.tournament, this.statistics);
        }
        break;
      case GameScreen.MILESTONES:
        if (this.player) {
          screenHtml = MilestonesScreen.render(this.unlockedAchievements, this.player);
        }
        break;
    }

    const debugHtml = DebugPanel.render();

    this.container.innerHTML = `
      ${navHtml}
      <main id="screen-root">
        ${screenHtml}
      </main>
      ${debugHtml}
    `;

    // Bind event handlers for screen & components
    NavigationBar.bindEvents();
    DebugPanel.bindEvents();

    switch (currentScreen) {
      case GameScreen.MAIN_MENU:
        MainMenuScreen.bindEvents();
        break;
      case GameScreen.PLAYER_CREATION:
        PlayerCreationScreen.bindEvents();
        break;
      case GameScreen.CAREER_HUB:
        CareerHubScreen.bindEvents();
        break;
      case GameScreen.TOURNAMENT:
        TournamentScreen.bindEvents();
        break;
      case GameScreen.MATCH_PREPARATION:
        MatchPrepScreen.bindEvents();
        break;
      case GameScreen.MATCH:
        MatchScreen.bindEvents();
        break;
      case GameScreen.MATCH_RESULT:
        MatchResultScreen.bindEvents();
        break;
      case GameScreen.CAREER_PROGRESSION:
        CareerProgressionScreen.bindEvents();
        break;
      case GameScreen.STATISTICS:
        StatisticsScreen.bindEvents();
        break;
      case GameScreen.SETTINGS:
        SettingsScreen.bindEvents();
        break;
      case GameScreen.CALENDAR:
        CalendarScreen.bindEvents();
        break;
      case GameScreen.MILESTONES:
        MilestonesScreen.bindEvents();
        break;
    }
  }

  public showToast(message: string, type: 'success' | 'warning' | 'info' = 'info'): void {
    const toast = document.createElement('div');
    toast.className = `badge badge-${type === 'success' ? 'success' : (type === 'warning' ? 'warning' : 'primary')}`;
    toast.style.position = 'fixed';
    toast.style.top = '1.5rem';
    toast.style.right = '1.5rem';
    toast.style.padding = '0.75rem 1.25rem';
    toast.style.fontSize = '0.9rem';
    toast.style.zIndex = '9999';
    toast.style.boxShadow = '0 10px 30px rgba(0,0,0,0.5)';
    toast.innerText = message;
    document.body.appendChild(toast);

    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transition = 'opacity 0.5s ease';
      setTimeout(() => toast.remove(), 500);
    }, 2500);
  }
}
