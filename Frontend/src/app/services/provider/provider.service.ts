import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ProviderResponse } from '../../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class ProviderService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/Providers`;

  getAll(): Observable<ProviderResponse[]> {
    console.log('🔍 Llamando a getAll providers:', this.API_URL);
    return this.http.get<ProviderResponse[]>(this.API_URL).pipe(
      tap(response => console.log('📦 Respuesta RAW del backend:', response))
    );
  }

  getById(id: number): Observable<ProviderResponse> {
    return this.http.get<ProviderResponse>(`${this.API_URL}/${id}`);
  }
}
