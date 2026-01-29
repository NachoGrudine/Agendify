import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BulkUpdateScheduleDto, ProviderScheduleResponse } from '../../models/schedule.model';

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/ProviderSchedules`;

  getMySchedules(): Observable<ProviderScheduleResponse[]> {
    return this.http.get<ProviderScheduleResponse[]>(`${this.API_URL}/me`);
  }

  getProviderSchedules(providerId: number): Observable<ProviderScheduleResponse[]> {
    return this.http.get<ProviderScheduleResponse[]>(`${this.API_URL}/provider/${providerId}`);
  }

  bulkUpdateMySchedules(dto: BulkUpdateScheduleDto): Observable<ProviderScheduleResponse[]> {
    return this.http.put<ProviderScheduleResponse[]>(`${this.API_URL}/me/bulk-update`, dto);
  }
}
