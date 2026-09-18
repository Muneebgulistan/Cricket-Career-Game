export enum CareerLevel {
  UNDER_16 = 'UNDER_16',
  UNDER_19 = 'UNDER_19',
  DOMESTIC = 'DOMESTIC',
  COUNTRY_LEAGUE = 'COUNTRY_LEAGUE',
  INTERNATIONAL_HOME = 'INTERNATIONAL_HOME',
  INTERNATIONAL_AWAY = 'INTERNATIONAL_AWAY',
  TEST = 'TEST',
  T20_WORLD_CUP = 'T20_WORLD_CUP',
  ODI_WORLD_CUP = 'ODI_WORLD_CUP'
}

export interface CareerLevelMetadata {
  level: CareerLevel;
  displayName: string;
  tier: number;
  minAge: number;
  description: string;
  defaultFormat: string;
  reputationMultiplier: number;
  // Promotion requirements to advance from this level to next
  promotionCriteria: {
    minMatches: number;
    minRuns?: number;
    minBattingAverage?: number;
    minWickets?: number;
    maxBowlingEconomy?: number;
    minOverallRating: number;
    minForm: number;
  };
  // Selection requirements to be picked in the Starting XI at this level
  selectionCriteria: {
    minForm: number;
    minFitness: number;
    minConfidence: number;
  };
}

export const CAREER_LEVEL_ORDER: CareerLevel[] = [
  CareerLevel.UNDER_16,
  CareerLevel.UNDER_19,
  CareerLevel.DOMESTIC,
  CareerLevel.COUNTRY_LEAGUE,
  CareerLevel.INTERNATIONAL_HOME,
  CareerLevel.INTERNATIONAL_AWAY,
  CareerLevel.TEST,
  CareerLevel.T20_WORLD_CUP,
  CareerLevel.ODI_WORLD_CUP
];

export const CAREER_LEVEL_CONFIG: Record<CareerLevel, CareerLevelMetadata> = {
  [CareerLevel.UNDER_16]: {
    level: CareerLevel.UNDER_16,
    displayName: 'Under-16 Youth Cricket',
    tier: 1,
    minAge: 14,
    description: 'Grassroots youth cricket. Prove your technique, composure, and raw talent against fellow youngsters.',
    defaultFormat: 'Youth 40 Overs',
    reputationMultiplier: 1.0,
    promotionCriteria: {
      minMatches: 4,
      minRuns: 100,
      minBattingAverage: 25,
      minWickets: 4,
      maxBowlingEconomy: 5.5,
      minOverallRating: 45,
      minForm: 55
    },
    selectionCriteria: {
      minForm: 30,
      minFitness: 40,
      minConfidence: 25
    }
  },
  [CareerLevel.UNDER_19]: {
    level: CareerLevel.UNDER_19,
    displayName: 'Under-19 National Championship',
    tier: 2,
    minAge: 17,
    description: 'High-intensity national youth cricket where national scouts look for future stars.',
    defaultFormat: 'Youth 50 Overs',
    reputationMultiplier: 1.5,
    promotionCriteria: {
      minMatches: 5,
      minRuns: 160,
      minBattingAverage: 30,
      minWickets: 6,
      maxBowlingEconomy: 5.2,
      minOverallRating: 54,
      minForm: 60
    },
    selectionCriteria: {
      minForm: 35,
      minFitness: 50,
      minConfidence: 35
    }
  },
  [CareerLevel.DOMESTIC]: {
    level: CareerLevel.DOMESTIC,
    displayName: 'Domestic First-Class / List-A',
    tier: 3,
    minAge: 18,
    description: 'State and provincial level. Competing against seasoned professionals and ex-internationals.',
    defaultFormat: 'List-A 50 Overs',
    reputationMultiplier: 2.2,
    promotionCriteria: {
      minMatches: 6,
      minRuns: 250,
      minBattingAverage: 34,
      minWickets: 9,
      maxBowlingEconomy: 5.0,
      minOverallRating: 62,
      minForm: 65
    },
    selectionCriteria: {
      minForm: 45,
      minFitness: 55,
      minConfidence: 45
    }
  },
  [CareerLevel.COUNTRY_LEAGUE]: {
    level: CareerLevel.COUNTRY_LEAGUE,
    displayName: 'County & Overseas Franchise League',
    tier: 4,
    minAge: 19,
    description: 'Top franchise T20 and English County cricket under global broadcast lights.',
    defaultFormat: 'T20',
    reputationMultiplier: 3.0,
    promotionCriteria: {
      minMatches: 6,
      minRuns: 280,
      minBattingAverage: 36,
      minWickets: 10,
      maxBowlingEconomy: 7.8,
      minOverallRating: 70,
      minForm: 68
    },
    selectionCriteria: {
      minForm: 50,
      minFitness: 60,
      minConfidence: 50
    }
  },
  [CareerLevel.INTERNATIONAL_HOME]: {
    level: CareerLevel.INTERNATIONAL_HOME,
    displayName: 'International Home Bilateral Series',
    tier: 5,
    minAge: 19,
    description: 'Wearing national colors in front of home supporters against top touring nations.',
    defaultFormat: 'ODI 50 Overs',
    reputationMultiplier: 4.0,
    promotionCriteria: {
      minMatches: 5,
      minRuns: 220,
      minBattingAverage: 40,
      minWickets: 8,
      maxBowlingEconomy: 5.2,
      minOverallRating: 76,
      minForm: 70
    },
    selectionCriteria: {
      minForm: 55,
      minFitness: 65,
      minConfidence: 55
    }
  },
  [CareerLevel.INTERNATIONAL_AWAY]: {
    level: CareerLevel.INTERNATIONAL_AWAY,
    displayName: 'International Away Tour',
    tier: 6,
    minAge: 20,
    description: 'Challenging overseas foreign pitches, hostile crowds, and unfamiliar conditions.',
    defaultFormat: 'ODI 50 Overs',
    reputationMultiplier: 5.0,
    promotionCriteria: {
      minMatches: 5,
      minRuns: 230,
      minBattingAverage: 42,
      minWickets: 9,
      maxBowlingEconomy: 5.0,
      minOverallRating: 81,
      minForm: 72
    },
    selectionCriteria: {
      minForm: 58,
      minFitness: 70,
      minConfidence: 60
    }
  },
  [CareerLevel.TEST]: {
    level: CareerLevel.TEST,
    displayName: 'Test Match Championship Series',
    tier: 7,
    minAge: 20,
    description: 'The pinnacle of cricket skill and mental endurance over 5 gruelling days with the red ball.',
    defaultFormat: 'Test (5 Days)',
    reputationMultiplier: 6.5,
    promotionCriteria: {
      minMatches: 4,
      minRuns: 300,
      minBattingAverage: 45,
      minWickets: 12,
      maxBowlingEconomy: 3.5,
      minOverallRating: 85,
      minForm: 75
    },
    selectionCriteria: {
      minForm: 60,
      minFitness: 75,
      minConfidence: 65
    }
  },
  [CareerLevel.T20_WORLD_CUP]: {
    level: CareerLevel.T20_WORLD_CUP,
    displayName: 'ICC T20 World Cup',
    tier: 8,
    minAge: 20,
    description: 'The global high-octane spectacle. Fast-paced world glory under supreme pressure.',
    defaultFormat: 'T20',
    reputationMultiplier: 8.0,
    promotionCriteria: {
      minMatches: 5,
      minRuns: 240,
      minBattingAverage: 38,
      minWickets: 8,
      maxBowlingEconomy: 7.2,
      minOverallRating: 88,
      minForm: 75
    },
    selectionCriteria: {
      minForm: 62,
      minFitness: 75,
      minConfidence: 65
    }
  },
  [CareerLevel.ODI_WORLD_CUP]: {
    level: CareerLevel.ODI_WORLD_CUP,
    displayName: 'ICC Cricket World Cup (50 Overs)',
    tier: 9,
    minAge: 21,
    description: 'The ultimate holy grail in world cricket. Becoming world champion immortalizes a career forever.',
    defaultFormat: 'ODI 50 Overs',
    reputationMultiplier: 10.0,
    promotionCriteria: {
      minMatches: 7,
      minRuns: 350,
      minBattingAverage: 48,
      minWickets: 12,
      maxBowlingEconomy: 4.8,
      minOverallRating: 92,
      minForm: 80
    },
    selectionCriteria: {
      minForm: 65,
      minFitness: 80,
      minConfidence: 70
    }
  }
};
