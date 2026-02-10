import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, BehaviorSubject } from 'rxjs';
import { LoginDto, AuthResponseDto, DecodedToken, RegisterDto, RefreshTokenDto } from '../../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly API_URL = `${environment.apiUrl}/auth`;
  private readonly ACCESS_TOKEN_KEY = 'agendify_access_token';
  private readonly REFRESH_TOKEN_KEY = 'agendify_refresh_token';

  // Signals para manejar el estado de autenticación
  public isAuthenticated = signal<boolean>(this.hasValidToken());
  public currentUser = signal<DecodedToken | null>(this.getDecodedToken());

  // Control de refresh token en progreso
  private refreshTokenInProgress = false;
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  login(credentials: LoginDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.API_URL}/login`, credentials).pipe(
      tap(response => this.handleAuthResponse(response)),
      catchError(error => throwError(() => error))
    );
  }

  register(registerData: RegisterDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.API_URL}/register`, registerData).pipe(
      tap(response => this.handleAuthResponse(response)),
      catchError(error => throwError(() => error))
    );
  }

  // ============================================
  // REFRESH TOKEN
  // ============================================
  refreshToken(): Observable<AuthResponseDto> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    const refreshDto: RefreshTokenDto = { refresh_token: refreshToken };

    return this.http.post<AuthResponseDto>(`${this.API_URL}/refresh`, refreshDto).pipe(
      tap(response => {
        this.handleAuthResponse(response);
        this.refreshTokenInProgress = false;
        this.refreshTokenSubject.next(response.access_token);
      }),
      catchError(error => {
        this.refreshTokenInProgress = false;
        this.logout();
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    this.isAuthenticated.set(false);
    this.currentUser.set(null);
    this.router.navigate(['/auth']);
  }

  // ============================================
  // MANEJO DE RESPUESTA DE AUTH
  // ============================================
  private handleAuthResponse(response: AuthResponseDto): void {
    this.saveTokens(response.access_token, response.refresh_token);
    this.isAuthenticated.set(true);
    this.currentUser.set(this.getDecodedToken());
  }

  // ============================================
  // TOKENS - GETTERS Y SETTERS
  // ============================================
  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  private saveTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  // Mantener compatibilidad con el interceptor existente
  getToken(): string | null {
    return this.getAccessToken();
  }

  private saveToken(token: string): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, token);
  }

  // ============================================
  // VALIDACIÓN Y DECODIFICACIÓN
  // ============================================
  private hasValidToken(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;

    const decoded = this.decodeToken(token);
    if (!decoded) return false;

    // Verificar si el token expira en más de 30 segundos
    const currentTime = Date.now() / 1000;
    return decoded.exp > (currentTime + 30);
  }

  getDecodedToken(): DecodedToken | null {
    const token = this.getAccessToken();
    if (!token) return null;
    return this.decodeToken(token);
  }

  getBusinessId(): number | null {
    const decoded = this.getDecodedToken();
    return decoded?.businessId ?? null;
  }

  getProviderId(): number | null {
    const decoded = this.getDecodedToken();
    return decoded?.providerId ?? null;
  }

  private decodeToken(token: string): DecodedToken | null {
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));

      return {
        userId: parseInt(decoded.UserId || decoded.userId || '0'),
        businessId: parseInt(decoded.BusinessId || decoded.businessId || '0'),
        providerId: parseInt(decoded.ProviderId || decoded.providerId || '0'),
        email: decoded.Email || decoded.email || '',
        exp: decoded.exp
      };
    } catch (error) {
      return null;
    }
  }

  // ============================================
  // HELPERS PARA OBTENER DATOS DEL TOKEN
  // ============================================
  getUserId(): number | null {
    return this.currentUser()?.userId ?? null;
  }

  getEmail(): string | null {
    return this.currentUser()?.email ?? null;
  }

  // ============================================
  // CONTROL DE REFRESH EN PROGRESO
  // ============================================
  isRefreshing(): boolean {
    return this.refreshTokenInProgress;
  }

  setRefreshing(value: boolean): void {
    this.refreshTokenInProgress = value;
  }

  getRefreshTokenSubject(): BehaviorSubject<string | null> {
    return this.refreshTokenSubject;
  }
}
