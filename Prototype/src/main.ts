import './assets/styles.css';
import { EventBus } from './core/EventBus';
import { GameStateManager, GameScreen } from './core/GameState';
import { PlayerData } from './player/PlayerModel';
import { PlayerFactory, PlayerCreationParams } from './player/PlayerFactory';
import { CareerLevel } from './career/CareerLevel';
import { CareerProgression } from './career/CareerProgression';
import { CareerStatisticsContainer, CareerStatisticsTracker } from './player/CareerStatistics';
import { TournamentInstance } from './tournament/TournamentModel';
import { TournamentManager } from './tournament/TournamentManager';
import { MatchInstance } from './match/MatchModel';
import { MatchSimulator } from './match/MatchSimulator';
import { InteractiveMatchEngine } from './match/InteractiveMatchEngine';
import { BattingShot, ShotDirection } from './match/BattingEngine';
import { BowlingDeliveryInput } from './match/BowlingEngine';
import { ThrowTarget } from './match/FieldingSystem';
import { SaveSystem } from './save/SaveSystem';
import { MatchHistoryEntry } from './save/SaveSchema';
import { WeeklySchedule, CareerCalendar } from './career/CareerCalendar';
import { FatigueState, FatigueSystem } from './career/FatigueSystem';
import { ActiveInjury, InjurySystem } from './career/InjurySystem';
import { TrainingCategory, TrainingSystem } from './career/TrainingSystem';
import { AchievementSystem, UnlockedAchievementRecord } from './career/AchievementSystem';
import { DifficultyLevel } from './career/Difficulty';
import { MatchPresentation, TossResult } from './match/MatchPresentation';
import { MatchEventManager } from './match/MatchEventManager';
import { MatchScreen } from './ui/screens/MatchScreen';
import { UIManager } from './ui/UIManager';
import { AudioManager } from './audio/AudioManager';
import { DismissalType } from './cricket/CricketTypes';
import { PerformanceEvaluator } from './career/PerformanceEvaluator';

class GameApp {
  private player: PlayerData | null = null;
  private tournament: TournamentInstance | null = null;
  private statistics: CareerStatisticsContainer = CareerStatisticsTracker.createInitialContainer();
  private currentMatch: MatchInstance | null = null;
  private matchHistory: MatchHistoryEntry[] = [];
  private recentRatings: number[] = [];
  private calendar: WeeklySchedule = CareerCalendar.createDefaultWeek();
  private fatigue: FatigueState = FatigueSystem.createInitialState();
  private activeInjury: ActiveInjury | null = null;
  private unlockedAchievements: UnlockedAchievementRecord[] = [];
  private difficulty: DifficultyLevel = DifficultyLevel.NORMAL;

  public start(): void {
    console.log('[GameApp] Starting Cricket Career Game (Step 3 Match Experience)...');

    const ui = UIManager.getInstance();
    ui.init('app');

    this.registerEventHandlers();

    // Check for existing autosave
    if (SaveSystem.saveExists('autosave')) {
      GameStateManager.getInstance().transitionTo(GameScreen.MAIN_MENU);
    } else {
      GameStateManager.getInstance().transitionTo(GameScreen.MAIN_MENU);
    }

    this.syncUI();
  }

  private syncUI(): void {
    UIManager.getInstance().updateContext({
      player: this.player,
      tournament: this.tournament,
      statistics: this.statistics,
      currentMatch: this.currentMatch,
      recentRatings: this.recentRatings,
      calendar: this.calendar,
      fatigue: this.fatigue,
      activeInjury: this.activeInjury,
      unlockedAchievements: this.unlockedAchievements
    });
  }

  private tryUnlock(id: string): void {
    if (!this.player) return;
    const unlocked = AchievementSystem.tryUnlock(
      this.unlockedAchievements,
      id,
      this.player.career.currentCareerLevel,
      this.tournament?.name
    );
    if (unlocked) {
      AudioManager.getInstance().playFanfare();
      UIManager.getInstance().showToast(`🏆 MILESTONE UNLOCKED: ${unlocked.name}!`, 'success');
    }
  }

  private registerEventHandlers(): void {
    const bus = EventBus.getInstance();

    // 1. Player Creation
    bus.on<PlayerCreationParams>('player:create', (params) => {
      console.log('[GameApp] Creating new Under-16 career for:', params);
      this.player = PlayerFactory.createUnder16Player(params);
      this.tournament = TournamentManager.createTournamentForLevel(this.player.career.currentCareerLevel, this.player.nationality);
      this.statistics = CareerStatisticsTracker.createInitialContainer();
      this.matchHistory = [];
      this.recentRatings = [];
      this.calendar = CareerCalendar.createDefaultWeek();
      this.fatigue = FatigueSystem.createInitialState();
      this.activeInjury = null;
      this.unlockedAchievements = [];

      this.saveCurrentState('autosave');
      this.syncUI();
      GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      UIManager.getInstance().showToast(`Welcome to your Under-16 career, ${this.player.firstName}!`, 'success');
    });

    // 2. Load Game
    bus.on<string>('menu:load-game', (slotId) => {
      const data = SaveSystem.loadGame(slotId);
      if (data) {
        this.player = data.player;
        this.tournament = data.tournament;
        this.statistics = data.statistics;
        this.matchHistory = data.matchHistory || [];
        this.recentRatings = data.recentRatings || [];
        this.calendar = data.calendar || CareerCalendar.createDefaultWeek();
        this.fatigue = data.fatigue || FatigueSystem.createInitialState();
        this.activeInjury = data.activeInjury || null;
        this.unlockedAchievements = data.unlockedAchievements || [];
        this.difficulty = data.settings?.difficulty || DifficultyLevel.NORMAL;

        this.syncUI();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
        UIManager.getInstance().showToast(`Loaded career: ${data.saveName}`, 'success');
      } else {
        UIManager.getInstance().showToast(`Failed to load slot: ${slotId}`, 'warning');
      }
    });

    // 3. Manual Save
    bus.on<string | undefined>('game:manual-save', (slotId = 'autosave') => {
      if (this.saveCurrentState(slotId)) {
        UIManager.getInstance().showToast(`Career saved to slot "${slotId}"!`, 'success');
      }
    });

    // 4. Match Prep Refresh
    bus.on('match:prep-refresh', () => {
      this.syncUI();
    });

    // 5. Start Match from Prep -> Initialize Interactive Match Engine
    bus.on<{ tossResult?: TossResult } | undefined>('match:start', (payload) => {
      if (!this.player || !this.tournament) return;
      const fixture = TournamentManager.getNextFixture(this.tournament);
      if (!fixture) return;

      this.currentMatch = MatchSimulator.initializeMatch(this.player, this.tournament, fixture);
      this.currentMatch.conditions = MatchPresentation.generateMatchConditions(this.currentMatch.venue);

      if (payload?.tossResult) {
        this.currentMatch.tossResult = payload.tossResult;
      }

      MatchEventManager.resetMatchEvents();
      InteractiveMatchEngine.initInteractiveMatch(this.currentMatch, this.player);

      this.tryUnlock('first_match');
      if (this.player.career.currentCareerLevel === CareerLevel.T20_WORLD_CUP || this.player.career.currentCareerLevel === CareerLevel.ODI_WORLD_CUP) {
        this.tryUnlock('first_world_cup_match');
      }

      // Safe Save point: before match
      this.saveCurrentState('autosave');

      this.syncUI();
      GameStateManager.getInstance().transitionTo(GameScreen.MATCH);
    });

    // 6. UI refresh in match
    bus.on('match:ui-refresh', () => {
      this.syncUI();
    });

    // 7. Interactive User Batting Shot
    bus.on<{ shot: BattingShot; direction?: ShotDirection; timingOffsetMs: number }>('match:interactive-bat-shot', ({ shot, direction = ShotDirection.CENTER, timingOffsetMs }) => {
      if (!this.currentMatch || !this.player) return;

      const result = InteractiveMatchEngine.executeUserShot(
        this.currentMatch,
        this.player,
        shot,
        timingOffsetMs,
        this.difficulty,
        this.fatigue.fatigueLevel,
        direction
      );

      // Trigger Canvas visual animation
      const fieldView = MatchScreen.getFieldView();
      if (fieldView) {
        if (result.isWicket) {
          fieldView.triggerWicketAnimation(result.dismissalType || DismissalType.BOWLED);
          AudioManager.getInstance().playWicket();
        } else {
          fieldView.triggerShotAnimation(shot, direction, result.runs);
          if (result.isBoundarySix) AudioManager.getInstance().playCheer(true);
          else if (result.isBoundaryFour) AudioManager.getInstance().playCheer(false);
        }
      }

      // Check batting achievements
      const pPerf = this.currentMatch.playerPerformance;
      if (pPerf.runs > 0) this.tryUnlock('first_run');
      if (pPerf.fours > 0 || pPerf.sixes > 0) this.tryUnlock('first_boundary');
      if (pPerf.runs >= 50) this.tryUnlock('first_50');
      if (pPerf.runs >= 100) this.tryUnlock('first_100');

      InteractiveMatchEngine.determineCurrentPhase(this.currentMatch, this.player);
      this.syncUI();
    });

    // 8. Running Decision
    bus.on<boolean>('match:running-decision', (playerChoseRun) => {
      if (!this.currentMatch || !this.player) return;

      const outcome = InteractiveMatchEngine.resolveRunningChoice(
        this.currentMatch,
        this.player,
        playerChoseRun,
        this.fatigue.fatigueLevel
      );

      if (outcome.isRunOut) {
        AudioManager.getInstance().playWicket();
      }

      InteractiveMatchEngine.determineCurrentPhase(this.currentMatch, this.player);
      this.syncUI();
    });

    // 9. Interactive User Bowling Delivery
    bus.on<BowlingDeliveryInput>('match:interactive-bowl-delivery', (input) => {
      if (!this.currentMatch || !this.player) return;

      const result = InteractiveMatchEngine.executeUserBowling(
        this.currentMatch,
        this.player,
        input,
        this.currentMatch.conditions?.pitchType || 'BALANCED',
        this.fatigue.fatigueLevel
      );

      // Trigger field view
      const fieldView = MatchScreen.getFieldView();
      if (fieldView) {
        if (result.isWicket) {
          fieldView.triggerWicketAnimation(result.dismissalType || DismissalType.BOWLED);
          AudioManager.getInstance().playWicket();
        }
      }

      const pPerf = this.currentMatch.playerPerformance;
      if (pPerf.wickets >= 5) this.tryUnlock('first_5_wickets');

      InteractiveMatchEngine.determineCurrentPhase(this.currentMatch, this.player);
      this.syncUI();
    });

    // 10. Interactive User Fielding Reaction
    bus.on<{ reactionTimeMs: number; selectedTarget?: ThrowTarget }>('match:interactive-field-react', ({ reactionTimeMs, selectedTarget = 'DIRECT_HIT' }) => {
      if (!this.currentMatch || !this.player) return;

      const outcome = InteractiveMatchEngine.executeUserFielding(this.currentMatch, this.player, reactionTimeMs, selectedTarget);
      if (outcome.isCatch) {
        this.tryUnlock('first_catch');
        AudioManager.getInstance().playCatch();
      }

      InteractiveMatchEngine.determineCurrentPhase(this.currentMatch, this.player);
      this.syncUI();
    });

    // 11. Simulate to User's Next Turn
    bus.on('match:sim-to-my-turn', () => {
      if (!this.currentMatch || !this.player) return;

      InteractiveMatchEngine.advanceToNextUserMoment(this.currentMatch, this.player);
      this.syncUI();
    });

    // 12. Start Second Innings
    bus.on('match:start-second-innings', () => {
      if (!this.currentMatch || !this.player) return;

      InteractiveMatchEngine.transitionToSecondInnings(this.currentMatch, this.player);
      InteractiveMatchEngine.determineCurrentPhase(this.currentMatch, this.player);

      // Safe save point: innings break
      this.saveCurrentState('autosave');

      this.syncUI();
    });

    // 13. View Results from interactive match end
    bus.on('match:view-results', () => {
      if (!this.currentMatch || !this.player) return;

      this.finalizeMatchEvaluation();
      GameStateManager.getInstance().transitionTo(GameScreen.MATCH_RESULT);
    });

    // 14. Fast Forward Full Match
    bus.on('match:fast-forward', () => {
      if (!this.currentMatch || !this.player || !this.tournament) return;

      MatchSimulator.simulateFullMatch(this.currentMatch, this.player);
      this.finalizeMatchEvaluation();
      GameStateManager.getInstance().transitionTo(GameScreen.MATCH_RESULT);
    });

    // 15. Restart Match
    bus.on('match:restart', () => {
      if (!this.currentMatch || !this.player || !this.tournament) return;
      const fixture = TournamentManager.getNextFixture(this.tournament);
      if (!fixture) return;

      this.currentMatch = MatchSimulator.initializeMatch(this.player, this.tournament, fixture);
      InteractiveMatchEngine.initInteractiveMatch(this.currentMatch, this.player);
      this.syncUI();
      UIManager.getInstance().showToast('Match restarted from ball 1.', 'info');
    });

    // 16. Abandon Match
    bus.on('match:abandon', () => {
      if (!this.currentMatch || !this.player) return;
      this.currentMatch = null;
      this.saveCurrentState('autosave');
      this.syncUI();
      GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
      UIManager.getInstance().showToast('Match abandoned. Returned safely to Career Hub.', 'warning');
    });

    // 17. Conclude Match Result -> return to Hub
    bus.on('match:conclude', () => {
      this.currentMatch = null;
      this.saveCurrentState('autosave');
      this.syncUI();
      GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
    });

    // 18. Claim Promotion
    bus.on('career:claim-promotion', () => {
      if (!this.player) return;
      const oldLevel = this.player.career.currentCareerLevel;
      const success = CareerProgression.promote(this.player);
      if (success) {
        this.tournament = TournamentManager.createTournamentForLevel(this.player.career.currentCareerLevel, this.player.nationality);
        this.player.career.currentTeam = `${this.player.nationality} ${this.player.career.currentCareerLevel.replace('_', ' ')}`;
        this.player.career.currentTournament = this.tournament.name;

        this.tryUnlock('first_promotion');
        if (this.player.career.currentCareerLevel === CareerLevel.INTERNATIONAL_HOME) {
          this.tryUnlock('international_debut');
        }

        this.saveCurrentState('autosave');
        this.syncUI();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_HUB);
        UIManager.getInstance().showToast(`PROMOTED! You advance from ${oldLevel} to ${this.player.career.currentCareerLevel}!`, 'success');
      }
    });

    // 19. Advance Calendar Day
    bus.on('calendar:advance-day', () => {
      const adv = CareerCalendar.advanceDay(this.calendar);

      if (this.activeInjury) {
        const healed = InjurySystem.advanceHealing(this.activeInjury, 1);
        if (healed) {
          this.activeInjury = null;
          UIManager.getInstance().showToast('Medical clearance received! You have fully recovered from injury.', 'success');
        }
      }

      this.saveCurrentState('autosave');
      this.syncUI();
      UIManager.getInstance().showToast(`Advanced calendar: Now ${adv.currentDay} (Week ${this.calendar.weekNumber})`, 'info');
    });

    // 20. Training Drills
    bus.on<TrainingCategory>('player:drill-train', (category) => {
      if (!this.player) return;

      const result = TrainingSystem.executeDrill(this.player, category);
      FatigueSystem.applyTrainingLoad(this.fatigue, 'MEDIUM');

      if (!this.activeInjury) {
        const injury = InjurySystem.checkForInjury(this.fatigue.fatigueLevel, this.player.mental.fitness);
        if (injury) {
          this.activeInjury = injury;
          UIManager.getInstance().showToast(`⚠️ INJURY: Sustained ${injury.name}! Out for ${injury.daysRemaining} days.`, 'warning');
        }
      }

      this.saveCurrentState('autosave');
      this.syncUI();
      UIManager.getInstance().showToast(result.message, 'success');
    });

    // 21. Physio Recovery
    bus.on('player:physio-recovery', () => {
      if (!this.player) return;

      FatigueSystem.applyRestDay(this.fatigue, true);
      this.player.mental.fitness = Math.min(100, this.player.mental.fitness + 15);
      this.player.mental.confidence = Math.min(100, this.player.mental.confidence + 5);

      if (this.activeInjury) {
        const healed = InjurySystem.advanceHealing(this.activeInjury, 2, true);
        if (healed) {
          this.activeInjury = null;
          UIManager.getInstance().showToast('Physio clearance! Completely healed from injury.', 'success');
        }
      }

      this.saveCurrentState('autosave');
      this.syncUI();
      UIManager.getInstance().showToast('Physio recovery completed. Fatigue cleared and stamina restored!', 'success');
    });

    // 22. Debug Hooks
    this.registerDebugListeners();
  }

  private finalizeMatchEvaluation(): void {
    if (!this.currentMatch || !this.player || !this.tournament) return;

    const perf = this.currentMatch.playerPerformance;
    const inn1 = this.currentMatch.innings[0];

    const playerTeamId = this.currentMatch.teamA.name.includes(this.player.nationality) || inn1.battingTeamName.includes(this.player.nationality)
      ? this.currentMatch.teamA.id
      : this.currentMatch.teamB.id;

    const playerWon = (this.currentMatch.winnerTeamId === playerTeamId);
    perf.isPlayerTeamWinner = playerWon;

    const isMotm = (perf.runs >= 50 || perf.wickets >= 4);
    if (isMotm) {
      this.tryUnlock('first_motm');
      this.currentMatch.manOfTheMatchPlayerName = `${this.player.firstName} ${this.player.lastName}`;
    }

    const evaluation = PerformanceEvaluator.evaluateMatch(this.player, {
      runsScored: perf.runs,
      ballsFaced: perf.balls,
      fours: perf.fours,
      sixes: perf.sixes,
      isOut: perf.isOut,
      oversBowled: perf.overs,
      maidens: perf.maidens,
      runsConceded: perf.runsConceded,
      wicketsTaken: perf.wickets,
      catches: perf.catches,
      runOuts: perf.runOuts,
      stumpings: perf.stumpings,
      matchResult: playerWon ? 'WIN' : 'LOSS',
      isManOfTheMatch: isMotm
    });

    perf.evaluation = evaluation;

    // Apply mental and fitness updates
    this.player.mental.form = Math.max(1, Math.min(99, this.player.mental.form + evaluation.formDelta));
    this.player.mental.confidence = Math.max(1, Math.min(99, this.player.mental.confidence + evaluation.confidenceDelta));
    this.player.mental.fitness = Math.max(10, Math.min(100, this.player.mental.fitness + evaluation.fitnessDelta));

    // Apply fatigue & injury check
    FatigueSystem.applyMatchWorkload(this.fatigue, perf.balls, perf.overs);
    if (!this.activeInjury) {
      const inj = InjurySystem.checkForInjury(this.fatigue.fatigueLevel, this.player.mental.fitness, true);
      if (inj) {
        this.activeInjury = inj;
        UIManager.getInstance().showToast(`⚠️ INJURY: Picked up ${inj.name}! Out for ${inj.daysRemaining} days.`, 'warning');
      }
    }

    // Record Statistics
    CareerStatisticsTracker.recordMatch(
      this.statistics,
      this.player.career.currentCareerLevel,
      this.tournament.id,
      this.tournament.name,
      {
        runs: perf.runs,
        balls: perf.balls,
        fours: perf.fours,
        sixes: perf.sixes,
        isOut: perf.isOut,
        didBat: perf.didBat,
        overs: perf.overs,
        maidens: perf.maidens,
        runsConceded: perf.runsConceded,
        wickets: perf.wickets,
        didBowl: perf.didBowl,
        catches: perf.catches,
        runOuts: perf.runOuts,
        stumpings: perf.stumpings
      }
    );

    // Record Tournament Result
    TournamentManager.recordMatchResult(
      this.tournament,
      this.currentMatch.fixtureId,
      this.currentMatch.winnerTeamId || '',
      this.currentMatch.resultSummary || 'Completed'
    );

    if (this.tournament.status === 'COMPLETED' && this.tournament.championTeamId === this.tournament.playerTeamId) {
      this.tryUnlock('first_tournament_win');
      if (this.player.career.currentCareerLevel === CareerLevel.ODI_WORLD_CUP) {
        this.tryUnlock('world_cup_winner');
      }
    }

    // Save match history
    this.recentRatings.push(evaluation.matchRating);
    this.matchHistory.push({
      matchId: this.currentMatch.id,
      date: this.currentMatch.date,
      tournamentName: this.tournament.name,
      opponentName: this.currentMatch.teamB.name,
      resultSummary: this.currentMatch.resultSummary || '',
      playerRuns: perf.runs,
      playerWickets: perf.wickets,
      matchRating: evaluation.matchRating
    });

    // Update Player Career Summary
    this.player.career.matchesPlayed = this.statistics.allTime.batting.matches;
    this.player.career.runs = this.statistics.allTime.batting.runs;
    this.player.career.wickets = this.statistics.allTime.bowling.wickets;
    this.player.career.catches = this.statistics.allTime.fielding.catches;
    this.player.career.average = this.statistics.allTime.batting.average;
    this.player.career.strikeRate = this.statistics.allTime.batting.strikeRate;
    this.player.career.economy = this.statistics.allTime.bowling.economy;

    // Safe Save point: after match and career update
    this.saveCurrentState('autosave');
    this.syncUI();
  }

  private registerDebugListeners(): void {
    const bus = EventBus.getInstance();

    bus.on<number>('debug:modify-form', (delta) => {
      if (!this.player) return;
      this.player.mental.form = Math.max(1, Math.min(99, this.player.mental.form + delta));
      this.syncUI();
    });

    bus.on<number>('debug:modify-conf', (delta) => {
      if (!this.player) return;
      this.player.mental.confidence = Math.max(1, Math.min(99, this.player.mental.confidence + delta));
      this.syncUI();
    });

    bus.on('debug:force-four', () => {
      if (!this.currentMatch || !this.player) return;
      const innings = this.currentMatch.innings[this.currentMatch.currentInningsIndex];
      innings.totalRuns += 4;
      this.currentMatch.playerPerformance.runs += 4;
      this.currentMatch.playerPerformance.fours += 1;
      this.currentMatch.commentaryLog.push('[DEBUG] FOUR forced! Ball cracked to the boundary!');
      this.syncUI();
      AudioManager.getInstance().playCheer(false);
    });

    bus.on('debug:force-six', () => {
      if (!this.currentMatch || !this.player) return;
      const innings = this.currentMatch.innings[this.currentMatch.currentInningsIndex];
      innings.totalRuns += 6;
      this.currentMatch.playerPerformance.runs += 6;
      this.currentMatch.playerPerformance.sixes += 1;
      this.currentMatch.commentaryLog.push('[DEBUG] SIX forced! Launched into the top tier!');
      this.syncUI();
      AudioManager.getInstance().playCheer(true);
    });

    bus.on('debug:force-wkt', () => {
      if (!this.currentMatch || !this.player) return;
      const innings = this.currentMatch.innings[this.currentMatch.currentInningsIndex];
      innings.totalWickets += 1;
      this.currentMatch.commentaryLog.push('[DEBUG] WICKET forced! Timber sent cartwheeling!');
      this.syncUI();
      AudioManager.getInstance().playWicket();
    });

    bus.on('debug:force-catch', () => {
      if (!this.currentMatch || !this.player) return;
      this.currentMatch.interactiveState!.phase = 'USER_FIELDING';
      this.currentMatch.interactiveState!.pendingFieldingPrompt = {
        id: `debug_catch_${Date.now()}`,
        type: 'CATCH_OPPORTUNITY' as any,
        title: '🔥 FORCED CATCH OPPORTUNITY',
        description: 'Ball sliced straight down your throat! React now!',
        batsmanName: 'AI Batter',
        reactionWindowMs: 1200,
        difficulty: DifficultyLevel.NORMAL
      };
      this.syncUI();
    });

    bus.on('debug:skip-over', () => {
      if (!this.currentMatch) return;
      const state = this.currentMatch.interactiveState;
      if (state) {
        state.currentOver++;
        state.currentBallInOver = 1;
        state.runsInCurrentOver = 0;
        this.syncUI();
      }
    });

    bus.on('debug:complete-inn', () => {
      if (!this.currentMatch || !this.player) return;
      this.currentMatch.innings[this.currentMatch.currentInningsIndex].isCompleted = true;
      if (this.currentMatch.currentInningsIndex === 0) {
        InteractiveMatchEngine.transitionToSecondInnings(this.currentMatch, this.player);
      } else {
        InteractiveMatchEngine.concludeMatch(this.currentMatch);
      }
      this.syncUI();
    });

    bus.on<number>('debug:add-runs', (runs) => {
      if (!this.player || !this.tournament) return;
      this.player.career.runs += runs;
      this.player.career.matchesPlayed++;
      this.syncUI();
      UIManager.getInstance().showToast(`Debug: Added ${runs} runs.`, 'info');
    });

    bus.on<number>('debug:add-wkts', (wkts) => {
      if (!this.player || !this.tournament) return;
      this.player.career.wickets += wkts;
      this.player.career.matchesPlayed++;
      this.syncUI();
      UIManager.getInstance().showToast(`Debug: Added ${wkts} wickets.`, 'info');
    });

    bus.on('debug:force-promote', () => {
      if (!this.player) return;
      const prev = this.player.career.currentCareerLevel;
      CareerProgression.promote(this.player);
      this.tournament = TournamentManager.createTournamentForLevel(this.player.career.currentCareerLevel, this.player.nationality);
      this.syncUI();
      UIManager.getInstance().showToast(`Debug: Force promoted ${prev} -> ${this.player.career.currentCareerLevel}`, 'success');
    });

    bus.on('debug:force-demote', () => {
      if (!this.player) return;
      CareerProgression.demote(this.player);
      this.syncUI();
      UIManager.getInstance().showToast(`Debug: Player status set to DROPPED`, 'warning');
    });
  }

  private saveCurrentState(slotId: string): boolean {
    if (!this.player) return false;
    return SaveSystem.saveGame(slotId, {
      player: this.player,
      tournament: this.tournament,
      statistics: this.statistics,
      matchHistory: this.matchHistory,
      recentRatings: this.recentRatings,
      calendar: this.calendar,
      fatigue: this.fatigue,
      activeInjury: this.activeInjury,
      unlockedAchievements: this.unlockedAchievements
    });
  }
}

// Bootstrap application on DOM load
window.addEventListener('DOMContentLoaded', () => {
  const app = new GameApp();
  app.start();
});
