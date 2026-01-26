import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';

/**
 * Interceptor que convierte automáticamente las respuestas del backend
 * de snake_case a camelCase para mantener las convenciones de JavaScript/TypeScript
 */
export const caseConverterInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (event instanceof HttpResponse && event.body) {
        return event.clone({ body: convertKeysToCamelCase(event.body) });
      }
      return event;
    })
  );
};

/**
 * Convierte las keys de un objeto de snake_case a camelCase recursivamente
 */
function convertKeysToCamelCase(obj: any): any {
  if (obj === null || obj === undefined) {
    return obj;
  }

  // Si es un array, procesamos cada elemento
  if (Array.isArray(obj)) {
    return obj.map(item => convertKeysToCamelCase(item));
  }

  // Si no es un objeto, lo retornamos tal cual
  if (typeof obj !== 'object') {
    return obj;
  }

  // Si es una instancia de Date u otro objeto especial, lo retornamos sin modificar
  if (obj instanceof Date) {
    return obj;
  }

  // Convertimos las keys del objeto
  const converted: any = {};

  for (const key in obj) {
    if (obj.hasOwnProperty(key)) {
      const camelKey = snakeToCamel(key);
      converted[camelKey] = convertKeysToCamelCase(obj[key]);
    }
  }

  return converted;
}

/**
 * Convierte una string de snake_case a camelCase
 */
function snakeToCamel(str: string): string {
  return str.replace(/_([a-z])/g, (_, letter) => letter.toUpperCase());
}
