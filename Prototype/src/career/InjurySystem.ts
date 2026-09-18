export enum InjurySeverity {
  MINOR = 'MINOR',
  MODERATE = 'MODERATE',
  MAJOR = 'MAJOR'
}

export interface ActiveInjury {
  id: string;
  name: string;
  severity: InjurySeverity;
  bodyPart: string;
  daysRemaining: number;
  totalDays: number;
  description: string;
}

export class InjurySystem {
  private static readonly INJURY_CATALOG = [
    { name: 'Hamstring Tightness', severity: InjurySeverity.MINOR, bodyPart: 'Hamstring', daysMin: 2, daysMax: 4 },
    { name: 'Split Webbing / Bruised Finger', severity: InjurySeverity.MINOR, bodyPart: 'Hand', daysMin: 1, daysMax: 3 },
    { name: 'Calf Strain', severity: InjurySeverity.MODERATE, bodyPart: 'Calf', daysMin: 5, daysMax: 9 },
    { name: 'Side Strain / Oblique Tear', severity: InjurySeverity.MODERATE, bodyPart: 'Torso', daysMin: 8, daysMax: 14 },
    { name: 'Lumbar Stress Fracture', severity: InjurySeverity.MAJOR, bodyPart: 'Back', daysMin: 16, daysMax: 28 },
    { name: 'ACL Knee Sprain', severity: InjurySeverity.MAJOR, bodyPart: 'Knee', daysMin: 20, daysMax: 35 }
  ];

  /**
   * Checks if an injury occurs based on fatigue, fitness, and match intensity.
   */
  public static checkForInjury(
    fatigue: number,
    fitness: number,
    isMatchEvent: boolean = false
  ): ActiveInjury | null {
    // Low base risk so player isn't constantly sidelined
    let riskChance = 0.01;

    if (fatigue >= 80) riskChance += 0.08;
    else if (fatigue >= 60) riskChance += 0.03;

    if (fitness < 40) riskChance += 0.05;

    if (isMatchEvent) riskChance += 0.015;

    if (Math.random() > riskChance) {
      return null;
    }

    // Determine severity
    let severity = InjurySeverity.MINOR;
    const roll = Math.random();
    if (fatigue >= 75 && roll < 0.25) {
      severity = InjurySeverity.MAJOR;
    } else if (roll < 0.45) {
      severity = InjurySeverity.MODERATE;
    }

    const matching = this.INJURY_CATALOG.filter(i => i.severity === severity);
    const template = matching[Math.floor(Math.random() * matching.length)] || this.INJURY_CATALOG[0];

    const days = template.daysMin + Math.floor(Math.random() * (template.daysMax - template.daysMin + 1));

    return {
      id: `inj_${Date.now()}`,
      name: template.name,
      severity: template.severity,
      bodyPart: template.bodyPart,
      daysRemaining: days,
      totalDays: days,
      description: `Sustained ${template.name.toLowerCase()} during intense physical workload.`
    };
  }

  /**
   * Advances injury healing by a specified number of days.
   */
  public static advanceHealing(injury: ActiveInjury, days: number = 1, hadPhysio: boolean = false): boolean {
    const healingDays = hadPhysio ? days * 1.5 : days;
    injury.daysRemaining = Math.max(0, injury.daysRemaining - healingDays);
    return injury.daysRemaining === 0; // True if fully recovered
  }
}
