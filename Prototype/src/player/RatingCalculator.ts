import { PlayerData, PlayerRole } from './PlayerModel';

export class RatingCalculator {
  /**
   * Calculates overall player rating (1 - 99) based on role and core attributes.
   */
  public static calculateOverall(player: Pick<PlayerData, 'role' | 'batting' | 'bowling' | 'fielding' | 'mental'>): number {
    const { role, batting, bowling, fielding, mental } = player;

    let rating = 50;

    switch (role) {
      case PlayerRole.BATSMAN: {
        const battingScore =
          batting.battingAbility * 0.35 +
          batting.battingTechnique * 0.25 +
          batting.timing * 0.20 +
          batting.power * 0.10 +
          batting.runningBetweenWickets * 0.10;

        const fieldingScore =
          fielding.fielding * 0.4 +
          fielding.catching * 0.4 +
          fielding.throwing * 0.2;

        const mentalScore =
          mental.pressureHandling * 0.5 +
          mental.confidence * 0.3 +
          mental.form * 0.2;

        rating = battingScore * 0.70 + fieldingScore * 0.15 + mentalScore * 0.15;
        break;
      }

      case PlayerRole.BOWLER: {
        const bowlingScore =
          bowling.bowlingAbility * 0.35 +
          bowling.accuracy * 0.25 +
          bowling.variation * 0.15 +
          Math.max(bowling.pace, bowling.spin) * 0.15 +
          Math.max(bowling.swing, bowling.seam) * 0.10;

        const fieldingScore =
          fielding.fielding * 0.5 +
          fielding.catching * 0.5;

        const mentalScore =
          mental.pressureHandling * 0.4 +
          mental.fitness * 0.4 +
          mental.form * 0.2;

        rating = bowlingScore * 0.70 + fieldingScore * 0.15 + mentalScore * 0.15;
        break;
      }

      case PlayerRole.ALL_ROUNDER: {
        const battingScore =
          batting.battingAbility * 0.4 +
          batting.battingTechnique * 0.3 +
          batting.power * 0.3;

        const bowlingScore =
          bowling.bowlingAbility * 0.4 +
          bowling.accuracy * 0.3 +
          bowling.variation * 0.3;

        const fieldingScore =
          fielding.fielding * 0.5 +
          fielding.catching * 0.5;

        const mentalScore =
          mental.fitness * 0.4 +
          mental.pressureHandling * 0.4 +
          mental.form * 0.2;

        rating = battingScore * 0.40 + bowlingScore * 0.40 + fieldingScore * 0.10 + mentalScore * 0.10;
        break;
      }

      case PlayerRole.WICKETKEEPER: {
        const battingScore =
          batting.battingAbility * 0.40 +
          batting.battingTechnique * 0.30 +
          batting.timing * 0.30;

        const keepingScore =
          fielding.catching * 0.45 +
          fielding.reflexes * 0.40 +
          fielding.fielding * 0.15;

        const mentalScore =
          mental.pressureHandling * 0.5 +
          mental.confidence * 0.3 +
          mental.fitness * 0.2;

        rating = battingScore * 0.45 + keepingScore * 0.45 + mentalScore * 0.10;
        break;
      }
    }

    return Math.round(Math.min(99, Math.max(1, rating)));
  }
}
