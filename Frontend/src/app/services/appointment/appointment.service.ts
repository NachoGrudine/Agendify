import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
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
   * @returns Observable con el próximo turno o null si no hay ninguno (404 es tratado como caso normal)
   */
  getNext(currentDateTime: Date): Observable<NextAppointmentResponse | null> {
    // Usar toLocalISOString para enviar en hora local sin conversión UTC
    // Formato: "2026-02-06T11:12:47" (sin Z)
    const params = new HttpParams().set('currentDateTime', DateTimeHelper.toLocalISOString(currentDateTime));
    return this.http.get<NextAppointmentResponse>(`${this.API_URL}/next`, { params }).pipe(
      catchError((error) => {
        // 404 significa que no hay próximos turnos, no es un error
        if (error.status === 404) {
          return of(null);
        }
        // Otros errores se propagan
        return throwError(() => error);
      })
    );
  }
}
