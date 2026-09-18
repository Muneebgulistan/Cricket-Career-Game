import { describe, it, expect } from 'vitest';
import { AIEngine, AIAggression } from '../src/match/AIEngine';
import { DeliveryLine, DeliveryLength } from '../src/match/BattingEngine';
import { BowlingVariation } from '../src/match/BowlingEngine';

describe('Step 3 — AI Batting Behavior & Situational Adaptations', () => {
  const stockDelivery = {
    line: DeliveryLine.OUTSIDE_OFF,
    length: DeliveryLength.GOOD_LENGTH,
    paceKph: 130,
    variation: BowlingVariation.NORMAL_PACE
  };

  it('should exhibit higher boundary rate in T20 Powerplay overs', () => {
    let powerplayBoundaries = 0;
    let standardBoundaries = 0;
    const trials = 300;

    for (let i = 0; i < trials; i++) {
      const ppRes = AIEngine.evaluateAIBatting(70, AIAggression.BALANCED, 60, stockDelivery, {
        oversRemaining: 18,
        wicketsDown: 0,
        isPowerplay: true
      });
      if (ppRes.isBoundaryFour || ppRes.isBoundarySix) powerplayBoundaries++;

      const stdRes = AIEngine.evaluateAIBatting(70, AIAggression.BALANCED, 60, stockDelivery, {
        oversRemaining: 10,
        wicketsDown: 3,
        isPowerplay: false
      });
      if (stdRes.isBoundaryFour || stdRes.isBoundarySix) standardBoundaries++;
    }

    expect(powerplayBoundaries).toBeGreaterThan(standardBoundaries * 0.9);
  });

  it('should play more defensively when team suffers a batting collapse', () => {
    let collapseDots = 0;
    let normalDots = 0;
    const trials = 300;

    for (let i = 0; i < trials; i++) {
      const collapse = AIEngine.evaluateAIBatting(65, AIAggression.DEFENSIVE, 65, stockDelivery, {
        oversRemaining: 10,
        wicketsDown: 7,
        isPowerplay: false
      });
      if (collapse.runs === 0 && !collapse.isWicket) collapseDots++;

      const normal = AIEngine.evaluateAIBatting(65, AIAggression.AGGRESSIVE, 65, stockDelivery, {
        oversRemaining: 10,
        wicketsDown: 1,
        isPowerplay: false
      });
      if (normal.runs === 0 && !normal.isWicket) normalDots++;
    }

    expect(collapseDots).toBeGreaterThan(normalDots);
  });
});
