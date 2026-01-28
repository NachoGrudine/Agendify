import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateAppointmentDto, AppointmentResponse } from '../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/Appointments`;

  create(dto: CreateAppointmentDto): Observable<AppointmentResponse> {
    return this.http.post<AppointmentResponse>(this.API_URL, dto);
  }

  getById(id: number): Observable<AppointmentResponse> {
    return this.http.get<AppointmentResponse>(`${this.API_URL}/${id}`);
  }

  getAll(): Observable<AppointmentResponse[]> {
    return this.http.get<AppointmentResponse[]>(this.API_URL);
  }
}
