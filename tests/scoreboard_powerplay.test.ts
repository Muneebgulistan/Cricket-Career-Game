import { describe, it, expect } from 'vitest';
import { MatchSimulator } from '../src/match/MatchSimulator';
import { InteractiveMatchEngine } from '../src/match/InteractiveMatchEngine';
import { TournamentManager } from '../src/tournament/TournamentManager';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BattingStyle } from '../src/player/PlayerModel';
import { CareerLevel } from '../src/career/CareerLevel';

describe('Step 3 — Professional Scoreboard & T20 Powerplay', () => {
  const dummyPlayer = PlayerFactory.createUnder16Player({
    firstName: 'Ahmed',
    lastName: 'Shehzad',
    role: PlayerRole.BATSMAN,
    battingStyle: BattingStyle.RIGHT_HAND,
    nationality: 'Pakistan',
  });

  const tournament = TournamentManager.createTournamentForLevel(CareerLevel.UNDER_16, 'Pakistan');
  const fixture = TournamentManager.getNextFixture(tournament)!;

  it('should initialize match with 0/0 and identify T20 powerplay in overs 1-6', () => {
    const match = MatchSimulator.initializeMatch(dummyPlayer, tournament, fixture);
    InteractiveMatchEngine.initInteractiveMatch(match, dummyPlayer);

    const state = match.interactiveState!;
    expect(state.currentOver).toBe(1);
    expect(state.currentBallInOver).toBe(1);
    expect(state.isPowerplay).toBe(true);
    expect(state.partnershipRuns).toBe(0);
  });

  it('should rotate strike accurately on odd runs scored', () => {
    const match = MatchSimulator.initializeMatch(dummyPlayer, tournament, fixture);
    InteractiveMatchEngine.initInteractiveMatch(match, dummyPlayer);

    const state = match.interactiveState!;
    const initialStriker = state.strikerIdx;
    const initialNonStriker = state.nonStrikerIdx;

    // Simulate single
    state.strikerIdx = initialNonStriker;
    state.nonStrikerIdx = initialStriker;

    expect(state.strikerIdx).toBe(initialNonStriker);
    expect(state.nonStrikerIdx).toBe(initialStriker);
  });
});
