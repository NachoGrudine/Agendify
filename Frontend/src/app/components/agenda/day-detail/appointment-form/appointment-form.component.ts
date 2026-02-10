import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';
import { LucideAngularModule, User, Briefcase, X, CalendarCheck, CalendarPlus, Clock, AlertCircle, CheckCircle, ChevronLeft, ChevronRight } from 'lucide-angular';
import { AppointmentService } from '../../../../services/appointment/appointment.service';
import { ProviderService } from '../../../../services/provider/provider.service';
import { CustomerService } from '../../../../services/customer/customer.service';
import { ServiceService } from '../../../../services/service-catalog/service.service';
import { ScheduleService } from '../../../../services/schedule/schedule.service';
import { AuthService } from '../../../../services/auth/auth.service';
import { AppointmentFormService } from '../../../../services/appointment/appointment-form.service';
import { ProviderResponse, CustomerResponse, AppointmentResponse } from '../../../../models/appointment.model';
import { ServiceResponse } from '../../../../models/service.model';
import { ProviderScheduleResponse } from '../../../../models/schedule.model';
import { DateTimeHelper } from '../../../../helpers/date-time.helper';
import { ErrorHelper } from '../../../../helpers/error.helper';
import { ScheduleValidator } from '../../../../validators/schedule.validator';
import { AppointmentDTOBuilder } from '../../../../builders/appointment-dto.builder';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ButtonComponent, LoadingSpinnerComponent, DropdownComponent, TextareaComponent, InputComponent, SectionIconComponent } from '../../../../shared/components';

/**
 * Componente unificado para crear y editar appointments
 * Usa el @Input mode para determinar el comportamiento
 */
@Component({
  selector: 'app-appointment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule, ButtonComponent, LoadingSpinnerComponent, DropdownComponent, TextareaComponent, InputComponent, SectionIconComponent],
  templateUrl: './appointment-form.component.html',
  styleUrls: ['./appointment-form.component.css']
})
export class AppointmentFormComponent implements OnInit, OnChanges {
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() appointmentId: number | null = null; // Solo para modo edit
  @Input() selectedDate: Date | null = null; // Solo para modo create
  @Output() cancel = new EventEmitter<void>();
  @Output() success = new EventEmitter<void>();

  // Services
  private readonly appointmentService = inject(AppointmentService);
  private readonly providerService = inject(ProviderService);
  private readonly customerService = inject(CustomerService);
  private readonly serviceService = inject(ServiceService);
  private readonly scheduleService = inject(ScheduleService);
  private readonly authService = inject(AuthService);
  private readonly formService = inject(AppointmentFormService);

  // Icons
  readonly UserIcon = User;
  readonly CalendarPlusIcon = CalendarPlus;
  readonly CalendarCheckIcon = CalendarCheck;
  readonly BriefcaseIcon = Briefcase;
  readonly XIcon = X;
  readonly ClockIcon = Clock;
  readonly AlertCircleIcon = AlertCircle;
  readonly CheckCircleIcon = CheckCircle;
  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;

  // Para usar Math en el template
  readonly Math = Math;

  // Form
  appointmentForm!: FormGroup;

  // Data
  originalAppointment: AppointmentResponse | null = null;
  providers = signal<ProviderResponse[]>([]);
  customers = signal<CustomerResponse[]>([]);
  services = signal<ServiceResponse[]>([]);
  providerSchedules = signal<ProviderScheduleResponse[]>([]);

  // UI State
  isLoading = signal(false);
  isSaving = signal(false);
  errorMessage = signal('');
  successMessage = signal('');
  showProviderField = signal(true);

  // Paginación de clientes
  customerPageSize = signal(5);
  customerCurrentPage = signal(1);
  customerPageSizeOptions = [5, 10, 15];

  // Computed
  readonly isEditMode = computed(() => this.mode === 'edit');
  readonly modalTitle = computed(() => this.isEditMode() ? 'Editar Turno' : 'Nuevo Turno');
  readonly modalIcon = computed(() => this.isEditMode() ? this.CalendarCheckIcon : this.CalendarPlusIcon);
  readonly submitButtonLabel = computed(() => this.isEditMode() ? 'Actualizar Turno' : 'Crear Turno');

  // Computed para verificar si la fecha/hora es pasada - SOLO en modo edición
  readonly isPastDateTime = computed(() => {
    // Solo validar en modo edición
    if (!this.isEditMode()) return false;

    if (!this.appointmentForm) return false;

    const formValue = this.appointmentForm.value;
    if (!formValue.startTime) return false;

    const baseDate = this.isEditMode() && this.originalAppointment
      ? DateTimeHelper.parseLocalDateTime(this.originalAppointment.startTime)
      : (this.selectedDate || new Date());

    const { hours: startHour, minutes: startMinute } = DateTimeHelper.parseTime(formValue.startTime);
    const startDateTime = DateTimeHelper.createDateTime(baseDate, startHour, startMinute);

    return startDateTime < new Date();
  });

  // Delegated to formService
  get filteredCustomers() { return this.formService.filteredCustomers; }
  get filteredServices() { return this.formService.filteredServices; }
  get showCustomerDropdown() { return this.formService.showCustomerDropdown; }
  get showServiceDropdown() { return this.formService.showServiceDropdown; }

  // Computed para paginación de clientes
  readonly paginatedCustomers = computed(() => {
    const customers = this.filteredCustomers();
    const pageSize = this.customerPageSize();
    const currentPage = this.customerCurrentPage();

    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;

    return customers.slice(startIndex, endIndex);
  });

  readonly totalCustomerPages = computed(() => {
    const total = this.filteredCustomers().length;
    const pageSize = this.customerPageSize();
    return Math.ceil(total / pageSize);
  });

  readonly hasCustomerPreviousPage = computed(() => this.customerCurrentPage() > 1);
  readonly hasCustomerNextPage = computed(() => this.customerCurrentPage() < this.totalCustomerPages());

  ngOnInit(): void {
    this.clearMessages();
    this.initForm();
    this.setupSearchListeners();

    if (this.isEditMode()) {
      // Modo edición: cargar datos
      if (this.appointmentId) {
        this.loadInitialData();
      }
    } else {
      // Modo creación: cargar listas
      this.loadProviders();
      this.loadCustomers();
      this.loadServices();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.isEditMode() && changes['appointmentId'] && changes['appointmentId'].currentValue) {
      this.clearMessages();
      if (this.appointmentForm) {
        this.loadInitialData();
      }
    }

    // Si cambia la fecha seleccionada en modo crear, resetear el formulario
    if (!this.isEditMode() && changes['selectedDate'] && this.appointmentForm) {
      this.resetForm();
    }
  }

  private initForm(): void {
    const date = this.isEditMode() && this.originalAppointment
      ? DateTimeHelper.parseLocalDateTime(this.originalAppointment.startTime)
      : (this.selectedDate || new Date());
    this.appointmentForm = this.formService.createForm(date);
  }

  private setupSearchListeners(): void {
    // Customer search
    this.appointmentForm.get('customerSearch')?.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(value => {
        this.formService.filterCustomers(value, this.customers());
        this.customerCurrentPage.set(1); // Resetear a página 1 al filtrar
      });

    // Service search
    this.appointmentForm.get('serviceSearch')?.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(value => {
        this.formService.filterServices(value, this.services());
      });
  }

  private loadInitialData(): void {
    if (!this.appointmentId) {
      this.errorMessage.set('ID de turno no válido');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    // Cargar todas las listas necesarias y el appointment
    Promise.all([
      this.providerService.getAll().toPromise(),
      this.customerService.getAll().toPromise(),
      this.serviceService.getAll().toPromise(),
      this.appointmentService.getById(this.appointmentId!).toPromise()
    ]).then(([providers, customers, services, appointment]) => {
      console.log('🔄 Datos cargados en modo edición:');
      console.log('  - Providers:', providers?.length);
      console.log('  - Customers:', customers?.length);
      console.log('  - Services:', services?.length);
      console.log('  - Appointment:', appointment);

      // Guardar las listas
      this.providers.set((providers || []).filter(p => p.isActive));
      this.customers.set(customers || []);
      this.services.set(services || []);

      if (this.providers().length === 1) {
        this.showProviderField.set(false);
      }

      // Poblar el formulario con los datos del appointment
      if (appointment) {
        this.originalAppointment = appointment;
        this.populateForm(appointment);
        this.loadProviderSchedules(appointment.providerId);
      }

      this.isLoading.set(false);
    }).catch((error) => {
      const errorMsg = ErrorHelper.extractErrorMessage(error, 'Error al cargar el turno');
      this.errorMessage.set(errorMsg);
      this.isLoading.set(false);
    });
  }

  private populateForm(appointment: AppointmentResponse): void {
    const startDate = DateTimeHelper.parseLocalDateTime(appointment.startTime);
    const endDate = DateTimeHelper.parseLocalDateTime(appointment.endTime);

    const startTime = DateTimeHelper.formatTime(startDate);
    const endTime = DateTimeHelper.formatTime(endDate);

    // Poblar el formulario con los datos del appointment
    this.appointmentForm.patchValue({
      providerId: appointment.providerId,
      customerId: appointment.customerId || null,
      customerSearch: appointment.customerName || '',
      serviceId: appointment.serviceId || null,
      serviceSearch: appointment.serviceName || '',
      startTime: startTime,
      endTime: endTime,
      notes: appointment.notes || ''
    }, { emitEvent: false }); // No emitir eventos para evitar búsquedas innecesarias
  }

  private loadProviders(): void {
    this.isLoading.set(true);
    this.providerService.getAll().subscribe({
      next: (data) => {
        this.providers.set(data.filter(p => p.isActive));

        if (this.providers().length === 1) {
          this.appointmentForm.patchValue({ providerId: this.providers()[0].id });
          this.showProviderField.set(false);
          this.appointmentForm.get('providerId')?.clearValidators();
          this.appointmentForm.get('providerId')?.updateValueAndValidity();
          this.loadProviderSchedules(this.providers()[0].id);
        } else if (this.providers().length > 1) {
          const currentProviderId = this.authService.getProviderId();
          if (currentProviderId) {
            const currentProvider = this.providers().find(p => p.id === currentProviderId);
            if (currentProvider) {
              this.appointmentForm.patchValue({ providerId: currentProviderId });
              this.loadProviderSchedules(currentProviderId);
            }
          }
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  private loadCustomers(): void {
    this.customerService.getAll().subscribe({
      next: (data) => this.customers.set(data),
      error: () => {}
    });
  }

  private loadServices(): void {
    this.serviceService.getAll().subscribe({
      next: (data) => this.services.set(data),
      error: () => {}
    });
  }

  private loadProviderSchedules(providerId: number): void {
    this.scheduleService.getProviderSchedules(providerId).subscribe({
      next: (schedules) => this.providerSchedules.set(schedules),
      error: () => {}
    });
  }

  onProviderChange(event: any): void {
    const providerId = event.value;
    if (providerId) {
      this.loadProviderSchedules(providerId);
    }
  }

  selectCustomer(customer: CustomerResponse): void {
    this.formService.selectCustomer(this.appointmentForm, customer);
  }

  selectService(service: ServiceResponse): void {
    this.formService.selectService(this.appointmentForm, service);
  }

  onSubmit(): void {
    if (!this.formService.validateForm(this.appointmentForm)) {
      return;
    }

    const formValue = this.appointmentForm.value;
    const baseDate = this.isEditMode() && this.originalAppointment
      ? DateTimeHelper.parseLocalDateTime(this.originalAppointment.startTime)
      : (this.selectedDate || new Date());

    const { hours: startHour, minutes: startMinute } = DateTimeHelper.parseTime(formValue.startTime);
    const { hours: endHour, minutes: endMinute } = DateTimeHelper.parseTime(formValue.endTime);

    const startDateTime = DateTimeHelper.createDateTime(baseDate, startHour, startMinute);
    const endDateTime = DateTimeHelper.createDateTime(baseDate, endHour, endMinute);

    // Validar que no sea una fecha/hora pasada SOLO en modo edición
    // En modo crear, permitimos crear turnos para cualquier fecha seleccionada
    const now = new Date();
    if (this.isEditMode() && startDateTime < now) {
      this.errorMessage.set('No se puede editar un turno a una fecha u hora pasada');
      return;
    }

    const timeValidation = AppointmentDTOBuilder.validateTimes(startDateTime, endDateTime);
    if (!timeValidation.valid) {
      this.errorMessage.set(timeValidation.error!);
      return;
    }

    if (!ScheduleValidator.isWithinProviderSchedule(startDateTime, endDateTime, this.providerSchedules())) {
      this.errorMessage.set('El horario seleccionado está fuera del horario laboral del provider');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    if (this.isEditMode()) {
      this.updateAppointment(formValue, baseDate);
    } else {
      this.createAppointment(formValue, baseDate);
    }
  }

  private createAppointment(formValue: any, baseDate: Date): void {
    const dto = AppointmentDTOBuilder.buildCreateDTO(formValue, baseDate);

    this.appointmentService.create(dto).subscribe({
      next: () => {
        this.successMessage.set('¡Turno creado exitosamente!');
        this.isSaving.set(false);
        setTimeout(() => {
          this.resetForm(); // Limpiar el formulario antes de cerrar
          this.success.emit();
        }, 1500);
      },
      error: (error) => {
        const errorMsg = ErrorHelper.formatScheduleConflictError(
          error,
          formValue.startTime,
          formValue.endTime
        );
        this.errorMessage.set(errorMsg);
        this.isSaving.set(false);
      }
    });
  }

  private updateAppointment(formValue: any, baseDate: Date): void {
    const dto = AppointmentDTOBuilder.buildUpdateDTO(formValue, baseDate, formValue.status || 'Pending');

    this.appointmentService.update(this.appointmentId!, dto).subscribe({
      next: () => {
        this.successMessage.set('¡Turno actualizado exitosamente!');
        this.isSaving.set(false);
        setTimeout(() => {
          this.success.emit();
        }, 1500);
      },
      error: (error) => {
        const errorMsg = ErrorHelper.formatScheduleConflictError(
          error,
          formValue.startTime,
          formValue.endTime
        );
        this.errorMessage.set(errorMsg);
        this.isSaving.set(false);
      }
    });
  }

  onCancel(): void {
    this.clearMessages();
    // Resetear el formulario en modo crear para que quede limpio
    if (!this.isEditMode()) {
      this.resetForm();
    }
    this.cancel.emit();
  }

  private clearMessages(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  /**
   * Resetea el formulario a sus valores iniciales
   * Solo se usa en modo crear
   */
  private resetForm(): void {
    if (this.isEditMode()) return;

    // Reinicializar el formulario completamente
    this.appointmentForm.reset({
      providerId: null,
      customerSearch: '',
      customerId: null,
      customerName: '',
      serviceSearch: '',
      serviceId: null,
      serviceName: '',
      startTime: '09:00',
      endTime: '10:00',
      notes: ''
    });

    // Si solo hay un proveedor, pre-seleccionarlo
    if (this.providers().length === 1) {
      this.appointmentForm.patchValue({ providerId: this.providers()[0].id });
    } else if (this.providers().length > 1) {
      const currentProviderId = this.authService.getProviderId();
      if (currentProviderId) {
        const currentProvider = this.providers().find(p => p.id === currentProviderId);
        if (currentProvider) {
          this.appointmentForm.patchValue({ providerId: currentProviderId });
        }
      }
    }

    // Limpiar los filtros del servicio de formulario
    this.formService.clearFilters();
  }

  getCalculatedDuration(): string {
    const start = this.appointmentForm.get('startTime')?.value;
    const end = this.appointmentForm.get('endTime')?.value;

    if (!start || !end) return '';

    const [startHour, startMin] = start.split(':').map(Number);
    const [endHour, endMin] = end.split(':').map(Number);

    const totalMinutes = (endHour * 60 + endMin) - (startHour * 60 + startMin);

    if (totalMinutes <= 0) return 'Hora inválida';

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (hours > 0 && minutes > 0) {
      return `${hours}h ${minutes}min`;
    } else if (hours > 0) {
      return `${hours}h`;
    } else {
      return `${minutes}min`;
    }
  }

  get formattedSelectedDate(): string {
    if (this.isEditMode() && this.originalAppointment) {
      const date = DateTimeHelper.parseLocalDateTime(this.originalAppointment.startTime);
      return DateTimeHelper.formatDateSpanish(date);
    } else if (this.selectedDate) {
      return DateTimeHelper.formatDateSpanish(this.selectedDate);
    }
    return DateTimeHelper.formatDateSpanish(new Date());
  }

  /**
   * Verifica si la fecha/hora seleccionada está en el pasado
   * Solo aplica en modo edición
   */
  isDateTimeInPast(): boolean {
    // Solo validar en modo edición
    if (!this.isEditMode()) return false;

    const startTime = this.appointmentForm.get('startTime')?.value;
    if (!startTime) return false;

    const baseDate = this.isEditMode() && this.originalAppointment
      ? DateTimeHelper.parseLocalDateTime(this.originalAppointment.startTime)
      : (this.selectedDate || new Date());

    const { hours: startHour, minutes: startMinute } = DateTimeHelper.parseTime(startTime);
    const startDateTime = DateTimeHelper.createDateTime(baseDate, startHour, startMinute);

    const now = new Date();
    return startDateTime < now;
  }

  // Métodos de paginación de clientes
  goToCustomerPage(page: number): void {
    if (page >= 1 && page <= this.totalCustomerPages()) {
      this.customerCurrentPage.set(page);
    }
  }

  changeCustomerPageSize(event: any): void {
    this.customerPageSize.set(event.value);
    this.customerCurrentPage.set(1); // Resetear a página 1 al cambiar tamaño
  }

  previousCustomerPage(): void {
    if (this.hasCustomerPreviousPage()) {
      this.customerCurrentPage.update(page => page - 1);
    }
  }

  nextCustomerPage(): void {
    if (this.hasCustomerNextPage()) {
      this.customerCurrentPage.update(page => page + 1);
    }
  }
}
