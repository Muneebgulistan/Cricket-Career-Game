import { MatchInstance, InningsScorecard, InteractiveMatchState, InteractivePhase } from './MatchModel';
import { PlayerData, PlayerRole, BowlingStyle } from '../player/PlayerModel';
import { BattingShot, BattingEngine, DeliveryLine, DeliveryLength, BattingExecutionResult, ShotDirection } from './BattingEngine';
import { BowlingEngine, BowlingDeliveryInput, BowlingDeliveryResult, BowlingVariation } from './BowlingEngine';
import { TimingSystem, TimingEvaluation } from './TimingSystem';
import { FieldingSystem, FieldingEventOutcome, ThrowTarget } from './FieldingSystem';
import { AIEngine, AIAggression } from './AIEngine';
import { DifficultyLevel } from '../career/Difficulty';
import { BallPhysics, DeliveryTrajectory, PitchCondition } from './BallPhysics';
import { RunningSystem, RunningOpportunity, RunningOutcome } from './RunningSystem';
import { MatchEventManager } from './MatchEventManager';
import { MatchPresentation } from './MatchPresentation';
import { DismissalType } from '../cricket/CricketTypes';

export class InteractiveMatchEngine {
  /**
   * Initializes the interactive match state for ball-by-ball stepping.
   */
  public static initInteractiveMatch(match: MatchInstance, player: PlayerData): void {
    const inningsIdx = match.currentInningsIndex;
    const currentInnings = match.innings[inningsIdx];

    // Generate conditions if missing
    if (!match.conditions) {
      match.conditions = MatchPresentation.generateMatchConditions(match.venue);
    }

    const isUserBattingTeam = currentInnings.battingTeamId === player.career.currentTeam ||
      currentInnings.battingTeamName.includes(player.nationality);
    const isUserBowlingTeam = !isUserBattingTeam;

    // Generate Lineups with AI traits
    const battingLineup = this.buildBattingLineup(currentInnings.battingTeamName, isUserBattingTeam, player);
    const bowlingAttack = this.buildBowlingAttack(currentInnings.bowlingTeamName, isUserBowlingTeam, player);

    currentInnings.batsmen = battingLineup.map(p => ({
      playerId: p.id,
      name: p.name,
      runs: 0,
      balls: 0,
      fours: 0,
      sixes: 0,
      isOut: false,
      dismissalText: 'Not Out'
    }));

    currentInnings.bowlers = bowlingAttack.map(b => ({
      playerId: b.id,
      name: b.name,
      overs: 0,
      maidens: 0,
      runs: 0,
      wickets: 0,
      economy: 0.0
    }));

    const isPowerplay = match.oversPerSide >= 20 ? 1 <= 6 : false;

    const state: InteractiveMatchState = {
      phase: 'AI_INNINGS',
      strikerIdx: 0,
      nonStrikerIdx: 1,
      nextBatsmanIdx: 2,
      bowlerIdx: 0,
      currentOver: 1,
      currentBallInOver: 1,
      runsInCurrentOver: 0,
      battingLineup,
      bowlingAttack,
      isUserBattingTeam,
      isUserBowlingTeam,
      currentFieldPreset: 'BALANCED',
      isPowerplay,
      partnershipRuns: 0,
      partnershipBalls: 0,
      recentBallsTimeline: []
    };

    match.interactiveState = state;

    // Check starting phase
    this.determineCurrentPhase(match, player);
  }

  /**
   * Checks whether the current delivery requires user interaction (batting, bowling, fielding).
   */
  public static determineCurrentPhase(match: MatchInstance, player: PlayerData): InteractivePhase {
    const state = match.interactiveState;
    if (!state) return 'AI_INNINGS';

    const innings = match.innings[match.currentInningsIndex];
    if (innings.isCompleted) {
      if (match.currentInningsIndex === 0) {
        state.phase = 'INNINGS_BREAK';
        return 'INNINGS_BREAK';
      } else {
        state.phase = 'MATCH_CONCLUDED';
        return 'MATCH_CONCLUDED';
      }
    }

    // Check if waiting on a pending running decision
    if (state.pendingRunningOpp) {
      state.phase = 'RUNNING_DECISION';
      return 'RUNNING_DECISION';
    }

    const striker = state.battingLineup[state.strikerIdx];
    const bowler = state.bowlingAttack[state.bowlerIdx];

    // Powerplay status in T20 (first 6 overs)
    state.isPowerplay = match.oversPerSide >= 20 && state.currentOver <= 6;

    if (striker && striker.isUser) {
      // User is on strike!
      state.phase = 'USER_BATTING';
      const bowlerAggression = AIAggression.BALANCED;
      const delivery = AIEngine.decideBowlerDelivery(bowler.ability, bowlerAggression, {
        oversRemaining: match.oversPerSide - state.currentOver,
        wicketsDown: innings.totalWickets,
        requiredRunRate: match.targetRuns ? (match.targetRuns - innings.totalRuns) / Math.max(1, match.oversPerSide - state.currentOver) : undefined,
        isPowerplay: state.isPowerplay
      });

      // Calculate trajectory
      const pitchType: PitchCondition = match.conditions?.pitchType || 'BALANCED';
      const trajectory = BallPhysics.calculateTrajectory(
        delivery.line,
        delivery.length,
        delivery.variation,
        BowlingStyle.RIGHT_ARM_FAST,
        pitchType,
        delivery.paceKph
      );

      state.pendingDelivery = {
        ...delivery,
        bowlerName: bowler.name,
        trajectory
      };
      return 'USER_BATTING';
    }

    if (bowler && bowler.isUser) {
      state.phase = 'USER_BOWLING';
      return 'USER_BOWLING';
    }

    // Check for random fielding event for user when team is fielding
    if (state.isUserBowlingTeam && Math.random() < 0.08 && !state.pendingFieldingPrompt) {
      state.pendingFieldingPrompt = FieldingSystem.generateFieldingEvent(player, striker.name);
      state.phase = 'USER_FIELDING';
      return 'USER_FIELDING';
    }

    state.phase = 'AI_INNINGS';
    return 'AI_INNINGS';
  }

  /**
   * Fast-forwards balls where the user is NOT actively involved until
   * the user is either on strike, bowling, responding to a fielding event, or innings ends.
   */
  public static advanceToNextUserMoment(match: MatchInstance, player: PlayerData): InteractivePhase {
    const state = match.interactiveState;
    if (!state) return 'AI_INNINGS';

    let safety = 0;
    while (safety < 300) {
      safety++;
      const phase = this.determineCurrentPhase(match, player);
      if (phase !== 'AI_INNINGS') {
        return phase;
      }

      // Simulate one AI ball
      this.simulateSingleAIBall(match, player);

      const innings = match.innings[match.currentInningsIndex];
      if (innings.isCompleted) {
        return this.determineCurrentPhase(match, player);
      }
    }

    return this.determineCurrentPhase(match, player);
  }

  /**
   * Executes the user's manual batting shot choice.
   */
  public static executeUserShot(
    match: MatchInstance,
    player: PlayerData,
    shot: BattingShot,
    timingOffsetMs: number,
    difficulty: DifficultyLevel = DifficultyLevel.NORMAL,
    fatigue: number = 0,
    direction: ShotDirection = ShotDirection.CENTER
  ): BattingExecutionResult {
    const state = match.interactiveState!;
    const innings = match.innings[match.currentInningsIndex];
    const delivery = state.pendingDelivery!;

    const timing = TimingSystem.evaluateOffset(timingOffsetMs, player, difficulty, fatigue);
    const result = BattingEngine.resolveShot(shot, delivery, timing, player, direction);

    const strikerScorecard = innings.batsmen[state.strikerIdx];
    const bowlerScorecard = innings.bowlers[state.bowlerIdx];

    strikerScorecard.balls++;
    match.playerPerformance.balls++;
    match.playerPerformance.didBat = true;

    // Track partnership
    state.partnershipBalls = (state.partnershipBalls || 0) + 1;

    if (result.isWicket) {
      innings.totalWickets++;
      strikerScorecard.isOut = true;
      strikerScorecard.dismissalText = result.dismissalText || 'c Fielder b Bowler';
      bowlerScorecard.wickets++;

      match.playerPerformance.isOut = true;
      match.playerPerformance.dismissal = strikerScorecard.dismissalText;

      match.commentaryLog.push(`[Ov ${state.currentOver}.${state.currentBallInOver}] ${result.commentary}`);
      state.lastDeliverySummary = result.commentary;
      state.recentBallsTimeline = state.recentBallsTimeline || [];
      state.recentBallsTimeline.push('W');
      if (state.recentBallsTimeline.length > 8) state.recentBallsTimeline.shift();

      // Trigger event
      MatchEventManager.triggerEvent('WICKET', 'WICKET FALLEN!', `${strikerScorecard.name} is dismissed!`, 'W');

      // Partnership breaks
      state.partnershipRuns = 0;
      state.partnershipBalls = 0;

      // User got out: next batsman comes in
      if (state.nextBatsmanIdx < state.battingLineup.length) {
        state.strikerIdx = state.nextBatsmanIdx;
        state.nextBatsmanIdx++;
      }
    } else {
      innings.totalRuns += result.runs;
      state.runsInCurrentOver += result.runs;
      bowlerScorecard.runs += result.runs;
      strikerScorecard.runs += result.runs;
      state.partnershipRuns = (state.partnershipRuns || 0) + result.runs;

      match.playerPerformance.runs += result.runs;

      state.recentBallsTimeline = state.recentBallsTimeline || [];
      state.recentBallsTimeline.push(result.runs === 0 ? '•' : `${result.runs}`);
      if (state.recentBallsTimeline.length > 8) state.recentBallsTimeline.shift();

      if (result.isBoundaryFour) {
        strikerScorecard.fours++;
        match.playerPerformance.fours++;
        MatchEventManager.triggerEvent('FOUR', 'CRACKING FOUR!', `${strikerScorecard.name} finds the fence!`, '4');
      }
      if (result.isBoundarySix) {
        strikerScorecard.sixes++;
        match.playerPerformance.sixes++;
        MatchEventManager.triggerEvent('SIX', 'MAXIMUM SIX!', `${strikerScorecard.name} clears the ropes!`, '6');
      }

      // Check in-match batter milestones
      MatchEventManager.checkBatterMilestone(strikerScorecard.name, strikerScorecard.runs, strikerScorecard.balls, true);

      match.commentaryLog.push(`[Ov ${state.currentOver}.${state.currentBallInOver}] ${result.commentary}`);
      state.lastDeliverySummary = result.commentary;

      // Check running push opportunity (for non-boundaries)
      const opp = RunningSystem.evaluateRunningOpportunity(result.runs, direction, result.isBoundaryFour || result.isBoundarySix);
      if (opp && opp.canTakeExtraRun && Math.random() < 0.35) {
        state.pendingRunningOpp = opp;
        state.phase = 'RUNNING_DECISION';
        return result;
      }

      // Strike rotation
      if (result.runs % 2 !== 0) {
        const temp = state.strikerIdx;
        state.strikerIdx = state.nonStrikerIdx;
        state.nonStrikerIdx = temp;
      }
    }

    this.advanceDeliveryCounter(match);
    return result;
  }

  /**
   * Resolves player's decision to push for a tight extra run.
   */
  public static resolveRunningChoice(
    match: MatchInstance,
    player: PlayerData,
    playerChoseRun: boolean,
    fatigue: number = 0
  ): RunningOutcome {
    const state = match.interactiveState!;
    const opp = state.pendingRunningOpp!;
    state.pendingRunningOpp = undefined;

    const outcome = RunningSystem.resolveRunningDecision(opp, playerChoseRun, player, fatigue);
    const innings = match.innings[match.currentInningsIndex];

    if (outcome.isRunOut) {
      innings.totalWickets++;
      const dismissedIdx = outcome.dismissedBatter === 'STRIKER' ? state.strikerIdx : state.nonStrikerIdx;
      const dismissedScorecard = innings.batsmen[dismissedIdx];
      dismissedScorecard.isOut = true;
      dismissedScorecard.dismissalText = `run out (${player.lastName})`;

      if (dismissedIdx === state.strikerIdx) {
        match.playerPerformance.isOut = true;
        match.playerPerformance.dismissal = dismissedScorecard.dismissalText;
      }

      match.commentaryLog.push(`[RUN OUT] ${outcome.commentary}`);
      state.lastDeliverySummary = outcome.commentary;

      if (state.nextBatsmanIdx < state.battingLineup.length) {
        if (dismissedIdx === state.strikerIdx) state.strikerIdx = state.nextBatsmanIdx;
        else state.nonStrikerIdx = state.nextBatsmanIdx;
        state.nextBatsmanIdx++;
      }
    } else if (outcome.runsCompleted > opp.baseRunsScored) {
      // Extra run completed
      innings.totalRuns += 1;
      state.runsInCurrentOver += 1;
      const strikerScorecard = innings.batsmen[state.strikerIdx];
      strikerScorecard.runs += 1;
      match.playerPerformance.runs += 1;

      // Strike rotates with the extra run
      const temp = state.strikerIdx;
      state.strikerIdx = state.nonStrikerIdx;
      state.nonStrikerIdx = temp;

      match.commentaryLog.push(`[RUNNING] ${outcome.commentary}`);
      state.lastDeliverySummary = outcome.commentary;
    }

    this.advanceDeliveryCounter(match);
    return outcome;
  }

  /**
   * Executes the user's manual bowling delivery.
   */
  public static executeUserBowling(
    match: MatchInstance,
    player: PlayerData,
    input: BowlingDeliveryInput,
    pitchType: string = 'BALANCED',
    fatigue: number = 0
  ): BowlingDeliveryResult {
    const state = match.interactiveState!;
    const innings = match.innings[match.currentInningsIndex];
    const striker = state.battingLineup[state.strikerIdx];

    const result = BowlingEngine.executeDelivery(
      input,
      player,
      { name: striker.name, ability: striker.ability, aggression: 'BALANCED' },
      pitchType,
      fatigue
    );

    const strikerScorecard = innings.batsmen[state.strikerIdx];
    const bowlerScorecard = innings.bowlers[state.bowlerIdx];

    match.playerPerformance.didBowl = true;

    if (result.isExtra) {
      innings.totalRuns += result.runsConceded;
      bowlerScorecard.runs += result.runsConceded;
      match.playerPerformance.runsConceded += result.runsConceded;
      if (result.extraType === 'wide') innings.extras.wides++;
      innings.extras.total++;

      match.commentaryLog.push(`[Ov ${state.currentOver}.${state.currentBallInOver}] ${result.commentary}`);
      state.lastDeliverySummary = result.commentary;
      state.recentBallsTimeline = state.recentBallsTimeline || [];
      state.recentBallsTimeline.push('Wd');
      if (state.recentBallsTimeline.length > 8) state.recentBallsTimeline.shift();
      return result;
    }

    strikerScorecard.balls++;

    if (result.isWicket) {
      innings.totalWickets++;
      strikerScorecard.isOut = true;
      strikerScorecard.dismissalText = result.dismissalText || `b ${player.lastName}`;
      bowlerScorecard.wickets++;
      match.playerPerformance.wickets++;

      MatchEventManager.triggerEvent('WICKET', 'BOWLED HIM!', `${strikerScorecard.name} clean dismissed!`, 'W');
      MatchEventManager.checkBowlerMilestone(
        `${player.firstName} ${player.lastName}`,
        match.playerPerformance.wickets,
        match.playerPerformance.runsConceded,
        bowlerScorecard.overs,
        true
      );

      match.commentaryLog.push(`[Ov ${state.currentOver}.${state.currentBallInOver}] ${result.commentary}`);
      state.lastDeliverySummary = result.commentary;
      state.recentBallsTimeline = state.recentBallsTimeline || [];
      state.recentBallsTimeline.push('W');
      if (state.recentBallsTimeline.length > 8) state.recentBallsTimeline.shift();

      if (state.nextBatsmanIdx < state.battingLineup.length) {
        state.strikerIdx = state.nextBatsmanIdx;
        state.nextBatsmanIdx++;
      }
    } else {
      innings.totalRuns += result.runsConceded;
      state.runsInCurrentOver += result.runsConceded;
      bowlerScorecard.runs += result.runsConceded;
      strikerScorecard.runs += result.runsConceded;
      match.playerPerformance.runsConceded += result.runsConceded;

      state.recentBallsTimeline = state.recentBallsTimeline || [];
      state.recentBallsTimeline.push(result.runsConceded === 0 ? '•' : `${result.runsConceded}`);
      if (state.recentBallsTimeline.length > 8) state.recentBallsTimeline.shift();

      match.commentaryLog.push(`[Ov ${state.currentOver}.${state.currentBallInOver}] ${result.commentary}`);
      state.lastDeliverySummary = result.commentary;

      if (result.runsConceded % 2 !== 0) {
        const temp = state.strikerIdx;
        state.strikerIdx = state.nonStrikerIdx;
        state.nonStrikerIdx = temp;
      }
    }

    this.advanceDeliveryCounter(match);
    return result;
  }

  /**
   * Resolves the user's reaction to a fielding prompt.
   */
  public static executeUserFielding(
    match: MatchInstance,
    player: PlayerData,
    reactionTimeMs: number,
    selectedTarget: ThrowTarget = 'DIRECT_HIT'
  ): FieldingEventOutcome {
    const state = match.interactiveState!;
    const innings = match.innings[match.currentInningsIndex];
    const prompt = state.pendingFieldingPrompt!;

    const outcome = FieldingSystem.resolveReaction(prompt, reactionTimeMs, player, selectedTarget);
    state.pendingFieldingPrompt = undefined;

    if (outcome.isCatch) {
      match.playerPerformance.catches++;
      innings.totalWickets++;
      const strikerScorecard = innings.batsmen[state.strikerIdx];
      strikerScorecard.isOut = true;
      strikerScorecard.dismissalText = `c ${player.lastName} b Bowler`;

      MatchEventManager.triggerEvent('WICKET', 'TAKEN AT SHORT COVER!', `${strikerScorecard.name} is caught!`, 'C');

      if (state.nextBatsmanIdx < state.battingLineup.length) {
        state.strikerIdx = state.nextBatsmanIdx;
        state.nextBatsmanIdx++;
      }
    } else if (outcome.isRunOut) {
      match.playerPerformance.runOuts++;
      innings.totalWickets++;
      const strikerScorecard = innings.batsmen[state.strikerIdx];
      strikerScorecard.isOut = true;
      strikerScorecard.dismissalText = `run out (${player.lastName})`;

      MatchEventManager.triggerEvent('WICKET', 'RUN OUT!', `${strikerScorecard.name} caught short!`, 'RO');

      if (state.nextBatsmanIdx < state.battingLineup.length) {
        state.strikerIdx = state.nextBatsmanIdx;
        state.nextBatsmanIdx++;
      }
    }

    match.commentaryLog.push(`[FIELDING] ${outcome.commentary}`);
    state.lastDeliverySummary = outcome.commentary;

    this.advanceDeliveryCounter(match);
    return outcome;
  }

  /**
   * Increments balls and overs, rotates strike at over end, and triggers innings completion checks.
   */
  private static advanceDeliveryCounter(match: MatchInstance): void {
    const state = match.interactiveState!;
    const innings = match.innings[match.currentInningsIndex];
    const target = match.targetRuns;

    // Check target reached
    if (target && innings.totalRuns >= target) {
      innings.isCompleted = true;
      this.concludeMatch(match);
      return;
    }

    // Check all out
    if (innings.totalWickets >= 10) {
      innings.isCompleted = true;
      this.handleInningsEnd(match);
      return;
    }

    state.currentBallInOver++;

    if (state.currentBallInOver > 6) {
      // End of over
      const bowlerScorecard = innings.bowlers[state.bowlerIdx];
      bowlerScorecard.overs++;
      if (state.runsInCurrentOver === 0) bowlerScorecard.maidens++;
      bowlerScorecard.economy = Math.round((bowlerScorecard.runs / bowlerScorecard.overs) * 100) / 100;

      if (state.bowlingAttack[state.bowlerIdx].isUser) {
        match.playerPerformance.overs++;
        if (state.runsInCurrentOver === 0) match.playerPerformance.maidens++;
      }

      innings.oversCompleted = state.currentOver;
      state.currentOver++;
      state.currentBallInOver = 1;
      state.runsInCurrentOver = 0;

      // Rotate strike
      const temp = state.strikerIdx;
      state.strikerIdx = state.nonStrikerIdx;
      state.nonStrikerIdx = temp;

      // Change bowler
      state.bowlerIdx = (state.bowlerIdx + 1) % state.bowlingAttack.length;

      // Check max overs reached
      if (innings.oversCompleted >= match.oversPerSide) {
        innings.isCompleted = true;
        this.handleInningsEnd(match);
      }
    }
  }

  private static simulateSingleAIBall(match: MatchInstance, player: PlayerData): void {
    const state = match.interactiveState!;
    const innings = match.innings[match.currentInningsIndex];
    const striker = state.battingLineup[state.strikerIdx];
    const bowler = state.bowlingAttack[state.bowlerIdx];

    const delivery = AIEngine.decideBowlerDelivery(bowler.ability, AIAggression.BALANCED, {
      oversRemaining: match.oversPerSide - state.currentOver,
      wicketsDown: innings.totalWickets,
      requiredRunRate: match.targetRuns ? (match.targetRuns - innings.totalRuns) / Math.max(1, match.oversPerSide - state.currentOver) : undefined,
      isPowerplay: state.isPowerplay
    });

    const shotResult = AIEngine.evaluateAIBatting(striker.ability, AIAggression.BALANCED, bowler.ability, delivery, {
      requiredRunRate: match.targetRuns ? (match.targetRuns - innings.totalRuns) / Math.max(1, match.oversPerSide - state.currentOver) : undefined,
      oversRemaining: match.oversPerSide - state.currentOver,
      wicketsDown: innings.totalWickets,
      isPowerplay: state.isPowerplay
    });

    const strikerScorecard = innings.batsmen[state.strikerIdx];
    const bowlerScorecard = innings.bowlers[state.bowlerIdx];

    strikerScorecard.balls++;

    if (shotResult.isWicket) {
      innings.totalWickets++;
      strikerScorecard.isOut = true;
      strikerScorecard.dismissalText = `b ${bowler.name}`;
      bowlerScorecard.wickets++;

      if (state.nextBatsmanIdx < state.battingLineup.length) {
        state.strikerIdx = state.nextBatsmanIdx;
        state.nextBatsmanIdx++;
      }
    } else {
      innings.totalRuns += shotResult.runs;
      state.runsInCurrentOver += shotResult.runs;
      bowlerScorecard.runs += shotResult.runs;
      strikerScorecard.runs += shotResult.runs;

      if (shotResult.isBoundaryFour) strikerScorecard.fours++;
      if (shotResult.isBoundarySix) strikerScorecard.sixes++;

      if (shotResult.runs % 2 !== 0) {
        const temp = state.strikerIdx;
        state.strikerIdx = state.nonStrikerIdx;
        state.nonStrikerIdx = temp;
      }
    }

    this.advanceDeliveryCounter(match);
  }

  private static handleInningsEnd(match: MatchInstance): void {
    if (match.currentInningsIndex === 0) {
      // First innings done -> prepare target
      match.targetRuns = match.innings[0].totalRuns + 1;
      match.currentInningsIndex = 1;
      match.interactiveState!.phase = 'INNINGS_BREAK';
    } else {
      this.concludeMatch(match);
    }
  }

  public static transitionToSecondInnings(match: MatchInstance, player: PlayerData): void {
    this.initInteractiveMatch(match, player);
  }

  public static concludeMatch(match: MatchInstance): void {
    match.isCompleted = true;
    if (match.interactiveState) {
      match.interactiveState.phase = 'MATCH_CONCLUDED';
    }

    const inn1 = match.innings[0];
    const inn2 = match.innings[1];

    let winnerId = '';
    let summary = '';

    if (inn2.totalRuns > inn1.totalRuns) {
      winnerId = inn2.battingTeamId;
      const wicketsLeft = 10 - inn2.totalWickets;
      summary = `${inn2.battingTeamName} won by ${wicketsLeft} wicket${wicketsLeft > 1 ? 's' : ''}`;
    } else if (inn1.totalRuns > inn2.totalRuns) {
      winnerId = inn1.battingTeamId;
      const runMargin = inn1.totalRuns - inn2.totalRuns;
      summary = `${inn1.battingTeamName} won by ${runMargin} run${runMargin > 1 ? 's' : ''}`;
    } else {
      winnerId = 'TIE';
      summary = 'Match Tied in thrilling finish!';
    }

    match.winnerTeamId = winnerId;
    match.resultSummary = summary;
  }

  private static buildBattingLineup(teamName: string, isUserTeam: boolean, player: PlayerData) {
    const surnames = ['Sharma', 'Smith', 'Root', 'Babar', 'Warner', 'Kohli', 'Stokes', 'Cummins', 'Bumrah', 'Shaheen', 'Rashid'];
    const lineup: Array<{ id: string; name: string; isUser: boolean; ability: number }> = [];

    let playerBattingPos = 4;
    if (player.role === PlayerRole.ALL_ROUNDER) playerBattingPos = 6;
    else if (player.role === PlayerRole.BOWLER) playerBattingPos = 8;

    for (let i = 1; i <= 11; i++) {
      if (isUserTeam && i === playerBattingPos) {
        lineup.push({ id: player.id, name: `${player.firstName} ${player.lastName}`, isUser: true, ability: player.batting.battingAbility });
      } else {
        const surname = surnames[(i + teamName.length) % surnames.length];
        lineup.push({ id: `p_${i}`, name: `Player ${surname}`, isUser: false, ability: 50 + (i <= 5 ? 15 : -10) });
      }
    }
    return lineup;
  }

  private static buildBowlingAttack(teamName: string, isUserTeam: boolean, player: PlayerData) {
    const bowlers: Array<{ id: string; name: string; isUser: boolean; ability: number; variation: BowlingVariation }> = [];
    const surnames = ['Pacer', 'Spinner', 'Quick', 'Strike', 'Swing'];

    for (let i = 0; i < 5; i++) {
      if (isUserTeam && i === 0 && (player.role === PlayerRole.BOWLER || player.role === PlayerRole.ALL_ROUNDER)) {
        bowlers.push({
          id: player.id,
          name: `${player.firstName} ${player.lastName}`,
          isUser: true,
          ability: player.bowling.bowlingAbility,
          variation: BowlingVariation.NORMAL_PACE
        });
      } else {
        bowlers.push({
          id: `b_${i}`,
          name: `${surnames[i % surnames.length]} (${teamName.substring(0, 3)})`,
          isUser: false,
          ability: 55,
          variation: BowlingVariation.NORMAL_PACE
        });
      }
    }
    return bowlers;
  }
}
