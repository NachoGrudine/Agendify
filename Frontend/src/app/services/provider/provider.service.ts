import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProviderResponse } from '../../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class ProviderService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/Providers`;

  getAll(): Observable<ProviderResponse[]> {
    return this.http.get<ProviderResponse[]>(this.API_URL);
  }

  getById(id: number): Observable<ProviderResponse> {
    return this.http.get<ProviderResponse>(`${this.API_URL}/${id}`);
  }
}
