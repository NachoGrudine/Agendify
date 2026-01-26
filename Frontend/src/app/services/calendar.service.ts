import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CalendarDaySummaryDto, DayDetailsDto } from '../models/calendar.model';

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/calendar`;

  /**
   * Obtiene el resumen del calendario para un rango de fechas (mes completo)
   * @param startDate Fecha de inicio del mes
   * @param endDate Fecha de fin del mes
   * @returns Lista de resúmenes por día con turnos, tiempo ocupado y disponible
   */
  getCalendarSummary(startDate: Date, endDate: Date): Observable<CalendarDaySummaryDto[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<CalendarDaySummaryDto[]>(`${this.apiUrl}/summary`, { params });
  }

  /**
   * Obtiene el detalle completo de un día específico con todos los turnos
   * @param date Fecha del día
   * @param filters Filtros opcionales (status, startTime, customerName, providerName)
   * @returns Detalle del día con lista de turnos
   */
  getDayDetails(
    date: Date,
    filters?: {
      status?: string;
      startTime?: string;
      customerName?: string;
      providerName?: string;
    }
  ): Observable<DayDetailsDto> {
    let params = new HttpParams();

    if (filters?.status) params = params.set('status', filters.status);
    if (filters?.startTime) params = params.set('startTime', filters.startTime);
    if (filters?.customerName) params = params.set('customerName', filters.customerName);
    if (filters?.providerName) params = params.set('providerName', filters.providerName);

    const dateStr = date.toISOString().split('T')[0];
    return this.http.get<DayDetailsDto>(`${this.apiUrl}/day/${dateStr}`, { params });
  }
}

