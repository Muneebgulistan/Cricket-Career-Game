export enum CalendarDay {
  MONDAY = 'Monday',
  TUESDAY = 'Tuesday',
  WEDNESDAY = 'Wednesday',
  THURSDAY = 'Thursday',
  FRIDAY = 'Friday',
  SATURDAY = 'Saturday',
  SUNDAY = 'Sunday'
}

export enum CalendarActivityType {
  TRAINING = 'TRAINING',
  REST = 'REST',
  RECOVERY = 'RECOVERY',
  MATCH_PREP = 'MATCH_PREP',
  MATCH = 'MATCH',
  TRAVEL = 'TRAVEL'
}

export interface CalendarSlot {
  day: CalendarDay;
  activity: CalendarActivityType;
  details?: string;
  isCompleted: boolean;
}

export interface WeeklySchedule {
  weekNumber: number;
  seasonYear: number;
  slots: CalendarSlot[];
  currentDayIndex: number; // 0 (Monday) to 6 (Sunday)
}

export class CareerCalendar {
  public static createDefaultWeek(weekNumber: number = 1, seasonYear: number = 2026): WeeklySchedule {
    return {
      weekNumber,
      seasonYear,
      currentDayIndex: 0,
      slots: [
        { day: CalendarDay.MONDAY, activity: CalendarActivityType.TRAINING, details: 'Nets Batting & Bowling Drills', isCompleted: false },
        { day: CalendarDay.TUESDAY, activity: CalendarActivityType.TRAINING, details: 'Fitness & Fielding Conditioning', isCompleted: false },
        { day: CalendarDay.WEDNESDAY, activity: CalendarActivityType.REST, details: 'Rest & Workload Reset', isCompleted: false },
        { day: CalendarDay.THURSDAY, activity: CalendarActivityType.TRAINING, details: 'Tactical Preparation & Simulation', isCompleted: false },
        { day: CalendarDay.FRIDAY, activity: CalendarActivityType.MATCH_PREP, details: 'Pitch Inspection & Team Selection', isCompleted: false },
        { day: CalendarDay.SATURDAY, activity: CalendarActivityType.MATCH, details: 'Tournament Fixture Match Day', isCompleted: false },
        { day: CalendarDay.SUNDAY, activity: CalendarActivityType.RECOVERY, details: 'Physio Ice Bath & Active Recovery', isCompleted: false }
      ]
    };
  }

  public static getCurrentSlot(schedule: WeeklySchedule): CalendarSlot {
    return schedule.slots[schedule.currentDayIndex] || schedule.slots[0];
  }

  public static setDayActivity(schedule: WeeklySchedule, dayIndex: number, activity: CalendarActivityType, details?: string): void {
    if (dayIndex >= 0 && dayIndex < schedule.slots.length) {
      schedule.slots[dayIndex].activity = activity;
      if (details) schedule.slots[dayIndex].details = details;
    }
  }

  /**
   * Advances the calendar by 1 day. If Sunday finishes, rolls to next week.
   */
  public static advanceDay(schedule: WeeklySchedule): { isNewWeek: boolean; currentDay: CalendarDay } {
    schedule.slots[schedule.currentDayIndex].isCompleted = true;

    if (schedule.currentDayIndex >= 6) {
      // New week!
      schedule.weekNumber++;
      schedule.currentDayIndex = 0;
      schedule.slots = this.createDefaultWeek(schedule.weekNumber, schedule.seasonYear).slots;
      return { isNewWeek: true, currentDay: CalendarDay.MONDAY };
    }

    schedule.currentDayIndex++;
    return { isNewWeek: false, currentDay: schedule.slots[schedule.currentDayIndex].day };
  }
}
