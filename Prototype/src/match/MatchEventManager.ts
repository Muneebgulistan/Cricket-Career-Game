export type MatchEventType =
  | 'FOUR'
  | 'SIX'
  | 'WICKET'
  | 'APPEAL'
  | 'DROPPED_CATCH'
  | 'BATTER_MILESTONE'
  | 'BOWLER_MILESTONE'
  | 'PARTNERSHIP_MILESTONE'
  | 'HAT_TRICK_BALL'
  | 'MATCH_WINNING_MOMENT';

export interface MatchEventNotification {
  id: string;
  type: MatchEventType;
  title: string;
  subtitle: string;
  badge: string;
  timestamp: number;
}

export class MatchEventManager {
  private static activeEvents: MatchEventNotification[] = [];
  private static shownMilestones: Set<string> = new Set();

  public static resetMatchEvents(): void {
    this.activeEvents = [];
    this.shownMilestones.clear();
  }

  /**
   * Checks for batter score milestones (10, 25, 50, 75, 100).
   */
  public static checkBatterMilestone(
    batterName: string,
    runs: number,
    balls: number,
    isUser: boolean
  ): MatchEventNotification | null {
    const milestones = [100, 75, 50, 25, 10];

    for (const m of milestones) {
      const key = `${batterName}_${m}_runs`;
      if (runs >= m && !this.shownMilestones.has(key)) {
        // Mark this milestone and all lower milestones as shown
        milestones.filter(lower => lower <= m).forEach(lower => {
          this.shownMilestones.add(`${batterName}_${lower}_runs`);
        });

        let title = `${m} RUNS!`;
        if (m === 100) title = '💯 CENTURY COMPLETED!';
        else if (m === 50) title = '⭐ HALF-CENTURY!';

        const event: MatchEventNotification = {
          id: `bm_${Date.now()}_${m}`,
          type: 'BATTER_MILESTONE',
          title,
          subtitle: `${batterName} reaches ${runs} runs off ${balls} deliveries${isUser ? ' (Magnificent Batting!)' : ''}`,
          badge: `${m} RUNS`,
          timestamp: Date.now()
        };
        this.activeEvents.push(event);
        return event;
      }
    }
    return null;
  }

  /**
   * Checks for bowler wicket milestones (1, 2, 3, 4, 5).
   */
  public static checkBowlerMilestone(
    bowlerName: string,
    wickets: number,
    runsConceded: number,
    overs: number,
    isUser: boolean
  ): MatchEventNotification | null {
    const milestones = [5, 4, 3, 2, 1];

    for (const w of milestones) {
      const key = `${bowlerName}_${w}_wkts`;
      if (wickets >= w && !this.shownMilestones.has(key)) {
        milestones.filter(lower => lower <= w).forEach(lower => {
          this.shownMilestones.add(`${bowlerName}_${lower}_wkts`);
        });

        let title = `${w} WICKETS!`;
        if (w === 5) title = '🔥 FIVE-WICKET HAUL!';
        else if (w === 3) title = '🎯 THREE-WICKET BURST!';

        const event: MatchEventNotification = {
          id: `wm_${Date.now()}_${w}`,
          type: 'BOWLER_MILESTONE',
          title,
          subtitle: `${bowlerName} claims ${wickets}/${runsConceded} in ${overs} overs${isUser ? ' (Lethal Spell!)' : ''}`,
          badge: `${w} WICKETS`,
          timestamp: Date.now()
        };
        this.activeEvents.push(event);
        return event;
      }
    }
    return null;
  }

  /**
   * Creates an instantaneous event (e.g. Boundary, Six, Wicket, Appeal).
   */
  public static triggerEvent(
    type: MatchEventType,
    title: string,
    subtitle: string,
    badge: string
  ): MatchEventNotification {
    const event: MatchEventNotification = {
      id: `evt_${Date.now()}_${Math.random()}`,
      type,
      title,
      subtitle,
      badge,
      timestamp: Date.now()
    };
    this.activeEvents.push(event);
    return event;
  }

  public static getLatestEvent(): MatchEventNotification | undefined {
    return this.activeEvents[this.activeEvents.length - 1];
  }
}
