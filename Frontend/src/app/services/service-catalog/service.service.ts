import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ServiceResponse } from '../../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class ServiceService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/Services`;

  getAll(): Observable<ServiceResponse[]> {
    return this.http.get<ServiceResponse[]>(this.API_URL);
  }

  searchByName(name: string): Observable<ServiceResponse[]> {
    const params = new HttpParams().set('name', name);
    return this.http.get<ServiceResponse[]>(`${this.API_URL}/search`, { params });
  }

  getById(id: number): Observable<ServiceResponse> {
    return this.http.get<ServiceResponse>(`${this.API_URL}/${id}`);
  }
}
