import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, ArrowLeft, Calendar, Clock, Users, Filter, Plus, Eye, X, CheckCircle, XCircle, AlertCircle } from 'lucide-angular';
import { CalendarService } from '../../../services/calendar.service';
import { ProviderService } from '../../../services/provider.service';
import { DayDetailsDto } from '../../../models/calendar.model';
import { ProviderResponse } from '../../../models/appointment.model';

@Component({
  selector: 'app-day-detail',
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './day-detail.html',
  styleUrl: './day-detail.css',
})
export class DayDetailComponent implements OnInit {
  private readonly calendarService = inject(CalendarService);
  private readonly providerService = inject(ProviderService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  // Exponer Math para el template
  readonly Math = Math;

  // Iconos
  readonly ArrowLeftIcon = ArrowLeft;
  readonly CalendarIcon = Calendar;
  readonly ClockIcon = Clock;
  readonly FilterIcon = Filter;
  readonly PlusIcon = Plus;
  readonly EyeIcon = Eye;
  readonly XIcon = X;
  readonly CheckCircleIcon = CheckCircle;
  readonly XCircleIcon = XCircle;
  readonly AlertCircleIcon = AlertCircle;
  readonly Users = Users;

  // Datos principales
  dayDetails = signal<DayDetailsDto | null>(null);
  providers = signal<ProviderResponse[]>([]);
  isLoading = signal(false);
  selectedDate: Date | null = null;

  // Paginación
  currentPage = signal(1);
  pageSize = signal(15);
  pageSizeOptions = [5, 10, 15, 25, 50];

  // Filtros
  showFilters = signal(false);
  searchText = ''; // Campo de búsqueda general (busca en cliente, servicio y profesional)
  filters = {
    status: '',
    startTime: ''
  };

  // Lista de estados disponibles
  availableStatuses = [
    { value: '', label: 'Todos los Estados' },
    { value: 'Pending', label: 'Pendiente' },
    { value: 'Confirmed', label: 'Confirmado' },
    { value: 'Completed', label: 'Completado' },
    { value: 'Cancelled', label: 'Cancelado' }
  ];

  ngOnInit(): void {
    // Obtener la fecha de los parámetros de la ruta
    this.route.queryParams.subscribe(params => {
      if (params['date']) {
        // Parsear la fecha en formato YYYY-MM-DD como hora LOCAL, no UTC
        // Esto evita el problema de timezone donde se resta un día
        const dateStr = params['date'];
        const [year, month, day] = dateStr.split('-').map(Number);
        this.selectedDate = new Date(year, month - 1, day);


        this.loadProviders();
        this.loadDayDetails();
      } else {
        // Si no hay fecha, volver al calendario
        this.router.navigate(['/dashboard/agenda']);
      }
    });
  }

  loadProviders(): void {
    this.providerService.getAll().subscribe({
      next: (providers) => {
        this.providers.set(providers);
      },
      error: (error) => {
        // Error al cargar providers
      }
    });
  }

  loadDayDetails(): void {
    if (!this.selectedDate) return;

    this.isLoading.set(true);

    const filterParams = {
      status: this.filters.status || undefined,
      startTime: this.filters.startTime || undefined,
      searchText: this.searchText || undefined
    };

    this.calendarService.getDayDetails(
      this.selectedDate,
      this.currentPage(),
      this.pageSize(),
      filterParams
    ).subscribe({
      next: (details) => {
        this.dayDetails.set(details);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  changePageSize(newSize: number): void {
    this.pageSize.set(newSize);
    this.currentPage.set(1); // Resetear a la primera página al cambiar el tamaño
    this.loadDayDetails();
  }

  applyFilters(): void {
    this.currentPage.set(1); // Resetear a la primera página al aplicar filtros
    this.loadDayDetails();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  clearFilters(): void {
    // Limpiar campo de búsqueda
    this.searchText = '';

    // Limpiar filtros
    this.filters = {
      status: '',
      startTime: ''
    };

    // Cerrar el panel de filtros
    this.showFilters.set(false);

    // Resetear a la primera página y recargar
    this.currentPage.set(1);
    this.loadDayDetails();
  }

  goToPage(page: number): void {
    const details = this.dayDetails();
    if (!details) return;

    if (page < 1 || page > details.totalPages) return;

    this.currentPage.set(page);
    this.loadDayDetails();
  }

  nextPage(): void {
    const details = this.dayDetails();
    if (details && this.currentPage() < details.totalPages) {
      this.goToPage(this.currentPage() + 1);
    }
  }

  previousPage(): void {
    if (this.currentPage() > 1) {
      this.goToPage(this.currentPage() - 1);
    }
  }

  getPageNumbers(): number[] {
    const details = this.dayDetails();
    if (!details) return [];

    const totalPages = details.totalPages;
    const current = this.currentPage();
    const pages: number[] = [];

    // Mostrar hasta 5 páginas
    let startPage = Math.max(1, current - 2);
    let endPage = Math.min(totalPages, current + 2);

    // Ajustar si estamos cerca del inicio o del final
    if (endPage - startPage < 4) {
      if (startPage === 1) {
        endPage = Math.min(5, totalPages);
      } else if (endPage === totalPages) {
        startPage = Math.max(1, totalPages - 4);
      }
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  toggleFilters(): void {
    this.showFilters.set(!this.showFilters());
  }

  goBack(): void {
    this.router.navigate(['/dashboard/agenda']);
  }

  newAppointment(): void {
    // Usar formato local para evitar problemas de timezone
    if (this.selectedDate) {
      const d = this.selectedDate;
      const dateStr = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
      this.router.navigate(['/dashboard/nuevo-turno'], {
        queryParams: { date: dateStr }
      });
    } else {
      this.router.navigate(['/dashboard/nuevo-turno']);
    }
  }

  viewAppointmentDetail(appointmentId: number): void {
    // TODO: Implementar navegación a detalle de turno
  }

  getFormattedDate(): string {
    if (!this.selectedDate) return '';

    const days = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
    const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
                    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

    const dayName = days[this.selectedDate.getDay()];
    const day = this.selectedDate.getDate();
    const month = months[this.selectedDate.getMonth()];
    const year = this.selectedDate.getFullYear();

    return `${dayName}, ${day} de ${month} de ${year}`;
  }

  getStatusClass(status: string): string {
    const statusLower = status.toLowerCase();
    switch (statusLower) {
      case 'confirmado':
        return 'status-confirmed';
      case 'pendiente':
        return 'status-pending';
      case 'completado':
        return 'status-completed';
      case 'cancelado':
        return 'status-cancelled';
      default:
        return 'status-default';
    }
  }

  getStatusIcon(status: string) {
    const statusLower = status.toLowerCase();
    switch (statusLower) {
      case 'confirmado':
        return this.CheckCircleIcon;
      case 'pendiente':
        return this.AlertCircleIcon;
      case 'completado':
        return this.CheckCircleIcon;
      case 'cancelado':
        return this.XCircleIcon;
      default:
        return this.AlertCircleIcon;
    }
  }


  formatMinutesToHours(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
  }
}
