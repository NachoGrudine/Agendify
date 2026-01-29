/**
 * Helper para manejar operaciones con fechas y horas
 * Evita problemas de timezone y centraliza la lógica de conversión
 */
export class DateTimeHelper {

  /**
   * Convierte una fecha a formato ISO pero en hora LOCAL (sin convertir a UTC)
   * Ejemplo: 2026-01-29 09:00 local → "2026-01-29T09:00:00" (sin Z al final)
   * Esto evita que el backend interprete la hora como UTC
   */
  static toLocalISOString(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
  }

  /**
   * Convierte un string de fecha a Date evitando problemas de timezone
   */
  static parseDate(dateStr: string): Date {
    try {
      // Si viene en formato ISO completo (con hora)
      if (dateStr.includes('T')) {
        return new Date(dateStr);
      }

      // Si viene solo la fecha (YYYY-MM-DD)
      const [year, month, day] = dateStr.split('-').map(Number);
      return new Date(year, month - 1, day);
    } catch (error) {
      return new Date();
    }
  }

  /**
   * Valida que una fecha sea válida
   */
  static isValidDate(date: Date | null): boolean {
    return date !== null && !isNaN(date.getTime());
  }

  /**
   * Formatea una fecha en formato YYYY-MM-DD
   */
  static formatDate(date: Date): string {
    if (!DateTimeHelper.isValidDate(date)) {
      const now = new Date();
      return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
    }
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  }

  /**
   * Formatea una fecha en formato legible en español
   */
  static formatDateSpanish(date: Date): string {
    const days = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
    const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
                    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
    return `${days[date.getDay()]}, ${date.getDate()} de ${months[date.getMonth()]}`;
  }

  /**
   * Convierte un TimeSpan (HH:mm:ss) a minutos
   */
  static timeSpanToMinutes(timeSpan: string): number {
    const parts = timeSpan.split(':');
    return parseInt(parts[0]) * 60 + parseInt(parts[1]);
  }

  /**
   * Crea un DateTime combinando una fecha base con horas y minutos
   */
  static createDateTime(baseDate: Date, hours: number, minutes: number): Date {
    const dateTime = new Date(baseDate);
    dateTime.setHours(hours, minutes, 0, 0);
    return dateTime;
  }

  /**
   * Parsea un string de tiempo "HH:mm" y retorna {hours, minutes}
   */
  static parseTime(timeStr: string): { hours: number; minutes: number } {
    const [hours, minutes] = timeStr.split(':').map(Number);
    return { hours, minutes };
  }

  /**
   * Formatea un Date a string de tiempo "HH:mm"
   */
  static formatTime(date: Date): string {
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes}`;
  }
}
