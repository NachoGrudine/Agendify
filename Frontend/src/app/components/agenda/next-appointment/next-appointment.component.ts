import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Calendar, Clock, User } from 'lucide-angular';
import { AppointmentService } from '../../../services/appointment/appointment.service';
import { NextAppointmentResponse } from '../../../models/appointment.model';

@Component({
  selector: 'app-next-appointment',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './next-appointment.component.html',
  styleUrl: './next-appointment.component.css'
})
export class NextAppointmentComponent implements OnInit {
  private readonly appointmentService = inject(AppointmentService);

  readonly CalendarIcon = Calendar;
  readonly ClockIcon = Clock;
  readonly UserIcon = User;

  // Estado del componente
  nextAppointment = signal<NextAppointmentResponse | null>(null);
  isLoading = signal(false);
  hasError = signal(false);
  errorMessage = signal('');

  ngOnInit(): void {
    this.loadNextAppointment();
  }

  /**
   * Carga el pr�ximo turno programado
   */
  loadNextAppointment(): void {
    this.isLoading.set(true);
    this.hasError.set(false);
    this.errorMessage.set('');

    const currentDateTime = new Date();

    this.appointmentService.getNext(currentDateTime).subscribe({
      next: (response) => {
        // response puede ser NextAppointmentResponse o null (si no hay próximos turnos)
        this.nextAppointment.set(response);
        this.isLoading.set(false);
      },
      error: (error) => {
        // Solo errores reales del servidor llegan aquí (500, 503, etc.)
        this.hasError.set(true);
        this.errorMessage.set('Error al cargar próximo turno');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Formatea la fecha del turno en formato legible
   * Ejemplo: "Lunes, 10 de Febrero"
   */
  formatDay(dateString: string): string {
    const date = new Date(dateString);
    const options: Intl.DateTimeFormatOptions = {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    };
    return date.toLocaleDateString('es-ES', options);
  }

  /**
   * Formatea la hora en formato HH:MM
   */
  formatTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });
  }

  /**
   * Devuelve el texto a mostrar para el cliente
   * Si no hay nombre, solo muestra fecha y hora
   */
  getCustomerDisplay(): string {
    const appointment = this.nextAppointment();
    if (!appointment) return '';

    if (appointment.customerName && appointment.customerName !== 'Sin cliente asignado') {
      return appointment.customerName;
    }

    return `${this.formatDay(appointment.day)} - ${this.formatTime(appointment.startTime)}`;
  }

  /**
   * Indica si debe mostrar los detalles de hora cuando hay nombre de cliente
   */
  shouldShowTimeDetails(): boolean {
    const appointment = this.nextAppointment();
    if (!appointment) return false;
    return !!(appointment.customerName && appointment.customerName !== 'Sin cliente asignado');
  }
}
