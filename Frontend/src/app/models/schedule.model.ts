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
    dayOfWeek: DayOfWeek;
    startTime: string;
    endTime: string;
  }[];
}

export interface ProviderScheduleResponse {
  id: number;
  providerId: number;
  dayOfWeek: DayOfWeek | string; // Puede venir como número o string ("Monday", "Tuesday", etc.)
  startTime: string;
  endTime: string;
}
