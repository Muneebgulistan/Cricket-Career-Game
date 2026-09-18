export interface VenueData {
  id: string;
  name: string;
  city: string;
  country: string;
  pitchType: 'FLAT_BATTING' | 'GREEN_SEAM' | 'DUSTY_SPIN' | 'BALANCED';
  capacity: number;
}

export const VENUES_DATABASE: VenueData[] = [
  { id: 'v_grassroots', name: 'Riverside Community Oval', city: 'Regional Center', country: 'Domestic', pitchType: 'BALANCED', capacity: 1500 },
  { id: 'v_memorial', name: 'Youth Memorial Ground', city: 'Capital District', country: 'Domestic', pitchType: 'FLAT_BATTING', capacity: 3200 },
  { id: 'v_eden', name: 'Eden Gardens', city: 'Kolkata', country: 'India', pitchType: 'DUSTY_SPIN', capacity: 66000 },
  { id: 'v_mcg', name: 'Melbourne Cricket Ground', city: 'Melbourne', country: 'Australia', pitchType: 'BALANCED', capacity: 100000 },
  { id: 'v_lords', name: 'Lord\'s Cricket Ground', city: 'London', country: 'England', pitchType: 'GREEN_SEAM', capacity: 31000 },
  { id: 'v_qaddafi', name: 'Gaddafi Stadium', city: 'Lahore', country: 'Pakistan', pitchType: 'FLAT_BATTING', capacity: 27000 },
  { id: 'v_wanderers', name: 'The Wanderers Stadium', city: 'Johannesburg', country: 'South Africa', pitchType: 'GREEN_SEAM', capacity: 34000 }
];

export class VenueRepository {
  public static getRandom(): VenueData {
    return VENUES_DATABASE[Math.floor(Math.random() * VENUES_DATABASE.length)];
  }

  public static getById(id: string): VenueData | undefined {
    return VENUES_DATABASE.find(v => v.id === id);
  }
}
