import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Eye, Plus, X } from 'lucide-angular';

@Component({
  selector: 'app-day-action-modal',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './day-action-modal.component.html',
  styleUrls: ['./day-action-modal.component.css']
})
export class DayActionModalComponent {
  @Input() selectedDate: Date | null = null;
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();
  @Output() viewDay = new EventEmitter<void>();
  @Output() newAppointment = new EventEmitter<void>();

  // Iconos
  readonly EyeIcon = Eye;
  readonly PlusIcon = Plus;
  readonly XIcon = X;

  // Getters (reactivos a cambios de @Input)
  get isPastDate(): boolean {
    if (!this.selectedDate) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const selected = new Date(this.selectedDate);
    selected.setHours(0, 0, 0, 0);
    return selected < today;
  }

  get formattedDate(): string {
    if (!this.selectedDate) return '';
    const date = new Date(this.selectedDate);
    const days = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
    const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
                    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

    const dayName = days[date.getDay()];
    const day = date.getDate();
    const month = months[date.getMonth()];

    return `${dayName}, ${day} de ${month}`;
  }

  onClose(): void {
    this.close.emit();
  }

  onViewDay(): void {
    this.viewDay.emit();
    this.close.emit();
  }

  onNewAppointment(): void {
    if (!this.isPastDate) {
      this.newAppointment.emit();
      this.close.emit();
    }
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }
}
