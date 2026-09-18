export class Formatters {
  public static formatAverage(avg: number): string {
    if (isNaN(avg) || avg === 0) return '0.00';
    return avg.toFixed(2);
  }

  public static formatStrikeRate(sr: number): string {
    if (isNaN(sr) || sr === 0) return '0.00';
    return sr.toFixed(2);
  }

  public static formatEconomy(eco: number): string {
    if (isNaN(eco) || eco === 0) return '0.00';
    return eco.toFixed(2);
  }

  public static formatOvers(overs: number, balls: number = 0): string {
    if (balls === 0) return `${overs}.0`;
    return `${overs}.${balls}`;
  }

  public static formatBestBowling(wickets: number, runs: number): string {
    if (wickets === 0 && runs === 0) return '-';
    return `${wickets}/${runs}`;
  }

  public static getFormBadgeClass(form: number): string {
    if (form >= 75) return 'badge-success';
    if (form >= 50) return 'badge-primary';
    if (form >= 35) return 'badge-warning';
    return 'badge-danger';
  }

  public static getRatingBadgeClass(rating: number): string {
    if (rating >= 80) return 'badge-gold';
    if (rating >= 65) return 'badge-purple';
    if (rating >= 50) return 'badge-blue';
    return 'badge-gray';
  }
}
