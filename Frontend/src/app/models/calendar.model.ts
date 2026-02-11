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
  appointmentsTrend: number; // Diferencia con el día anterior (ej: +2, -3, 0)
  totalScheduledMinutes: number;
  totalOccupiedMinutes: number;
  appointments: AppointmentDetailDto[];
  currentPage: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
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
  notes?: string;
}

