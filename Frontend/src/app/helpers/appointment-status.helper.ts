/**
 * Helper para formateo de estados de appointments
 */
export class AppointmentStatusHelper {

  private static readonly statusConfig: { [key: string]: { className: string; icon: string; label: string } } = {
    'confirmado': {
      className: 'status-confirmed',
      icon: 'CheckCircle',
      label: 'Confirmado'
    },
    'pendiente': {
      className: 'status-pending',
      icon: 'AlertCircle',
      label: 'Pendiente'
    },
    'completado': {
      className: 'status-completed',
      icon: 'CheckCircle',
      label: 'Completado'
    },
    'cancelado': {
      className: 'status-cancelled',
      icon: 'XCircle',
      label: 'Cancelado'
    }
  };

  /**
   * Obtiene la clase CSS para un estado
   */
  static getStatusClass(status: string): string {
    const statusLower = status.toLowerCase();
    return this.statusConfig[statusLower]?.className || 'status-default';
  }

  /**
   * Obtiene el nombre del icono para un estado
   */
  static getStatusIconName(status: string): string {
    const statusLower = status.toLowerCase();
    return this.statusConfig[statusLower]?.icon || 'AlertCircle';
  }

  /**
   * Obtiene el label humanizado del estado
   */
  static getStatusLabel(status: string): string {
    const statusLower = status.toLowerCase();
    return this.statusConfig[statusLower]?.label || status;
  }

  /**
   * Formatea minutos a formato "Xh Ym"
   */
  static formatMinutesToHours(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
  }
}
