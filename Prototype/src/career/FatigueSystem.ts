export interface FatigueState {
  fatigueLevel: number;       // 0 - 100 (0 = fully refreshed, 100 = completely exhausted)
  weeklyTrainingLoad: number; // cumulative workload points
  matchOversBowled: number;   // recent bowling load
  matchBallsFaced: number;    // recent batting load
  recoveryRate: number;       // base recovery rate per rest day
}

export class FatigueSystem {
  public static createInitialState(): FatigueState {
    return {
      fatigueLevel: 10,
      weeklyTrainingLoad: 0,
      matchOversBowled: 0,
      matchBallsFaced: 0,
      recoveryRate: 20
    };
  }

  /**
   * Applies match physical exertion to fatigue.
   */
  public static applyMatchWorkload(
    state: FatigueState,
    ballsFaced: number,
    oversBowled: number
  ): { delta: number; newLevel: number } {
    // Bowling places 3x the physiological strain of batting
    const battingFatigue = Math.floor(ballsFaced / 15) * 2;
    const bowlingFatigue = Math.floor(oversBowled * 3.5);
    const totalAdded = Math.min(45, Math.max(8, battingFatigue + bowlingFatigue));

    state.fatigueLevel = Math.min(100, state.fatigueLevel + totalAdded);
    state.matchBallsFaced += ballsFaced;
    state.matchOversBowled += oversBowled;

    return { delta: totalAdded, newLevel: state.fatigueLevel };
  }

  /**
   * Applies training drill exertion.
   */
  public static applyTrainingLoad(state: FatigueState, drillIntensity: 'LOW' | 'MEDIUM' | 'HIGH'): number {
    let strain = 8;
    if (drillIntensity === 'MEDIUM') strain = 14;
    if (drillIntensity === 'HIGH') strain = 22;

    state.fatigueLevel = Math.min(100, state.fatigueLevel + strain);
    state.weeklyTrainingLoad += strain;
    return strain;
  }

  /**
   * Recovers stamina and clears fatigue on rest or physio days.
   */
  public static applyRestDay(state: FatigueState, isPhysioSession: boolean = false): number {
    const recovery = isPhysioSession ? state.recoveryRate * 1.8 : state.recoveryRate;
    const actualRecovery = Math.round(recovery);

    state.fatigueLevel = Math.max(0, state.fatigueLevel - actualRecovery);
    state.weeklyTrainingLoad = Math.max(0, state.weeklyTrainingLoad - 10);
    return actualRecovery;
  }

  /**
   * Returns penalty factor on reaction timing and bowling accuracy based on fatigue.
   */
  public static getFatiguePenaltyMultiplier(fatigue: number): number {
    if (fatigue < 40) return 1.0;
    if (fatigue < 65) return 0.90;
    if (fatigue < 85) return 0.75;
    return 0.55; // Extreme fatigue
  }
}
