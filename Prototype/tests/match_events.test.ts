import { describe, it, expect, beforeEach } from 'vitest';
import { MatchEventManager } from '../src/match/MatchEventManager';

describe('Step 3 — Match Events & In-Match Milestones', () => {
  beforeEach(() => {
    MatchEventManager.resetMatchEvents();
  });

  it('should trigger in-match milestone notifications when batter reaches 50 and 100', () => {
    const fifty = MatchEventManager.checkBatterMilestone('Muneeb Gulistan', 50, 34, true);
    expect(fifty).not.toBeNull();
    expect(fifty?.title).toContain('HALF-CENTURY');

    // Should not trigger again at 51
    const duplicate = MatchEventManager.checkBatterMilestone('Muneeb Gulistan', 51, 35, true);
    expect(duplicate).toBeNull();

    // Reaching 100 triggers century
    const century = MatchEventManager.checkBatterMilestone('Muneeb Gulistan', 100, 68, true);
    expect(century).not.toBeNull();
    expect(century?.title).toContain('CENTURY');
  });

  it('should trigger bowler milestone notifications on 3 and 5 wickets', () => {
    const threeWkts = MatchEventManager.checkBowlerMilestone('Naseem Shah', 3, 14, 3, true);
    expect(threeWkts).not.toBeNull();
    expect(threeWkts?.title).toContain('THREE-WICKET');

    const fiveWkts = MatchEventManager.checkBowlerMilestone('Naseem Shah', 5, 22, 4, true);
    expect(fiveWkts).not.toBeNull();
    expect(fiveWkts?.title).toContain('FIVE-WICKET');
  });

  it('should trigger instant event notifications for boundaries and wickets', () => {
    const evt = MatchEventManager.triggerEvent('SIX', 'MAXIMUM!', 'Clears the fence', '6');
    expect(evt.type).toBe('SIX');
    expect(MatchEventManager.getLatestEvent()?.id).toBe(evt.id);
  });
});
