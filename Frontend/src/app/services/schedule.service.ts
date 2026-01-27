import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BulkUpdateScheduleDto, ProviderScheduleResponse } from '../models/schedule.model';

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {
  private readonly http = inject(HttpClient);
private readonly API_URL = `${environment.apiUrl}/ProviderSchedules`;

  // Obtener horarios del usuario logueado
  getMySchedules(): Observable<ProviderScheduleResponse[]> {
    return this.http.get<ProviderScheduleResponse[]>(`${this.API_URL}/me`);
  }

  // Actualizar horarios del usuario logueado
  bulkUpdateMySchedules(dto: BulkUpdateScheduleDto): Observable<ProviderScheduleResponse[]> {
    return this.http.put<ProviderScheduleResponse[]>(`${this.API_URL}/me/bulk-update`, dto);
  }

  // Métodos legacy (por si se necesitan en el futuro)
  getByProvider(providerId: number): Observable<ProviderScheduleResponse[]> {
    return this.http.get<ProviderScheduleResponse[]>(`${this.API_URL}/provider/${providerId}`);
  }

  bulkUpdate(providerId: number, dto: BulkUpdateScheduleDto): Observable<ProviderScheduleResponse[]> {
    return this.http.put<ProviderScheduleResponse[]>(`${this.API_URL}/provider/${providerId}/bulk`, dto);
  }
}
