import { PlayerData, PlayerRole } from '../player/PlayerModel';

export interface MatchPerformanceInput {
  runsScored: number;
  ballsFaced: number;
  fours: number;
  sixes: number;
  isOut: boolean;

  oversBowled: number;
  maidens: number;
  runsConceded: number;
  wicketsTaken: number;

  catches: number;
  runOuts: number;
  stumpings: number;

  matchResult: 'WIN' | 'LOSS' | 'DRAW';
  isManOfTheMatch?: boolean;
}

export interface PerformanceEvaluationResult {
  matchRating: number;          // 1.0 - 10.0 scale (like Cricinfo / FIFA match ratings)
  battingScore: number;
  bowlingScore: number;
  fieldingScore: number;
  formDelta: number;            // -15 to +15
  confidenceDelta: number;      // -15 to +15
  fitnessDelta: number;         // -10 to -2
  xpGained: number;             // Experience points toward attribute progression
  summary: string;
}

export class PerformanceEvaluator {
  /**
   * Evaluates a player's individual match performance and calculates
   * ratings, form changes, confidence, and attribute experience.
   */
  public static evaluateMatch(player: PlayerData, input: MatchPerformanceInput): PerformanceEvaluationResult {
    // 1. Batting Score (0 - 100)
    let battingScore = 0;
    if (input.ballsFaced > 0) {
      const strikeRate = (input.runsScored / input.ballsFaced) * 100;
      const baseRuns = Math.min(60, input.runsScored * 0.6);
      const boundaryBonus = (input.fours * 2) + (input.sixes * 4);
      const srFactor = strikeRate >= 120 ? 15 : (strikeRate >= 85 ? 10 : (strikeRate >= 60 ? 5 : 0));
      const notOutBonus = (!input.isOut && input.runsScored >= 20) ? 10 : 0;
      battingScore = Math.min(100, baseRuns + boundaryBonus + srFactor + notOutBonus);
    }

    // 2. Bowling Score (0 - 100)
    let bowlingScore = 0;
    if (input.oversBowled > 0) {
      const economy = input.runsConceded / input.oversBowled;
      const wicketPoints = input.wicketsTaken * 22;
      const maidenPoints = input.maidens * 6;
      let ecoPoints = 0;
      if (economy <= 3.5) ecoPoints = 25;
      else if (economy <= 4.5) ecoPoints = 20;
      else if (economy <= 6.0) ecoPoints = 12;
      else if (economy >= 8.5) ecoPoints = -10;

      bowlingScore = Math.max(0, Math.min(100, wicketPoints + maidenPoints + ecoPoints + 15));
    }

    // 3. Fielding Score (0 - 100)
    const fieldingScore = Math.min(100, (input.catches * 25) + (input.runOuts * 30) + (input.stumpings * 30));

    // 4. Role-weighted Combined Match Rating (1.0 to 10.0 scale)
    let rawRating = 5.0;

    switch (player.role) {
      case PlayerRole.BATSMAN: {
        rawRating = (battingScore * 0.75 + fieldingScore * 0.25) / 10;
        break;
      }
      case PlayerRole.BOWLER: {
        rawRating = (bowlingScore * 0.75 + fieldingScore * 0.25) / 10;
        break;
      }
      case PlayerRole.ALL_ROUNDER: {
        const battingWeight = input.ballsFaced > 0 ? 0.45 : 0.15;
        const bowlingWeight = input.oversBowled > 0 ? 0.45 : 0.15;
        const fieldingWeight = 1 - (battingWeight + bowlingWeight);
        rawRating = (battingScore * battingWeight + bowlingScore * bowlingWeight + fieldingScore * fieldingWeight) / 10;
        break;
      }
      case PlayerRole.WICKETKEEPER: {
        rawRating = (battingScore * 0.50 + fieldingScore * 0.50) / 10;
        break;
      }
    }

    // Man of the Match bonus
    if (input.isManOfTheMatch) {
      rawRating += 1.0;
    }

    // Win bonus
    if (input.matchResult === 'WIN') {
      rawRating += 0.4;
    } else if (input.matchResult === 'LOSS') {
      rawRating -= 0.3;
    }

    const matchRating = Math.max(1.0, Math.min(10.0, Math.round(rawRating * 10) / 10));

    // 5. Form Delta based on rating
    let formDelta = 0;
    if (matchRating >= 8.0) formDelta = 8 + Math.round((matchRating - 8.0) * 4);
    else if (matchRating >= 6.5) formDelta = 3;
    else if (matchRating >= 5.0) formDelta = 0;
    else if (matchRating >= 4.0) formDelta = -4;
    else formDelta = -8;

    // 6. Confidence Delta
    let confidenceDelta = 0;
    if (matchRating >= 7.5) confidenceDelta = 6;
    else if (matchRating >= 6.0) confidenceDelta = 2;
    else if (matchRating < 4.5) confidenceDelta = -6;

    if (input.matchResult === 'WIN') confidenceDelta += 2;
    else if (input.matchResult === 'LOSS') confidenceDelta -= 2;

    // 7. Fitness Drain
    const fitnessDelta = -(4 + Math.min(6, Math.floor(input.oversBowled * 0.8) + Math.floor(input.ballsFaced / 25)));

    // 8. XP Gained
    const xpGained = Math.round(matchRating * 30 * player.potential.developmentRate);

    // Summary statement
    let summary = 'A steady contribution for the team.';
    if (matchRating >= 9.0) summary = 'Masterclass performance! Headline-grabbing heroics.';
    else if (matchRating >= 7.5) summary = 'Terrific display under pressure. Crucial for team momentum.';
    else if (matchRating >= 6.0) summary = 'Solid, disciplined performance with positive flashes.';
    else if (matchRating < 4.0) summary = 'Struggled to find rhythm and timing today.';

    return {
      matchRating,
      battingScore: Math.round(battingScore),
      bowlingScore: Math.round(bowlingScore),
      fieldingScore: Math.round(fieldingScore),
      formDelta,
      confidenceDelta,
      fitnessDelta,
      xpGained,
      summary
    };
  }
}
