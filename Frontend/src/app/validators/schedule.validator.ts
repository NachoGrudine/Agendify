import { ProviderScheduleResponse } from '../models/schedule.model';
import { DateTimeHelper } from '../helpers/date-time.helper';

/**
 * Validador para verificar que los appointments estén dentro del horario del provider
 */
export class ScheduleValidator {

  /**
   * Mapeo de nombres de días a números
   */
  private static readonly dayNameToNumber: { [key: string]: number } = {
    'Sunday': 0,
    'Monday': 1,
    'Tuesday': 2,
    'Wednesday': 3,
    'Thursday': 4,
    'Friday': 5,
    'Saturday': 6
  };

  /**
   * Valida que un rango de tiempo esté dentro del horario laboral del provider
   */
  static isWithinProviderSchedule(
    start: Date,
    end: Date,
    providerSchedules: ProviderScheduleResponse[]
  ): boolean {
    const dayOfWeek = start.getDay();

    // Filtrar schedules del día actual
    const daySchedules = this.getSchedulesForDay(dayOfWeek, providerSchedules);

    if (daySchedules.length === 0) {
      return false;
    }

    const startMinutes = start.getHours() * 60 + start.getMinutes();
    const endMinutes = end.getHours() * 60 + end.getMinutes();

    return daySchedules.some(schedule => {
      const scheduleStart = DateTimeHelper.timeSpanToMinutes(schedule.startTime);
      const scheduleEnd = DateTimeHelper.timeSpanToMinutes(schedule.endTime);
      return startMinutes >= scheduleStart && endMinutes <= scheduleEnd;
    });
  }

  /**
   * Obtiene los schedules para un día específico
   */
  static getSchedulesForDay(dayOfWeek: number, providerSchedules: ProviderScheduleResponse[]): ProviderScheduleResponse[]
  {
    return providerSchedules.filter(s => {
      const scheduleDayOfWeek = typeof s.dayOfWeek === 'string'
        ? this.dayNameToNumber[s.dayOfWeek]
        : s.dayOfWeek;
      return scheduleDayOfWeek === dayOfWeek;
    });
  }

  /**
   * Verifica si un provider trabaja en un día específico
   */
  static providerWorksOnDay(
    dayOfWeek: number,
    providerSchedules: ProviderScheduleResponse[]
  ): boolean {
    return this.getSchedulesForDay(dayOfWeek, providerSchedules).length > 0;
  }

  /**
   * Obtiene el horario de trabajo para un día (ej: "09:00 - 18:00")
   */
  static getWorkingHoursForDay(
    dayOfWeek: number,
    providerSchedules: ProviderScheduleResponse[]
  ): string | null {
    const schedules = this.getSchedulesForDay(dayOfWeek, providerSchedules);

    if (schedules.length === 0) {
      return null;
    }

    // Si hay múltiples schedules, tomar el primero
    const schedule = schedules[0];
    return `${schedule.startTime.slice(0, 5)} - ${schedule.endTime.slice(0, 5)}`;
  }
}
