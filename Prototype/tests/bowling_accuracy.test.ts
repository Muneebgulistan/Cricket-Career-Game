import { describe, it, expect } from 'vitest';
import { BowlingEngine, BowlingVariation } from '../src/match/BowlingEngine';
import { DeliveryLine, DeliveryLength } from '../src/match/BattingEngine';
import { PlayerFactory } from '../src/player/PlayerFactory';
import { PlayerRole, BowlingStyle, BattingStyle } from '../src/player/PlayerModel';

describe('Step 3 — Bowling Accuracy & Power Meter', () => {
  const dummyBowler = PlayerFactory.createUnder16Player({
    firstName: 'Shaheen',
    lastName: 'Afridi',
    role: PlayerRole.BOWLER,
    battingStyle: BattingStyle.RIGHT_HAND,
    bowlingStyle: BowlingStyle.LEFT_ARM_FAST,
    nationality: 'Pakistan',
  });
  dummyBowler.bowlingStyle = BowlingStyle.LEFT_ARM_FAST;
  dummyBowler.bowling.accuracy = 99;

  it('should deliver on intended spot with high meter quality', () => {

    const result = BowlingEngine.executeDelivery(
      {
        line: DeliveryLine.OUTSIDE_OFF,
        length: DeliveryLength.GOOD_LENGTH,
        variation: BowlingVariation.OUTSWING,
        effortPacePercentage: 100,
        meterQuality: 0.95
      },
      dummyBowler,
      { name: 'Batter', ability: 60, aggression: 'BALANCED' },
      'BALANCED',
      0
    );

    expect(result.intendedLine).toBe(DeliveryLine.OUTSIDE_OFF);
    expect(result.intendedLength).toBe(DeliveryLength.GOOD_LENGTH);
    expect(result.isExtra).toBe(false);
  });

  it('should filter available variations strictly based on bowling style', () => {
    const paceVars = BowlingEngine.getAvailableVariations(BowlingStyle.RIGHT_ARM_FAST);
    expect(paceVars).toContain(BowlingVariation.OUTSWING);
    expect(paceVars).toContain(BowlingVariation.SLOWER_BALL);
    expect(paceVars).not.toContain(BowlingVariation.WRIST_VARIATION);

    const spinVars = BowlingEngine.getAvailableVariations(BowlingStyle.RIGHT_ARM_OFF_SPIN);
    expect(spinVars).toContain(BowlingVariation.ARM_BALL);
    expect(spinVars).toContain(BowlingVariation.SHARP_TURN);
    expect(spinVars).not.toContain(BowlingVariation.OUTSWING);
  });
});
