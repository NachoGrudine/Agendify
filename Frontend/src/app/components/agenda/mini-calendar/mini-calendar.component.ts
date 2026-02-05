import { Component, signal, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, ChevronLeft, ChevronRight } from 'lucide-angular';

@Component({
  selector: 'app-mini-calendar',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './mini-calendar.component.html',
  styleUrl: './mini-calendar.component.css'
})
export class MiniCalendarComponent {
  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;

  weekDays = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];

  private currentDate = new Date();
  private selectedDate = new Date();

  currentMonthYear = '';
  daysInMonth = signal<any[]>([]);

  // Output para notificar cuando se selecciona una fecha
  dateSelected = output<Date>();

  constructor() {
    this.updateCalendar();
  }

  /**
   * Método público para cambiar el mes desde el componente padre
   */
  goToDate(date: Date): void {
    this.currentDate = new Date(date.getFullYear(), date.getMonth(), 1);
    this.updateCalendar();
  }

  updateCalendar(): void {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth();

    // Actualizar el display del mes
    const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
                    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
    this.currentMonthYear = `${months[month]} ${year}`;

    // Obtener primer y último día del mes
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);

    // Obtener el día de la semana del primer día (0 = domingo)
    const startingDayOfWeek = firstDay.getDay();

    // Calcular días del mes anterior que se muestran
    const prevMonthLastDay = new Date(year, month, 0).getDate();
    const prevMonthDays = startingDayOfWeek;

    // Calcular días del siguiente mes
    const totalCells = 42; // 6 semanas * 7 días
    const daysInCurrentMonth = lastDay.getDate();
    const nextMonthDays = totalCells - prevMonthDays - daysInCurrentMonth;

    const days: any[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Días del mes anterior
    for (let i = prevMonthDays - 1; i >= 0; i--) {
      const day = prevMonthLastDay - i;
      const date = new Date(year, month - 1, day);
      days.push({
        dayNumber: day,
        date: date,
        isCurrentMonth: false,
        isToday: false,
        isSelected: this.isSameDay(date, this.selectedDate)
      });
    }

    // Días del mes actual
    for (let day = 1; day <= daysInCurrentMonth; day++) {
      const date = new Date(year, month, day);
      days.push({
        dayNumber: day,
        date: date,
        isCurrentMonth: true,
        isToday: this.isSameDay(date, today),
        isSelected: this.isSameDay(date, this.selectedDate)
      });
    }

    // Días del siguiente mes
    for (let day = 1; day <= nextMonthDays; day++) {
      const date = new Date(year, month + 1, day);
      days.push({
        dayNumber: day,
        date: date,
        isCurrentMonth: false,
        isToday: false,
        isSelected: this.isSameDay(date, this.selectedDate)
      });
    }

    this.daysInMonth.set(days);
  }

  private isSameDay(date1: Date, date2: Date): boolean {
    return date1.getFullYear() === date2.getFullYear() &&
           date1.getMonth() === date2.getMonth() &&
           date1.getDate() === date2.getDate();
  }

  previousMonth(): void {
    this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() - 1, 1);
    this.updateCalendar();
  }

  nextMonth(): void {
    this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 1);
    this.updateCalendar();
  }

  selectDate(day: any): void {
    if (!day.isCurrentMonth) {
      // Si se selecciona un día de otro mes, navegar a ese mes
      if (day.date < new Date(this.currentDate.getFullYear(), this.currentDate.getMonth(), 1)) {
        this.previousMonth();
      } else {
        this.nextMonth();
      }
    }

    this.selectedDate = day.date;
    this.updateCalendar();
    this.dateSelected.emit(day.date);
  }
}
