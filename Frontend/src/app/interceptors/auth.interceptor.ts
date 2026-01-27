import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  console.log('🔑 AuthInterceptor - URL:', req.url);
  console.log('🔑 AuthInterceptor - Token presente:', !!token);

  // Si hay token, clonamos la request y agregamos el header Authorization
  if (token) {
    console.log('🔑 AuthInterceptor - Agregando token Bearer');
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(clonedRequest);
  }

  console.warn('⚠️ AuthInterceptor - Sin token, request sin autenticación');
  return next(req);
};

