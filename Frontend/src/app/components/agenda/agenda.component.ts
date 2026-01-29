import { Component, signal, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FullCalendarModule, FullCalendarComponent } from '@fullcalendar/angular';
import { CalendarOptions, EventClickArg, DateSelectArg, DatesSetArg } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import esLocale from '@fullcalendar/core/locales/es';
import { CalendarService } from '../../services/calendar/calendar.service';
import { CalendarDaySummaryDto } from '../../models/calendar.model';
import { LucideAngularModule, ChevronLeft, ChevronRight, Bell } from 'lucide-angular';

@Component({
  selector: 'app-agenda',
  standalone: true,
  imports: [CommonModule, FullCalendarModule, LucideAngularModule],
  templateUrl: './agenda.component.html',
  styleUrls: ['./agenda.component.css']
})
export class AgendaComponent implements OnInit {
  private readonly calendarService = inject(CalendarService);
  private readonly router = inject(Router);
  @ViewChild('calendar') calendarComponent!: FullCalendarComponent;

  // Iconos Lucide
  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;
  readonly BellIcon = Bell;

  // Datos del calendario del mes actual
  calendarData = signal<Map<string, CalendarDaySummaryDto>>(new Map());
  isLoading = signal(false);

  // Para evitar llamadas duplicadas al mismo mes
  private currentLoadedMonth: string = '';

  // Datos del usuario (temporal - después vendrá del servicio de auth)
  userName = 'Admin User';
  userRole = 'Gerente';
  userInitials = 'AU';

  // Notificaciones
  hasNotifications = signal(true);
  notificationCount = signal(3);

  // Mes y año actual para el selector
  currentMonthYear = '';
  private currentDate = new Date();

  calendarOptions = signal<CalendarOptions>({
    plugins: [dayGridPlugin, interactionPlugin],
    initialView: 'dayGridMonth',
    locale: esLocale,
    headerToolbar: false, // Ocultamos el header por defecto de FullCalendar
    buttonText: {
      today: 'Hoy'
    },
    height: 'auto',
    contentHeight: 500, // Reducido de 480 a 380 para evitar scroll
    fixedWeekCount: false,
    showNonCurrentDates: true,
    editable: false,
    selectable: true,
    selectMirror: true,
    dayMaxEvents: 3,
    weekends: true,
    select: this.handleDateSelect.bind(this),
    eventClick: this.handleEventClick.bind(this),
    datesSet: this.handleDatesSet.bind(this),
    dayCellContent: this.renderDayCell.bind(this),
    events: []
  });

  ngOnInit(): void {
    this.updateMonthYearDisplay();
  }

  /**
   * Actualiza el display del mes y año actual
   */
  updateMonthYearDisplay(): void {
    const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
                    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
    this.currentMonthYear = `${months[this.currentDate.getMonth()]} ${this.currentDate.getFullYear()}`;
  }

  /**
   * Navegar al mes anterior
   */
  previousMonth(): void {
    if (this.calendarComponent) {
      const calendarApi = this.calendarComponent.getApi();
      calendarApi.prev();
      this.currentDate = calendarApi.getDate();
      this.updateMonthYearDisplay();
    }
  }

  /**
   * Navegar al mes siguiente
   */
  nextMonth(): void {
    if (this.calendarComponent) {
      const calendarApi = this.calendarComponent.getApi();
      calendarApi.next();
      this.currentDate = calendarApi.getDate();
      this.updateMonthYearDisplay();
    }
  }

  /**
   * Se ejecuta cuando cambia el mes en el calendario o cuando se inicializa
   */
  handleDatesSet(arg: DatesSetArg): void {
    // Obtener el mes actual visible (usamos el punto medio del rango visible para determinar el mes)
    const midPoint = new Date((arg.start.getTime() + arg.end.getTime()) / 2);
    const year = midPoint.getFullYear();
    const month = midPoint.getMonth();
    const monthKey = `${year}-${month}`;

    // Evitar recargar si ya tenemos los datos de este mes
    if (this.currentLoadedMonth === monthKey) {
      return;
    }

    this.currentLoadedMonth = monthKey;
    this.loadMonthData(year, month);
  }

  /**
   * Carga los datos del mes desde el backend
   */
  loadMonthData(year: number, month: number): void {
    this.isLoading.set(true);

    // Calcular primer y último día del mes
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);

    this.calendarService.getCalendarSummary(firstDay, lastDay).subscribe({
      next: (data) => {
        // Crear un Map con la fecha como key para acceso rápido
        const dataMap = new Map<string, CalendarDaySummaryDto>();
        data.forEach(day => {
          // Parsear la fecha del backend y usar formato local
          const d = new Date(day.date);
          const dateKey = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
          dataMap.set(dateKey, day);
        });

        this.calendarData.set(dataMap);
        this.isLoading.set(false);

        // FORZAR RE-RENDERIZADO del calendario después de cargar los datos
        this.forceCalendarRerender();
      },
      error: (error) => {
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Fuerza el re-renderizado del calendario para mostrar los nuevos datos
   */
  private forceCalendarRerender(): void {
    setTimeout(() => {
      if (this.calendarComponent) {
        const calendarApi = this.calendarComponent.getApi();
        // Usamos rerenderEvents y luego render para forzar actualización completa
        calendarApi.render();
      }
    }, 0);
  }

  /**
   * Personaliza el contenido de cada celda del día
   */
  renderDayCell(arg: any): any {
    // Usar formato local para evitar problemas de timezone
    const d = arg.date;
    const dateStr = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    const dayData = this.calendarData().get(dateStr);

    // Si no hay datos o ambos valores son 0, solo mostrar el número
    if (!dayData || (dayData.appointmentsCount === 0 && dayData.totalAvailableMinutes === 0)) {
      return { html: `<div class="fc-daygrid-day-number">${arg.dayNumberText}</div>` };
    }

    // SVG de iconos Lucide (Users y Clock)
    const usersIconSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`;
    const clockIconSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>`;

    // Crear badges con los datos
    let badgesHtml = '<div class="day-badges-container">';

    // Badge de turnos
    badgesHtml += `<div class="badge badge-turnos">`;
    badgesHtml += `<span class="badge-icon">${usersIconSvg}</span>`;
    badgesHtml += `<span class="badge-text">${dayData.appointmentsCount} Turnos</span>`;
    badgesHtml += `</div>`;

    // Badge de tiempo disponible
    badgesHtml += `<div class="badge badge-disponible">`;
    badgesHtml += `<span class="badge-icon">${clockIconSvg}</span>`;
    badgesHtml += `<span class="badge-text">${dayData.totalAvailableMinutes}m libres</span>`;
    badgesHtml += `</div>`;

    badgesHtml += '</div>';

    return {
      html: `
        <div class="custom-day-cell">
          <div class="day-number-wrapper">
            <span class="fc-daygrid-day-number">${arg.dayNumberText}</span>
          </div>
          ${badgesHtml}
        </div>
      `
    };
  }

  handleDateSelect(selectInfo: DateSelectArg): void {
    // Navegar directamente a la vista de detalle del día
    // Usar formato local para evitar problemas de timezone
    const d = selectInfo.start;
    const dateStr = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    this.router.navigate(['/dashboard/agenda/dia'], {
      queryParams: { date: dateStr }
    });
  }

  handleEventClick(clickInfo: EventClickArg): void {
    // Cuando se hace clic en un evento, también ir al día completo
    const eventStart = clickInfo.event.start;
    if (eventStart) {
      // Usar formato local para evitar problemas de timezone
      const dateStr = `${eventStart.getFullYear()}-${String(eventStart.getMonth() + 1).padStart(2, '0')}-${String(eventStart.getDate()).padStart(2, '0')}`;
      this.router.navigate(['/dashboard/agenda/dia'], {
        queryParams: { date: dateStr }
      });
    }
  }
}
