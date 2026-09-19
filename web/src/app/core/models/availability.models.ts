export type DayOfWeekName =
  | 'Sunday'
  | 'Monday'
  | 'Tuesday'
  | 'Wednesday'
  | 'Thursday'
  | 'Friday'
  | 'Saturday';

export interface AvailabilitySlot {
  dayOfWeek: DayOfWeekName;
  hoursAvailable: number;
}
