import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, ArrowLeft, Calendar, Clock, Users, Filter, Plus, Eye, X, CheckCircle, XCircle, AlertCircle, Edit, Trash2 } from 'lucide-angular';
import { CalendarService } from '../../../services/calendar/calendar.service';
import { DayDetailFilterService } from '../../../services/calendar/day-detail-filter.service';
import { ProviderService } from '../../../services/provider/provider.service';
import { AppointmentService } from '../../../services/appointment/appointment.service';
import { DayDetailsDto } from '../../../models/calendar.model';
import { ProviderResponse } from '../../../models/appointment.model';
import { DateTimeHelper } from '../../../helpers/date-time.helper';
import { AppointmentStatusHelper } from '../../../helpers/appointment-status.helper';

@Component({
  selector: 'app-day-detail',
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './day-detail.html',
  styleUrl: './day-detail.css',
})
export class DayDetailComponent implements OnInit {
  private readonly calendarService = inject(CalendarService);
  private readonly providerService = inject(ProviderService);
  private readonly appointmentService = inject(AppointmentService);
  private readonly filterService = inject(DayDetailFilterService);
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
  readonly EditIcon = Edit;
  readonly TrashIcon = Trash2;

  // Datos principales
  dayDetails = signal<DayDetailsDto | null>(null);
  providers = signal<ProviderResponse[]>([]);
  isLoading = signal(false);
  selectedDate: Date | null = null;

  // Delegados al filterService
  get currentPage() { return this.filterService.currentPage; }
  get pageSize() { return this.filterService.pageSize; }
  get pageSizeOptions() { return this.filterService.pageSizeOptions; }
  get showFilters() { return this.filterService.showFilters; }
  get searchText() { return this.filterService.searchText(); }
  set searchText(value: string) { this.filterService.searchText.set(value); }
  get filters() { return this.filterService.filters(); }
  set filters(value: any) { this.filterService.filters.set(value); }
  get availableStatuses() { return this.filterService.availableStatuses; }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['date']) {
        this.selectedDate = DateTimeHelper.parseDate(params['date']);
        this.loadProviders();
        this.loadDayDetails();
      } else {
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

    this.calendarService.getDayDetails(
      this.selectedDate,
      this.currentPage(),
      this.pageSize(),
      this.filterService.getFilterParams()
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
    this.filterService.changePageSize(newSize);
    this.loadDayDetails();
  }

  applyFilters(): void {
    this.filterService.applyFilters();
    this.loadDayDetails();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  clearFilters(): void {
    this.filterService.clearFilters();
    this.loadDayDetails();
  }

  goToPage(page: number): void {
    const details = this.dayDetails();
    if (!details) return;

    if (this.filterService.goToPage(page, details.totalPages)) {
      this.loadDayDetails();
    }
  }

  nextPage(): void {
    const details = this.dayDetails();
    if (details && this.filterService.nextPage(details.totalPages)) {
      this.loadDayDetails();
    }
  }

  previousPage(): void {
    if (this.filterService.previousPage()) {
      this.loadDayDetails();
    }
  }

  getPageNumbers(): number[] {
    const details = this.dayDetails();
    if (!details) return [];
    return this.filterService.calculatePageNumbers(details.totalPages);
  }

  toggleFilters(): void {
    this.filterService.toggleFilters();
  }

  goBack(): void {
    this.router.navigate(['/dashboard/agenda']);
  }

  newAppointment(): void {
    if (this.selectedDate) {
      const dateStr = DateTimeHelper.formatDate(this.selectedDate);
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
    return DateTimeHelper.formatDateSpanish(this.selectedDate);
  }

  getStatusClass(status: string): string {
    return AppointmentStatusHelper.getStatusClass(status);
  }

  getStatusIcon(status: string) {
    const iconName = AppointmentStatusHelper.getStatusIconName(status);
    // Mapear nombres de iconos a los componentes reales
    switch(iconName) {
      case 'CheckCircle': return this.CheckCircleIcon;
      case 'AlertCircle': return this.AlertCircleIcon;
      case 'XCircle': return this.XCircleIcon;
      default: return this.AlertCircleIcon;
    }
  }

  formatMinutesToHours(minutes: number): string {
    return AppointmentStatusHelper.formatMinutesToHours(minutes);
  }

  /**
   * Editar un appointment
   */
  editAppointment(appointmentId: number): void {
    this.router.navigate(['/dashboard/editar-turno', appointmentId]);
  }

  /**
   * Eliminar un appointment
   */
  deleteAppointment(appointmentId: number, customerName: string): void {
    const confirmed = confirm(`¿Estás seguro de que deseas eliminar el turno de ${customerName}?`);

    if (confirmed) {
      this.isLoading.set(true);

      this.appointmentService.delete(appointmentId).subscribe({
        next: () => {
          // Recargar los detalles del día después de eliminar
          this.loadDayDetails();
        },
        error: (error) => {
          alert(error?.error?.message || 'Error al eliminar el turno');
          this.isLoading.set(false);
        }
      });
    }
  }
}
