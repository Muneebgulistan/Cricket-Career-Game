import { CareerLevel } from '../career/CareerLevel';
import { MatchFormat } from '../cricket/CricketTypes';

export interface TournamentTemplate {
  id: string;
  name: string;
  type: 'CUP' | 'LEAGUE' | 'BILATERAL_SERIES' | 'WORLD_CUP';
  careerLevel: CareerLevel;
  totalTeams: number;
  totalMatches: number;
  format: MatchFormat;
  requiredPlayerLevel: CareerLevel;
  minimumPerformanceRequirement: number; // e.g. min match rating 5.0
  description: string;
  trophyName: string;
}

export const TOURNAMENT_TEMPLATES: TournamentTemplate[] = [
  {
    id: 'u16_state_cup',
    name: 'Under-16 State Youth Cup',
    type: 'CUP',
    careerLevel: CareerLevel.UNDER_16,
    totalTeams: 6,
    totalMatches: 5,
    format: MatchFormat.YOUTH_40,
    requiredPlayerLevel: CareerLevel.UNDER_16,
    minimumPerformanceRequirement: 4.5,
    description: 'Premier youth developmental competition. Prove yourself against top upcoming regional talent.',
    trophyName: 'State Youth Shield'
  },
  {
    id: 'u19_national_championship',
    name: 'Under-19 National Championship',
    type: 'LEAGUE',
    careerLevel: CareerLevel.UNDER_19,
    totalTeams: 8,
    totalMatches: 7,
    format: MatchFormat.YOUTH_50,
    requiredPlayerLevel: CareerLevel.UNDER_19,
    minimumPerformanceRequirement: 5.2,
    description: 'The premier national underage tournament. Monitored by national scouts and domestic franchises.',
    trophyName: 'National Under-19 Trophy'
  },
  {
    id: 'domestic_one_day_cup',
    name: 'National Domestic One-Day Trophy',
    type: 'LEAGUE',
    careerLevel: CareerLevel.DOMESTIC,
    totalTeams: 8,
    totalMatches: 7,
    format: MatchFormat.LIST_A,
    requiredPlayerLevel: CareerLevel.DOMESTIC,
    minimumPerformanceRequirement: 5.8,
    description: 'State and provincial one-day championship. A proving ground for international honours.',
    trophyName: 'Domestic One-Day Cup'
  },
  {
    id: 'country_premier_league',
    name: 'Super T20 Franchise League',
    type: 'LEAGUE',
    careerLevel: CareerLevel.COUNTRY_LEAGUE,
    totalTeams: 8,
    totalMatches: 8,
    format: MatchFormat.T20,
    requiredPlayerLevel: CareerLevel.COUNTRY_LEAGUE,
    minimumPerformanceRequirement: 6.2,
    description: 'High-stakes glamorous franchise tournament packed with international superstars and loud crowds.',
    trophyName: 'Franchise Premier Trophy'
  },
  {
    id: 'international_home_series',
    name: 'International Home Bilateral Series',
    type: 'BILATERAL_SERIES',
    careerLevel: CareerLevel.INTERNATIONAL_HOME,
    totalTeams: 2,
    totalMatches: 5,
    format: MatchFormat.ODI,
    requiredPlayerLevel: CareerLevel.INTERNATIONAL_HOME,
    minimumPerformanceRequirement: 6.5,
    description: 'Five-match ODI bilateral home series against a top tier visiting international nation.',
    trophyName: 'Bilateral Challenge Cup'
  },
  {
    id: 'international_away_tour',
    name: 'International Overseas Away Tour',
    type: 'BILATERAL_SERIES',
    careerLevel: CareerLevel.INTERNATIONAL_AWAY,
    totalTeams: 2,
    totalMatches: 5,
    format: MatchFormat.ODI,
    requiredPlayerLevel: CareerLevel.INTERNATIONAL_AWAY,
    minimumPerformanceRequirement: 6.8,
    description: 'Gruelling overseas away tour. Adapt to hostile conditions and foreign bouncy or spinning tracks.',
    trophyName: 'Trans-Oceanic Silver Trophy'
  },
  {
    id: 'test_series_championship',
    name: 'World Test Championship Series',
    type: 'BILATERAL_SERIES',
    careerLevel: CareerLevel.TEST,
    totalTeams: 2,
    totalMatches: 4,
    format: MatchFormat.TEST,
    requiredPlayerLevel: CareerLevel.TEST,
    minimumPerformanceRequirement: 7.2,
    description: 'Traditional 5-day red-ball Test cricket series. The purest test of character and skill.',
    trophyName: 'The Heritage Test Mace'
  },
  {
    id: 'icc_t20_world_cup',
    name: 'ICC Men\'s T20 World Cup',
    type: 'WORLD_CUP',
    careerLevel: CareerLevel.T20_WORLD_CUP,
    totalTeams: 12,
    totalMatches: 7,
    format: MatchFormat.T20,
    requiredPlayerLevel: CareerLevel.T20_WORLD_CUP,
    minimumPerformanceRequirement: 7.5,
    description: 'The global T20 tournament. 12 nations battling for world supremacy in front of millions.',
    trophyName: 'ICC T20 World Cup'
  },
  {
    id: 'icc_cricket_world_cup',
    name: 'ICC Men\'s Cricket World Cup (50 Overs)',
    type: 'WORLD_CUP',
    careerLevel: CareerLevel.ODI_WORLD_CUP,
    totalTeams: 10,
    totalMatches: 9,
    format: MatchFormat.ODI,
    requiredPlayerLevel: CareerLevel.ODI_WORLD_CUP,
    minimumPerformanceRequirement: 8.0,
    description: 'The pinnacle of the sport. Every cricketer\'s ultimate childhood dream.',
    trophyName: 'ICC World Cup Trophy'
  }
];
