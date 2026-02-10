import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth/auth.service';
import { catchError, switchMap, filter, take, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  // Agregar access token a todas las requests (excepto auth endpoints)
  const isAuthEndpoint = req.url.includes('/auth/login') ||
                         req.url.includes('/auth/register') ||
                         req.url.includes('/auth/refresh');

  let clonedRequest = req;
  if (token && !isAuthEndpoint) {
    clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // Si es un error 401 y NO es del endpoint de auth
      if (error.status === 401 && !isAuthEndpoint) {
        // Si ya hay un refresh en progreso, esperar a que termine
        if (authService.isRefreshing()) {
          return authService.getRefreshTokenSubject().pipe(
            filter(token => token !== null),
            take(1),
            switchMap(token => {
              // Reintentar el request original con el nuevo token
              const retryRequest = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${token}`
                }
              });
              return next(retryRequest);
            })
          );
        } else {
          // Iniciar el proceso de refresh
          authService.setRefreshing(true);

          return authService.refreshToken().pipe(
            switchMap(response => {
              // Reintentar el request original con el nuevo token
              const retryRequest = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${response.access_token}`
                }
              });
              return next(retryRequest);
            }),
            catchError(refreshError => {
              // Si el refresh falla, hacer logout
              authService.logout();
              return throwError(() => refreshError);
            })
          );
        }
      }

      return throwError(() => error);
    })
  );
};

