import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateAppointmentDto, UpdateAppointmentDto, AppointmentResponse, NextAppointmentResponse } from '../../models/appointment.model';
import { DateTimeHelper } from '../../helpers/date-time.helper';

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

  /**
   * Obtener el próximo turno programado
   * @param currentDateTime Fecha y hora actual del sistema
   */
  getNext(currentDateTime: Date): Observable<NextAppointmentResponse> {
    // Usar toLocalISOString para enviar en hora local sin conversión UTC
    // Formato: "2026-02-06T11:12:47" (sin Z)
    const params = new HttpParams().set('currentDateTime', DateTimeHelper.toLocalISOString(currentDateTime));
    return this.http.get<NextAppointmentResponse>(`${this.API_URL}/next`, { params });
  }
}
