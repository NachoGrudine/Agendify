import { Component, OnInit, inject, signal, computed } from '@angular/core';
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
import { ErrorHelper } from '../../../helpers/error.helper';
import { AppointmentStatusHelper } from '../../../helpers/appointment-status.helper';
import { ButtonComponent, InputComponent, LoadingSpinnerComponent, ProgressBarComponent, DialogComponent } from '../../../shared/components';
import { AppointmentFormComponent } from './appointment-form/appointment-form.component';

@Component({
  selector: 'app-day-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule,
    ButtonComponent,
    InputComponent,
    LoadingSpinnerComponent,
    ProgressBarComponent,
    DialogComponent,
    AppointmentFormComponent
  ],
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
  selectedDate = signal<Date | null>(null);

  // Modales - usando propiedades normales para compatibilidad con PrimeNG Dialog
  private _showNewAppointmentModal = false;
  private _showEditAppointmentModal = false;
  selectedAppointmentId = signal<number | null>(null);

  // Computed para verificar si la fecha seleccionada es pasada
  isPastDate = computed(() => {
    const selected = this.selectedDate();
    if (!selected) return false;

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const selectedDay = new Date(selected);
    selectedDay.setHours(0, 0, 0, 0);

    return selectedDay < today;
  });

  get showNewAppointmentModal() { return this._showNewAppointmentModal; }
  set showNewAppointmentModal(value: boolean) { this._showNewAppointmentModal = value; }

  get showEditAppointmentModal() { return this._showEditAppointmentModal; }
  set showEditAppointmentModal(value: boolean) { this._showEditAppointmentModal = value; }

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
        const parsedDate = DateTimeHelper.parseDate(params['date']);
        this.selectedDate.set(parsedDate);
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
    const date = this.selectedDate();
    if (!date) return;

    this.isLoading.set(true);

    this.calendarService.getDayDetails(
      date,
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

  viewAppointmentDetail(appointmentId: number): void {
    // TODO: Implementar navegación a detalle de turno
  }

  getFormattedDate(): string {
    const date = this.selectedDate();
    if (!date) return '';
    return DateTimeHelper.formatDateSpanish(date);
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
   * Volver al calendario
   */
  goBack(): void {
    this.router.navigate(['/dashboard/agenda']);
  }

  /**
   * Abrir modal de nuevo turno
   */
  newAppointment(): void {
    this.showNewAppointmentModal = true;
  }

  /**
   * Cerrar modal de nuevo turno
   */
  closeNewAppointmentModal(): void {
    this.showNewAppointmentModal = false;
  }

  /**
   * Cuando se crea un turno exitosamente
   */
  onAppointmentCreated(): void {
    this.showNewAppointmentModal = false;
    this.loadDayDetails(); // Recargar datos
  }

  /**
   * Editar un appointment - ABRE MODAL
   */
  editAppointment(appointmentId: number): void {
    console.log('📝 Editando appointment con ID:', appointmentId);
    this.selectedAppointmentId.set(appointmentId);
    console.log('📝 selectedAppointmentId después de set:', this.selectedAppointmentId());
    this.showEditAppointmentModal = true;
    console.log('📝 Modal abierto:', this.showEditAppointmentModal);
  }

  /**
   * Cerrar modal de editar turno
   */
  closeEditAppointmentModal(): void {
    this.showEditAppointmentModal = false;
    this.selectedAppointmentId.set(null);
  }

  /**
   * Cuando se actualiza un turno exitosamente
   */
  onAppointmentUpdated(): void {
    this.showEditAppointmentModal = false;
    this.selectedAppointmentId.set(null);
    this.loadDayDetails(); // Recargar datos
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
          const errorMsg = ErrorHelper.extractErrorMessage(error, 'Error al eliminar el turno');
          alert(errorMsg);
          this.isLoading.set(false);
        }
      });
    }
  }
}
