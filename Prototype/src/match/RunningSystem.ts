import { PlayerData } from '../player/PlayerModel';

export interface RunningOpportunity {
  baseRunsScored: number;
  canTakeExtraRun: boolean;
  extraRunRisk: 'LOW' | 'MEDIUM' | 'HIGH';
  fielderDistanceMeters: number;
  throwTargetEnd: 'STRIKER' | 'NON_STRIKER';
  commentaryHint: string;
}

export interface RunningOutcome {
  runsCompleted: number;
  isRunOut: boolean;
  dismissedBatter: 'STRIKER' | 'NON_STRIKER';
  commentary: string;
}

export class RunningSystem {
  /**
   * Evaluates if a running push opportunity exists after a ground stroke.
   */
  public static evaluateRunningOpportunity(
    runsHit: number,
    shotDirection: 'LEFT' | 'CENTER' | 'RIGHT',
    isBoundary: boolean
  ): RunningOpportunity | null {
    if (isBoundary || runsHit >= 4) return null;

    if (runsHit === 1) {
      // Chance for a second run in deep areas
      const isDeepZone = shotDirection === 'LEFT' || shotDirection === 'RIGHT';
      return {
        baseRunsScored: 1,
        canTakeExtraRun: isDeepZone,
        extraRunRisk: isDeepZone ? 'MEDIUM' : 'HIGH',
        fielderDistanceMeters: isDeepZone ? 45 : 22,
        throwTargetEnd: Math.random() < 0.5 ? 'STRIKER' : 'NON_STRIKER',
        commentaryHint: isDeepZone
          ? 'Deep in the outfield! The arm is taking time to gather—there could be a tight second on offer!'
          : 'Fielder is swooping in quickly from the ring!'
      };
    }

    if (runsHit === 2) {
      return {
        baseRunsScored: 2,
        canTakeExtraRun: true,
        extraRunRisk: 'HIGH',
        fielderDistanceMeters: 60,
        throwTargetEnd: 'STRIKER',
        commentaryHint: 'Sweeper covers the boundary slowly. Dare you push for a breathless third?'
      };
    }

    return null;
  }

  /**
   * Resolves the player's decision to push for the additional run or stay safe.
   */
  public static resolveRunningDecision(
    opportunity: RunningOpportunity,
    playerChoseRun: boolean,
    player: PlayerData,
    fatigue: number = 0
  ): RunningOutcome {
    if (!playerChoseRun) {
      return {
        runsCompleted: opportunity.baseRunsScored,
        isRunOut: false,
        dismissedBatter: 'STRIKER',
        commentary: 'Decided against the risky run and grounded the bat safely in the crease.'
      };
    }

    // Player pushed for the extra run!
    // Running speed calculation based on running skill, mental fitness, and fatigue
    const speedRating = (player.batting.runningBetweenWickets * 0.6) + (player.mental.fitness * 0.4);
    const effectiveSpeed = Math.max(30, speedRating - (fatigue * 0.4));

    // Throw accuracy roll
    let runOutRiskPct = 0.25;
    if (opportunity.extraRunRisk === 'HIGH') runOutRiskPct = 0.50;
    if (opportunity.extraRunRisk === 'LOW') runOutRiskPct = 0.12;

    // Faster running reduces risk
    runOutRiskPct -= (effectiveSpeed - 50) * 0.005;

    const roll = Math.random();
    if (roll < runOutRiskPct) {
      // Run out!
      const dismissed = opportunity.throwTargetEnd;
      return {
        runsCompleted: opportunity.baseRunsScored,
        isRunOut: true,
        dismissedBatter: dismissed,
        commentary: `OUT! DIRECT HIT! The throw is lightning fast and shatters the stumps as the diving batsman is inches short of the crease!`
      };
    }

    // Made it home safely
    return {
      runsCompleted: opportunity.baseRunsScored + 1,
      isRunOut: false,
      dismissedBatter: 'STRIKER',
      commentary: `Superb hustle! Pushed hard, turned on a sixpence, and dived in safely to steal the extra run!`
    };
  }
}
