import { EventBus } from './EventBus';

export enum GameScreen {
  MAIN_MENU = 'MAIN_MENU',
  PLAYER_CREATION = 'PLAYER_CREATION',
  CAREER_HUB = 'CAREER_HUB',
  TOURNAMENT = 'TOURNAMENT',
  MATCH_PREPARATION = 'MATCH_PREPARATION',
  MATCH = 'MATCH',
  MATCH_RESULT = 'MATCH_RESULT',
  CAREER_PROGRESSION = 'CAREER_PROGRESSION',
  STATISTICS = 'STATISTICS',
  CALENDAR = 'CALENDAR',
  MILESTONES = 'MILESTONES',
  SETTINGS = 'SETTINGS'
}

export class GameStateManager {
  private static instance: GameStateManager;
  private currentScreen: GameScreen = GameScreen.MAIN_MENU;
  private previousScreen: GameScreen | null = null;
  private screenData: Record<string, any> = {};

  private constructor() {}

  public static getInstance(): GameStateManager {
    if (!GameStateManager.instance) {
      GameStateManager.instance = new GameStateManager();
    }
    return GameStateManager.instance;
  }

  public getCurrentScreen(): GameScreen {
    return this.currentScreen;
  }

  public getPreviousScreen(): GameScreen | null {
    return this.previousScreen;
  }

  public getScreenData<T = any>(): T | undefined {
    return this.screenData as T;
  }

  public transitionTo(screen: GameScreen, data: Record<string, any> = {}): void {
    const from = this.currentScreen;
    this.previousScreen = from;
    this.currentScreen = screen;
    this.screenData = data;

    console.log(`[GameState] Transition: ${from} -> ${screen}`, data);
    EventBus.getInstance().emit('game:screen-changed', {
      from,
      to: screen,
      data
    });
  }

  public goBack(): void {
    if (this.previousScreen) {
      this.transitionTo(this.previousScreen);
    } else {
      this.transitionTo(GameScreen.CAREER_HUB);
    }
  }
}
