import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CustomerResponse, CreateCustomerDto, UpdateCustomerDto } from '../../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/Customers`;

  getAll(): Observable<CustomerResponse[]> {
    return this.http.get<CustomerResponse[]>(this.API_URL);
  }

  searchByName(name: string): Observable<CustomerResponse[]> {
    const params = new HttpParams().set('name', name);
    return this.http.get<CustomerResponse[]>(`${this.API_URL}/search`, { params });
  }

  getById(id: number): Observable<CustomerResponse> {
    return this.http.get<CustomerResponse>(`${this.API_URL}/${id}`);
  }

  create(dto: CreateCustomerDto): Observable<CustomerResponse> {
    return this.http.post<CustomerResponse>(this.API_URL, dto);
  }

  update(id: number, dto: UpdateCustomerDto): Observable<CustomerResponse> {
    return this.http.put<CustomerResponse>(`${this.API_URL}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }
}
