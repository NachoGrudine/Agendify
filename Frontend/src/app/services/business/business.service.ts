import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BusinessResponse, UpdateBusinessDto } from '../../models/business.model';

@Injectable({
  providedIn: 'root'
})
export class BusinessService {
  private readonly http = inject(HttpClient);
  private readonly API_URL = `${environment.apiUrl}/Business`;

  get(): Observable<BusinessResponse> {
    return this.http.get<BusinessResponse>(this.API_URL);
  }

  update(dto: UpdateBusinessDto): Observable<BusinessResponse> {
    return this.http.put<BusinessResponse>(this.API_URL, dto);
  }
}
