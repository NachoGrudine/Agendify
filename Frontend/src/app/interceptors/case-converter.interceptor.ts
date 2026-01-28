import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';

/**
 * Interceptor que convierte automáticamente:
 * - REQUEST: camelCase (frontend) → snake_case (backend)
 * - RESPONSE: snake_case (backend) → camelCase (frontend)
 *
 * Esto mantiene las convenciones de cada lado:
 * - Frontend: camelCase (JavaScript/TypeScript)
 * - Backend: snake_case (C# con JsonNamingPolicy.SnakeCaseLower)
 */
export const caseConverterInterceptor: HttpInterceptorFn = (req, next) => {
  // Convertir el body de la request de camelCase a snake_case
  let modifiedReq = req;

  if (req.body && typeof req.body === 'object') {
    const snakeCaseBody = convertKeysToSnakeCase(req.body);
    modifiedReq = req.clone({ body: snakeCaseBody });
  }

  // Procesar la respuesta y convertir de snake_case a camelCase
  return next(modifiedReq).pipe(
    map(event => {
      if (event instanceof HttpResponse && event.body) {
        return event.clone({ body: convertKeysToCamelCase(event.body) });
      }
      return event;
    })
  );
};

/**
 * Convierte las keys de un objeto de camelCase a snake_case recursivamente
 */
function convertKeysToSnakeCase(obj: any): any {
  if (obj === null || obj === undefined) {
    return obj;
  }

  if (Array.isArray(obj)) {
    return obj.map(item => convertKeysToSnakeCase(item));
  }

  if (typeof obj !== 'object') {
    return obj;
  }

  if (obj instanceof Date || obj instanceof File || obj instanceof Blob) {
    return obj;
  }

  const converted: any = {};

  for (const key in obj) {
    if (obj.hasOwnProperty(key)) {
      const snakeKey = camelToSnake(key);
      converted[snakeKey] = convertKeysToSnakeCase(obj[key]);
    }
  }

  return converted;
}

/**
 * Convierte las keys de un objeto de snake_case a camelCase recursivamente
 */
function convertKeysToCamelCase(obj: any): any {
  if (obj === null || obj === undefined) {
    return obj;
  }

  if (Array.isArray(obj)) {
    return obj.map(item => convertKeysToCamelCase(item));
  }

  if (typeof obj !== 'object') {
    return obj;
  }

  if (obj instanceof Date) {
    return obj;
  }

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
 * Convierte una string de camelCase a snake_case
 */
function camelToSnake(str: string): string {
  return str.replace(/[A-Z]/g, letter => `_${letter.toLowerCase()}`);
}

/**
 * Convierte una string de snake_case a camelCase
 */
function snakeToCamel(str: string): string {
  return str.replace(/_([a-z])/g, (_, letter) => letter.toUpperCase());
}
