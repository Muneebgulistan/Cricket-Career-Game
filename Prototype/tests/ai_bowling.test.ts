import { describe, it, expect } from 'vitest';
import { AIEngine, AIAggression } from '../src/match/AIEngine';
import { DeliveryLength } from '../src/match/BattingEngine';

describe('Step 3 — AI Bowling Strategy', () => {
  it('should bowl yorkers and bouncers more frequently in death overs', () => {
    let deathAttackingDeliveries = 0;
    const trials = 200;

    for (let i = 0; i < trials; i++) {
      const delivery = AIEngine.decideBowlerDelivery(75, AIAggression.AGGRESSIVE, {
        oversRemaining: 3,
        wicketsDown: 5
      });
      if (delivery.length === DeliveryLength.YORKER || delivery.length === DeliveryLength.BOUNCER) {
        deathAttackingDeliveries++;
      }
    }

    expect(deathAttackingDeliveries).toBeGreaterThan(trials * 0.45);
  });

  it('should adapt to aggressive batters with short balls or wider lines', () => {
    let tacticalDeliveries = 0;
    const trials = 200;

    for (let i = 0; i < trials; i++) {
      const del = AIEngine.decideBowlerDelivery(70, AIAggression.BALANCED, {
        oversRemaining: 12,
        wicketsDown: 2,
        batterAggression: AIAggression.AGGRESSIVE
      });
      if (del.length === DeliveryLength.SHORT || del.length === DeliveryLength.YORKER) {
        tacticalDeliveries++;
      }
    }

    expect(tacticalDeliveries).toBeGreaterThan(trials * 0.40);
  });
});
