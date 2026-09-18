export enum CareerStatus {
  YOUTH_PLAYER = 'Youth Player',
  ACADEMY_PLAYER = 'Academy Player',
  SELECTED = 'Selected',
  PLAYING = 'Playing',
  SUBSTITUTE = 'Substitute',
  DROPPED = 'Dropped',
  INJURED = 'Injured',
  PROMOTED = 'Promoted',
  RETIRED = 'Retired'
}

export interface StatusTransitionRule {
  from: CareerStatus;
  to: CareerStatus;
  reason: string;
}
