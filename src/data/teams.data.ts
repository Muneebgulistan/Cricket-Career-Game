export interface TeamData {
  id: string;
  name: string;
  shortName: string;
  country: string;
  category: 'YOUTH' | 'DOMESTIC' | 'FRANCHISE' | 'INTERNATIONAL';
  strengthRating: number; // 1 - 99
  primaryColor: string;
  secondaryColor: string;
}

export const TEAMS_DATABASE: TeamData[] = [
  // Youth Teams
  { id: 'u16_north', name: 'Northern Rising Stars U16', shortName: 'NRS', country: 'Domestic', category: 'YOUTH', strengthRating: 44, primaryColor: '#2563eb', secondaryColor: '#93c5fd' },
  { id: 'u16_south', name: 'Southern Titans U16', shortName: 'STS', country: 'Domestic', category: 'YOUTH', strengthRating: 46, primaryColor: '#dc2626', secondaryColor: '#fca5a5' },
  { id: 'u16_east', name: 'Eastern Eagles U16', shortName: 'EES', country: 'Domestic', category: 'YOUTH', strengthRating: 43, primaryColor: '#059669', secondaryColor: '#a7f3d0' },
  { id: 'u16_west', name: 'Western Warriors U16', shortName: 'WWS', country: 'Domestic', category: 'YOUTH', strengthRating: 45, primaryColor: '#d97706', secondaryColor: '#fde68a' },
  { id: 'u16_central', name: 'Central Colts U16', shortName: 'CCS', country: 'Domestic', category: 'YOUTH', strengthRating: 42, primaryColor: '#7c3aed', secondaryColor: '#ddd6fe' },
  { id: 'u16_metro', name: 'Metro Strikers U16', shortName: 'MSS', country: 'Domestic', category: 'YOUTH', strengthRating: 47, primaryColor: '#0891b2', secondaryColor: '#a5f3fc' },

  // Under-19 National Championship Teams
  { id: 'u19_state_1', name: 'Punjab Pioneers U19', shortName: 'PJB', country: 'Domestic', category: 'YOUTH', strengthRating: 52, primaryColor: '#e11d48', secondaryColor: '#fecdd3' },
  { id: 'u19_state_2', name: 'Mumbai Mariners U19', shortName: 'MUM', country: 'Domestic', category: 'YOUTH', strengthRating: 56, primaryColor: '#1d4ed8', secondaryColor: '#bfdbfe' },
  { id: 'u19_state_3', name: 'Karnataka Knights U19', shortName: 'KAR', country: 'Domestic', category: 'YOUTH', strengthRating: 54, primaryColor: '#b45309', secondaryColor: '#fde68a' },
  { id: 'u19_state_4', name: 'Yorkshire Academy U19', shortName: 'YOR', country: 'England', category: 'YOUTH', strengthRating: 53, primaryColor: '#047857', secondaryColor: '#a7f3d0' },
  { id: 'u19_state_5', name: 'NSW Bluebaggers U19', shortName: 'NSW', country: 'Australia', category: 'YOUTH', strengthRating: 55, primaryColor: '#0369a1', secondaryColor: '#bae6fd' },
  { id: 'u19_state_6', name: 'Lahore Qalandars Academy', shortName: 'LHR', country: 'Pakistan', category: 'YOUTH', strengthRating: 54, primaryColor: '#15803d', secondaryColor: '#bbf7d0' },

  // Domestic First-Class Teams
  { id: 'dom_rhinos', name: 'Apex Rhinos', shortName: 'RHN', country: 'Domestic', category: 'DOMESTIC', strengthRating: 64, primaryColor: '#4b5563', secondaryColor: '#e5e7eb' },
  { id: 'dom_stallions', name: 'Royal Stallions', shortName: 'STL', country: 'Domestic', category: 'DOMESTIC', strengthRating: 66, primaryColor: '#854d0e', secondaryColor: '#fef08a' },
  { id: 'dom_gladiators', name: 'Imperial Gladiators', shortName: 'GLD', country: 'Domestic', category: 'DOMESTIC', strengthRating: 67, primaryColor: '#991b1b', secondaryColor: '#fecaca' },
  { id: 'dom_falcons', name: 'Coastal Falcons', shortName: 'FLC', country: 'Domestic', category: 'DOMESTIC', strengthRating: 65, primaryColor: '#1e40af', secondaryColor: '#dbeafe' },

  // Franchise T20 League Teams
  { id: 't20_superkings', name: 'Super Kings XI', shortName: 'CSK', country: 'Franchise', category: 'FRANCHISE', strengthRating: 75, primaryColor: '#f59e0b', secondaryColor: '#1e3a8a' },
  { id: 't20_blasters', name: 'Bengaluru Blasters', shortName: 'BLR', country: 'Franchise', category: 'FRANCHISE', strengthRating: 74, primaryColor: '#dc2626', secondaryColor: '#18181b' },
  { id: 't20_heat', name: 'Brisbane Heatwave', shortName: 'HEA', country: 'Franchise', category: 'FRANCHISE', strengthRating: 73, primaryColor: '#0d9488', secondaryColor: '#ccfbf1' },
  { id: 't20_braves', name: 'Southern Braves', shortName: 'SOB', country: 'Franchise', category: 'FRANCHISE', strengthRating: 76, primaryColor: '#16a34a', secondaryColor: '#dcfce7' },

  // International Nations
  { id: 'int_india', name: 'India', shortName: 'IND', country: 'India', category: 'INTERNATIONAL', strengthRating: 90, primaryColor: '#1d4ed8', secondaryColor: '#f97316' },
  { id: 'int_australia', name: 'Australia', shortName: 'AUS', country: 'Australia', category: 'INTERNATIONAL', strengthRating: 91, primaryColor: '#eab308', secondaryColor: '#15803d' },
  { id: 'int_england', name: 'England', shortName: 'ENG', country: 'England', category: 'INTERNATIONAL', strengthRating: 88, primaryColor: '#dc2626', secondaryColor: '#1e3a8a' },
  { id: 'int_pakistan', name: 'Pakistan', shortName: 'PAK', country: 'Pakistan', category: 'INTERNATIONAL', strengthRating: 86, primaryColor: '#15803d', secondaryColor: '#86efac' },
  { id: 'int_south_africa', name: 'South Africa', shortName: 'SA', country: 'South Africa', category: 'INTERNATIONAL', strengthRating: 87, primaryColor: '#166534', secondaryColor: '#facc15' },
  { id: 'int_new_zealand', name: 'New Zealand', shortName: 'NZ', country: 'New Zealand', category: 'INTERNATIONAL', strengthRating: 85, primaryColor: '#18181b', secondaryColor: '#f4f4f5' },
  { id: 'int_west_indies', name: 'West Indies', shortName: 'WI', country: 'West Indies', category: 'INTERNATIONAL', strengthRating: 82, primaryColor: '#831843', secondaryColor: '#facc15' },
  { id: 'int_sri_lanka', name: 'Sri Lanka', shortName: 'SL', country: 'Sri Lanka', category: 'INTERNATIONAL', strengthRating: 81, primaryColor: '#1e3a8a', secondaryColor: '#eab308' }
];

export class TeamRepository {
  public static getById(id: string): TeamData | undefined {
    return TEAMS_DATABASE.find(t => t.id === id);
  }

  public static getTeamsForCategory(category: TeamData['category']): TeamData[] {
    return TEAMS_DATABASE.filter(t => t.category === category);
  }
}
