export interface TimeRange {
  start: string; // "09:00"
  end: string;   // "13:00"
}

export interface DaySchedule {
  dayName: string;      // "Lunes", "Martes", etc.
  dayOfWeek: DayOfWeek; // Enum del backend
  isOpen: boolean;      // Toggle switch
  slots: TimeRange[];   // Array dinámico de rangos horarios
}

export enum DayOfWeek {
  Sunday = 0,
  Monday = 1,
  Tuesday = 2,
  Wednesday = 3,
  Thursday = 4,
  Friday = 5,
  Saturday = 6
}

export interface BulkUpdateScheduleDto {
  schedules: {
    day_of_week: DayOfWeek;
    start_time: string;
    end_time: string;
  }[];
}

export interface ProviderScheduleResponse {
  id: number;
  provider_id: number;
  day_of_week: DayOfWeek;
  start_time: string;
  end_time: string;
}
