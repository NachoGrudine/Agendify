import { DateTimeHelper } from '../helpers/date-time.helper';

/**
 * Builder para construir DTOs de appointments
 * Centraliza la lógica de construcción y evita repetición
 */
export class AppointmentDTOBuilder {

  /**
   * Construye un DTO para crear un appointment
   */
  static buildCreateDTO(
    formValue: any,
    baseDate: Date
  ): any {
    const { startTime, endTime } = this.parseTimes(formValue, baseDate);

    const dto: any = {
      providerId: formValue.providerId,
      startTime: DateTimeHelper.toLocalISOString(startTime),
      endTime: DateTimeHelper.toLocalISOString(endTime),
      notes: formValue.notes || null
    };

    // Customer: ID o Name
    if (formValue.customerId) {
      dto.customerId = formValue.customerId;
    } else if (formValue.customerSearch && !formValue.customerId) {
      dto.customerName = formValue.customerSearch;
    }

    // Service: ID o Name
    if (formValue.serviceId) {
      dto.serviceId = formValue.serviceId;
    } else if (formValue.serviceSearch && !formValue.serviceId) {
      dto.serviceName = formValue.serviceSearch;
    }

    return dto;
  }

  /**
   * Construye un DTO para actualizar un appointment
   */
  static buildUpdateDTO(
    formValue: any,
    baseDate: Date,
    status: string
  ): any {
    const { startTime, endTime } = this.parseTimes(formValue, baseDate);

    const dto: any = {
      providerId: formValue.providerId,
      startTime: DateTimeHelper.toLocalISOString(startTime),
      endTime: DateTimeHelper.toLocalISOString(endTime),
      status: status,
      notes: formValue.notes || null
    };

    // Customer: ID o Name
    if (formValue.customerId) {
      dto.customerId = formValue.customerId;
      dto.customerName = null;
    } else if (formValue.customerSearch) {
      dto.customerId = null;
      dto.customerName = formValue.customerSearch;
    }

    // Service: ID o Name
    if (formValue.serviceId) {
      dto.serviceId = formValue.serviceId;
      dto.serviceName = null;
    } else if (formValue.serviceSearch) {
      dto.serviceId = null;
      dto.serviceName = formValue.serviceSearch;
    }

    return dto;
  }

  /**
   * Parsea los tiempos del formulario y retorna objetos Date
   */
  private static parseTimes(
    formValue: any, baseDate: Date): { startTime: Date; endTime: Date } {
    const startTimeStr = formValue.startTime; // "HH:mm"
    const endTimeStr = formValue.endTime;     // "HH:mm"

    const { hours: startHour, minutes: startMinute } = DateTimeHelper.parseTime(startTimeStr);
    const { hours: endHour, minutes: endMinute } = DateTimeHelper.parseTime(endTimeStr);

    const startTime = DateTimeHelper.createDateTime(baseDate, startHour, startMinute);
    const endTime = DateTimeHelper.createDateTime(baseDate, endHour, endMinute);

    return { startTime, endTime };
  }

  /**
   * Valida que las horas sean coherentes
   */
  static validateTimes(startTime: Date, endTime: Date): { valid: boolean; error?: string } {
    if (startTime >= endTime) {
      return {
        valid: false,
        error: 'La hora de inicio debe ser menor a la hora de fin'
      };
    }

    return { valid: true };
  }
}
