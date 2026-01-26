export interface CalendarDaySummaryDto {
  date: string;
  appointmentsCount: number;
  totalScheduledMinutes: number;  // Tiempo total que los providers trabajan ese día
  totalOccupiedMinutes: number;   // Tiempo ocupado por appointments
  totalAvailableMinutes: number;  // TotalScheduled - TotalOccupied
}

export interface DayDetailsDto {
  date: string;
  dayOfWeek: string;
  totalAppointments: number;
  totalScheduledMinutes: number;
  totalOccupiedMinutes: number;
  appointments: AppointmentDetailDto[];
}

export interface AppointmentDetailDto {
  id: number;
  customerName: string;
  providerName: string;
  serviceName?: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  status: string;
}

