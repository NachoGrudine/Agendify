/**
 * Helper para extraer mensajes de error de las respuestas HTTP
 * Maneja diferentes formatos de error incluyendo ProblemDetails (RFC 7807)
 */
export class ErrorHelper {
  /**
   * Extrae el mensaje de error de una respuesta HTTP de error
   * @param error - El objeto de error de HttpClient
   * @param defaultMessage - Mensaje por defecto si no se encuentra ninguno
   * @returns El mensaje de error extraído
   */
  static extractErrorMessage(error: any, defaultMessage: string = 'Se produjo un error'): string {
    // Si el error tiene la estructura ProblemDetails (RFC 7807)
    if (error?.error?.detail) {
      return error.error.detail;
    }

    // Fallback a message si existe
    if (error?.error?.message) {
      return error.error.message;
    }

    // Si el error tiene un mensaje directo
    if (error?.message) {
      return error.message;
    }

    // Si el error es un string
    if (typeof error === 'string') {
      return error;
    }

    // Si error.error es un string
    if (typeof error?.error === 'string') {
      return error.error;
    }

    // Mensaje por defecto
    return defaultMessage;
  }

  /**
   * Determina si un error es de conflicto de horarios
   * @param error - El objeto de error o mensaje
   * @returns true si es un error de conflicto de horarios
   */
  static isScheduleConflictError(error: any): boolean {
    const message = typeof error === 'string' ? error : this.extractErrorMessage(error);
    const lowerMessage = message.toLowerCase();

    return lowerMessage.includes('turno asignado') ||
           lowerMessage.includes('conflicto') ||
           lowerMessage.includes('solapamiento') ||
           lowerMessage.includes('overlap') ||
           lowerMessage.includes('conflict');
  }

  /**
   * Formatea un mensaje de error de conflicto de horarios con información adicional
   * @param error - El objeto de error
   * @param startTime - Hora de inicio solicitada (formato HH:mm)
   * @param endTime - Hora de fin solicitada (formato HH:mm)
   * @returns Mensaje de error formateado
   */
  static formatScheduleConflictError(error: any, startTime?: string, endTime?: string): string {
    const baseMessage = this.extractErrorMessage(error, 'Error al procesar el turno');

    if (this.isScheduleConflictError(baseMessage) && startTime && endTime) {
      return `${baseMessage}\n\nHorario solicitado: ${startTime} - ${endTime}\n\nPor favor, selecciona otro horario disponible.`;
    }

    return baseMessage;
  }
}
