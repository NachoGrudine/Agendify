import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProviderResponse, CreateProviderDto, UpdateProviderDto } from '../../models/appointment.model';

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

  create(dto: CreateProviderDto): Observable<ProviderResponse> {
    return this.http.post<ProviderResponse>(this.API_URL, dto);
  }

  update(id: number, dto: UpdateProviderDto): Observable<ProviderResponse> {
    return this.http.put<ProviderResponse>(`${this.API_URL}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }
}
