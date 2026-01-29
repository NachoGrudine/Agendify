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
    // Usar formato YYYY-MM-DD para evitar problemas de timezone
    const formatDate = (d: Date) =>
      `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

    const params = new HttpParams()
      .set('startDate', formatDate(startDate))
      .set('endDate', formatDate(endDate));

    return this.http.get<CalendarDaySummaryDto[]>(`${this.apiUrl}/summary`, { params });
  }

  /**
   * Obtiene el detalle completo de un día específico con todos los turnos
   * @param date Fecha del día
   * @param page Número de página (1-based)
   * @param pageSize Cantidad de registros por página
   * @param filters Filtros opcionales (status, startTime, searchText)
   * @returns Detalle del día con lista de turnos paginados
   */
  getDayDetails(
    date: Date,
    page: number = 1,
    pageSize: number = 15,
    filters?: {
      status?: string;
      startTime?: string;
      searchText?: string;
    }
  ): Observable<DayDetailsDto> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (filters?.status) params = params.set('status', filters.status);
    if (filters?.startTime) params = params.set('startTime', filters.startTime);
    if (filters?.searchText) params = params.set('searchText', filters.searchText);

    // Usar formato local YYYY-MM-DD para evitar problemas de timezone
    const dateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    return this.http.get<DayDetailsDto>(`${this.apiUrl}/day/${dateStr}`, { params });
  }
}

