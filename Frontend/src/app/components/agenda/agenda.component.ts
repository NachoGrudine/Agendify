import { Component, signal, OnInit, inject, ViewChild, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FullCalendarModule, FullCalendarComponent } from '@fullcalendar/angular';
import { CalendarOptions, EventClickArg, DateSelectArg, DatesSetArg } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import esLocale from '@fullcalendar/core/locales/es';
import { CalendarService } from '../../services/calendar/calendar.service';
import { CalendarDaySummaryDto } from '../../models/calendar.model';
import { LucideAngularModule, ChevronLeft, ChevronRight, CalendarDays, Hourglass, User } from 'lucide-angular';

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
  readonly CalendarIcon = CalendarDays;
  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;
  readonly HourglassIcon = Hourglass;
  readonly UserIcon = User;

  // Datos del calendario del mes actual
  calendarData = signal<Map<string, CalendarDaySummaryDto>>(new Map());
  isLoading = signal(false);

  // Para evitar llamadas duplicadas al mismo mes
  private currentLoadedMonth: string = '';

  // Control de scroll para cambiar de mes
  private scrollTimeout: any = null;
  private isScrolling = false;


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
   * Maneja el scroll del mouse sobre el calendario para cambiar de mes
   * Estilo Google Calendar
   */
  @HostListener('wheel', ['$event'])
  onWheel(event: WheelEvent): void {
    // Solo aplicar si el scroll es sobre el calendario
    const target = event.target as HTMLElement;
    if (!target.closest('.calendar-wrapper')) {
      return;
    }

    // Prevenir el scroll normal
    event.preventDefault();

    // Si ya hay un scroll en progreso, ignorar
    if (this.isScrolling) {
      return;
    }

    // Determinar dirección del scroll
    const deltaY = event.deltaY;

    // Threshold mínimo para considerar el scroll
    if (Math.abs(deltaY) < 10) {
      return;
    }

    // Marcar que estamos procesando un scroll
    this.isScrolling = true;

    // Limpiar timeout anterior si existe
    if (this.scrollTimeout) {
      clearTimeout(this.scrollTimeout);
    }

    // Cambiar de mes según la dirección
    if (deltaY > 0) {
      // Scroll hacia abajo = mes siguiente
      this.nextMonth();
    } else {
      // Scroll hacia arriba = mes anterior
      this.previousMonth();
    }

    // Debounce: permitir siguiente scroll después de un delay
    this.scrollTimeout = setTimeout(() => {
      this.isScrolling = false;
    }, 400); // 400ms de delay entre cambios de mes
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
   * Formatea los minutos de forma inteligente:
   * - Menos de 60 minutos: "45m"
   * - Horas exactas: "1H", "2H"
   * - Horas con minutos: "1:30", "2:45"
   */
  formatMinutes(totalMinutes: number): string {
    if (totalMinutes < 60) {
      return `${totalMinutes}m`;
    }

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (minutes === 0) {
      return `${hours}H`;
    }

    return `${hours}:${String(minutes).padStart(2, '0')}`;
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

    // Crear badges con los datos
    let badgesHtml = '<div class="day-badges-container" style="margin-top: 28px; position: relative;">';

    // Badge de turnos - celeste minimalista
    badgesHtml += `<div style="background: #e0f2fe; border-left: 3px solid #0ea5e9; padding: 4px 8px; border-radius: 4px; display: flex; align-items: center; gap: 4px; margin-bottom: 4px;">`;
    badgesHtml += `<svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="#0369a1" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="flex-shrink: 0;"><path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>`;
    badgesHtml += `<span style="color: #0369a1; font-size: 0.75rem; font-weight: 600; line-height: 1;">${dayData.appointmentsCount} Turnos</span>`;
    badgesHtml += `</div>`;

    // Badge de tiempo disponible - naranja o verde minimalista
    const isWarning = dayData.totalAvailableMinutes < 60;
    const bgColor = isWarning ? '#ffedd5' : '#d1fae5';
    const borderColor = isWarning ? '#fb923c' : '#34d399';
    const textColor = isWarning ? '#c2410c' : '#047857';
    const formattedTime = this.formatMinutes(dayData.totalAvailableMinutes);

    badgesHtml += `<div style="background: ${bgColor}; border-left: 3px solid ${borderColor}; padding: 4px 8px; border-radius: 4px; display: flex; align-items: center; gap: 4px;">`;
    badgesHtml += `<svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="${textColor}" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="flex-shrink: 0;"><path d="M5 22h14"/><path d="M5 2h14"/><path d="M17 22v-4.172a2 2 0 0 0-.586-1.414L12 12l-4.414 4.414A2 2 0 0 0 7 17.828V22"/><path d="M7 2v4.172a2 2 0 0 0 .586 1.414L12 12l4.414-4.414A2 2 0 0 0 17 6.172V2"/></svg>`;
    badgesHtml += `<span style="color: ${textColor}; font-size: 0.75rem; font-weight: 600; line-height: 1;">${formattedTime} libres</span>`;
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
