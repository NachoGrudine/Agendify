import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateAppointmentDto, UpdateAppointmentDto, AppointmentResponse } from '../../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/Appointments`;

  /**
   * Crear un nuevo appointment
   */
  create(dto: CreateAppointmentDto): Observable<AppointmentResponse> {
    return this.http.post<AppointmentResponse>(this.API_URL, dto);
  }

  /**
   * Obtener un appointment por ID
   */
  getById(id: number): Observable<AppointmentResponse> {
    return this.http.get<AppointmentResponse>(`${this.API_URL}/${id}`);
  }

  /**
   * Actualizar un appointment
   */
  update(id: number, dto: UpdateAppointmentDto): Observable<AppointmentResponse> {
    return this.http.put<AppointmentResponse>(`${this.API_URL}/${id}`, dto);
  }

  /**
   * Eliminar un appointment
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }
}
