import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { LoginDto, AuthResponseDto, DecodedToken, RegisterDto } from '../models/auth.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly API_URL = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'agendify_token';

  // Signals para manejar el estado de autenticación
  public isAuthenticated = signal<boolean>(this.hasToken());
  public currentUser = signal<DecodedToken | null>(this.getDecodedToken());

  login(credentials: LoginDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.API_URL}/login`, credentials).pipe(
      tap(response => {
        this.saveToken(response.token);
        this.isAuthenticated.set(true);
        this.currentUser.set(this.getDecodedToken());
      }),
      catchError(error => {
        console.error('Error en login:', error);
        return throwError(() => error);
      })
    );
  }

  register(registerData: RegisterDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.API_URL}/register`, registerData).pipe(
      tap(response => {
        this.saveToken(response.token);
        this.isAuthenticated.set(true);
        this.currentUser.set(this.getDecodedToken());
      }),
      catchError(error => {
        console.error('Error en registro:', error);
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.isAuthenticated.set(false);
    this.currentUser.set(null);
    this.router.navigate(['/auth']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  private hasToken(): boolean {
    const token = this.getToken();
    if (!token) return false;

    // Verificar si el token no está expirado
    const decoded = this.decodeToken(token);
    if (!decoded) return false;

    const currentTime = Date.now() / 1000;
    return decoded.exp > currentTime;
  }

  getDecodedToken(): DecodedToken | null {
    const token = this.getToken();
    if (!token) return null;
    return this.decodeToken(token);
  }

  getBusinessId(): number | null {
    const decoded = this.getDecodedToken();
    return decoded?.businessId ?? null;
  }

  private decodeToken(token: string): DecodedToken | null {
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));

      return {
        userId: parseInt(decoded.nameid || decoded.userId),
        businessId: parseInt(decoded.businessId),
        email: decoded.email,
        exp: decoded.exp
      };
    } catch (error) {
      console.error('Error decodificando token:', error);
      return null;
    }
  }
}
