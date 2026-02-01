import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';
import { LucideAngularModule, User, Briefcase, X, CalendarCheck, Clock, AlertCircle, CheckCircle } from 'lucide-angular';
import { AppointmentService } from '../../../../services/appointment/appointment.service';
import { ProviderService } from '../../../../services/provider/provider.service';
import { CustomerService } from '../../../../services/customer/customer.service';
import { ServiceService } from '../../../../services/service-catalog/service.service';
import { ScheduleService } from '../../../../services/schedule/schedule.service';
import { AppointmentFormService } from '../../../../services/appointment/appointment-form.service';
import { ProviderResponse, CustomerResponse, ServiceResponse, AppointmentResponse } from '../../../../models/appointment.model';
import { ProviderScheduleResponse } from '../../../../models/schedule.model';import { DateTimeHelper } from '../../../../helpers/date-time.helper';
import { ErrorHelper } from '../../../../helpers/error.helper';
import { ScheduleValidator } from '../../../../validators/schedule.validator';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ButtonComponent, LoadingSpinnerComponent, DropdownComponent, TextareaComponent, InputComponent, SectionIconComponent } from '../../../../shared/components';

/**
 * Componente para EDITAR appointments existentes
 * Para crear nuevos, ver NewAppointmentComponent
 */
@Component({
  selector: 'app-edit-appointment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule, ButtonComponent, LoadingSpinnerComponent, DropdownComponent, TextareaComponent, InputComponent, SectionIconComponent],
  templateUrl: './edit-appointment.component.html',
  styleUrls: ['./edit-appointment.component.css']
})
export class EditAppointmentComponent implements OnInit, OnChanges {
  @Input() appointmentId: number | null = null; // ID del turno a editar
  @Output() cancel = new EventEmitter<void>();
  @Output() success = new EventEmitter<void>();

  // Services
  private readonly appointmentService = inject(AppointmentService);
  private readonly providerService = inject(ProviderService);
  private readonly customerService = inject(CustomerService);
  private readonly serviceService = inject(ServiceService);
  private readonly scheduleService = inject(ScheduleService);
  private readonly formService = inject(AppointmentFormService);

  // Icons
  readonly UserIcon = User;
  readonly BriefcaseIcon = Briefcase;
  readonly XIcon = X;
  readonly CalendarCheckIcon = CalendarCheck;
  readonly ClockIcon = Clock;
  readonly AlertCircleIcon = AlertCircle;
  readonly CheckCircleIcon = CheckCircle;

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
  showProviderField = signal(true); // Mostrar campo de provider solo si hay múltiples

  // Delegated to formService
  get filteredCustomers() { return this.formService.filteredCustomers; }
  get filteredServices() { return this.formService.filteredServices; }
  get showCustomerDropdown() { return this.formService.showCustomerDropdown; }
  get showServiceDropdown() { return this.formService.showServiceDropdown; }

  ngOnInit(): void {
    console.log('🔧 EditAppointment ngOnInit');
    this.initForm();
    this.setupSearchListeners();
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Detectar cuando cambia el appointmentId
    if (changes['appointmentId'] && changes['appointmentId'].currentValue) {
      const newId = changes['appointmentId'].currentValue;
      console.log('🔄 EditAppointment ngOnChanges - appointmentId cambió a:', newId);

      // Si el formulario ya está inicializado, cargar los datos
      if (this.appointmentForm) {
        console.log('✅ Formulario ya inicializado, cargando datos...');
        this.loadInitialData();
      }
    }
  }

  /**
   * Carga los datos necesarios (providers, customers, services) antes de cargar el appointment
   */
  private loadInitialData(): void {
    if (!this.appointmentId) {
      console.error('❌ No se puede cargar datos sin appointmentId');
      this.errorMessage.set('ID de turno no válido');
      setTimeout(() => this.cancel.emit(), 2000);
      return;
    }

    console.log('📦 Cargando datos iniciales para appointment:', this.appointmentId);
    this.isLoading.set(true);
    let loadedCount = 0;
    const totalLoads = 3;

    const checkAllLoaded = () => {
      loadedCount++;
      console.log(`📦 Cargados ${loadedCount}/${totalLoads} recursos`);
      if (loadedCount === totalLoads) {
        // Todos los datos están cargados, ahora sí cargar el appointment
        console.log('✅ Todos los recursos cargados, cargando appointment...');
        this.loadAppointment();
      }
    };

    this.loadProviders(checkAllLoaded);
    this.loadCustomers(checkAllLoaded);
    this.loadServices(checkAllLoaded);
  }

  private initForm(): void {
    this.appointmentForm = this.formService.createForm(null);
  }

  private loadAppointment(): void {
    if (!this.appointmentId) {
      console.error('❌ No se puede cargar appointment sin ID');
      return;
    }

    console.log('📅 Cargando appointment con ID:', this.appointmentId);
    this.isLoading.set(true);
    this.appointmentService.getById(this.appointmentId).subscribe({
      next: (appointment) => {
        console.log('✅ Appointment cargado:', appointment);
        this.originalAppointment = appointment;
        this.populateForm(appointment);
        this.loadProviderSchedules(appointment.providerId);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('❌ Error al cargar appointment:', error);
        this.errorMessage.set('Error al cargar el turno');
        this.isLoading.set(false);
        setTimeout(() => this.cancel.emit(), 2000);
      }
    });
  }

  private populateForm(appointment: AppointmentResponse): void {
    const startDateTime = new Date(appointment.startTime);
    const endDateTime = new Date(appointment.endTime);

    // Encontrar el customer para mostrar su nombre
    let customerDisplayName = '';
    if (appointment.customerId) {
      const customer = this.customers().find(c => c.id === appointment.customerId);
      customerDisplayName = customer ? customer.name : '';
    }

    // Encontrar el service para mostrar su nombre
    let serviceDisplayName = '';
    if (appointment.serviceId) {
      const service = this.services().find(s => s.id === appointment.serviceId);
      serviceDisplayName = service ? service.name : '';
    }

    // Deshabilitar temporalmente los listeners para evitar validaciones prematuras
    this.appointmentForm.patchValue({
      providerId: appointment.providerId,
      customerId: appointment.customerId,
      customerSearch: customerDisplayName,
      serviceId: appointment.serviceId,
      serviceSearch: serviceDisplayName,
      startTime: DateTimeHelper.formatTime(startDateTime),
      endTime: DateTimeHelper.formatTime(endDateTime),
      notes: appointment.notes || ''
    }, { emitEvent: false });

    // Marcar los campos como touched y válidos
    Object.keys(this.appointmentForm.controls).forEach(key => {
      const control = this.appointmentForm.get(key);
      if (control) {
        control.markAsTouched();
        control.updateValueAndValidity();
      }
    });

    // Ahora sí emitir los eventos para que los watchers funcionen
    this.appointmentForm.updateValueAndValidity();
  }

  private loadProviders(onComplete?: () => void): void {
    this.providerService.getAll().subscribe({
      next: (data) => {
        this.providers.set(data.filter(p => p.isActive));

        // Si hay solo 1 provider, ocultar el campo
        if (this.providers().length === 1) {
          this.showProviderField.set(false);
        }

        if (onComplete) onComplete();
      },
      error: () => {
        this.errorMessage.set('Error al cargar providers');
        if (onComplete) onComplete();
      }
    });
  }

  private loadCustomers(onComplete?: () => void): void {
    this.customerService.getAll().subscribe({
      next: (data) => {
        this.customers.set(data);
        if (onComplete) onComplete();
      },
      error: () => {
        if (onComplete) onComplete();
      }
    });
  }

  private loadServices(onComplete?: () => void): void {
    this.serviceService.getAll().subscribe({
      next: (data) => {
        this.services.set(data);
        if (onComplete) onComplete();
      },
      error: () => {
        if (onComplete) onComplete();
      }
    });
  }

  private loadProviderSchedules(providerId: number): void {
    this.scheduleService.getProviderSchedules(providerId).subscribe({
      next: (schedules) => {
        this.providerSchedules.set(schedules);
      },
      error: () => {}
    });
  }

  onProviderChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const providerId = parseInt(select.value);
    if (providerId) {
      this.loadProviderSchedules(providerId);
    }
  }

  private setupSearchListeners(): void {
    // Customer search - SOLO MOSTRAR DROPDOWN CON MÍNIMO 3 CARACTERES
    this.appointmentForm.get('customerSearch')?.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchText => {
      // Si el usuario está escribiendo manualmente, limpiar el customerId
      const currentCustomerId = this.appointmentForm.get('customerId')?.value;
      if (currentCustomerId) {
        const customer = this.customers().find(c => c.id === currentCustomerId);
        // Si el texto no coincide con el customer actual, limpiar el ID
        if (!customer || customer.name !== searchText) {
          this.appointmentForm.patchValue({ customerId: null }, { emitEvent: false });
        }
      }

      // SOLO filtrar si hay al menos 3 caracteres
      if (searchText && searchText.length >= 3) {
        this.formService.filterCustomers(searchText, this.customers());
      } else {
        // Ocultar dropdown si hay menos de 3 caracteres
        this.formService.hideCustomerDropdown();
      }
    });

    // Service search - SOLO MOSTRAR DROPDOWN CON MÍNIMO 3 CARACTERES
    this.appointmentForm.get('serviceSearch')?.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchText => {
      // Si el usuario está escribiendo manualmente, limpiar el serviceId
      const currentServiceId = this.appointmentForm.get('serviceId')?.value;
      if (currentServiceId) {
        const service = this.services().find(s => s.id === currentServiceId);
        // Si el texto no coincide con el service actual, limpiar el ID
        if (!service || service.name !== searchText) {
          this.appointmentForm.patchValue({ serviceId: null }, { emitEvent: false });
        }
      }

      // SOLO filtrar si hay al menos 3 caracteres
      if (searchText && searchText.length >= 3) {
        this.formService.filterServices(searchText, this.services());
      } else {
        // Ocultar dropdown si hay menos de 3 caracteres
        this.formService.hideServiceDropdown();
      }
    });
  }

  selectCustomer(customer: CustomerResponse): void {
    this.formService.selectCustomer(this.appointmentForm, customer);
  }

  selectService(service: ServiceResponse): void {
    this.formService.selectService(this.appointmentForm, service);
  }


  /**
   * Actualizar un appointment existente
   */
  onSubmit(): void {
    if (!this.appointmentId || !this.originalAppointment) {
      this.errorMessage.set('Error: No se puede actualizar el turno');
      return;
    }

    // Validar formulario
    if (!this.formService.validateForm(this.appointmentForm)) {
      return;
    }

    const formValue = this.appointmentForm.value;
    const baseDate = new Date(this.originalAppointment.startTime);

    // Parsear tiempos para validación
    const { hours: startHour, minutes: startMinute } = DateTimeHelper.parseTime(formValue.startTime);
    const { hours: endHour, minutes: endMinute } = DateTimeHelper.parseTime(formValue.endTime);

    const startDateTime = DateTimeHelper.createDateTime(baseDate, startHour, startMinute);
    const endDateTime = DateTimeHelper.createDateTime(baseDate, endHour, endMinute);

    // Validar tiempos
    if (startDateTime >= endDateTime) {
      this.errorMessage.set('La hora de inicio debe ser anterior a la hora de fin');
      return;
    }

    // Validar que esté dentro del horario del provider
    if (!ScheduleValidator.isWithinProviderSchedule(startDateTime, endDateTime, this.providerSchedules())) {
      this.errorMessage.set('El horario seleccionado está fuera del horario laboral del provider');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    // Construir DTO para actualización
    const dto: any = {
      providerId: formValue.providerId,
      startTime: DateTimeHelper.toLocalISOString(startDateTime),
      endTime: DateTimeHelper.toLocalISOString(endDateTime),
      status: this.originalAppointment.status, // Mantener el estado original
      notes: formValue.notes || null
    };

    // Customer: ID o Name
    if (formValue.customerId) {
      dto.customerId = formValue.customerId;
      dto.customerName = null;
    } else if (formValue.customerSearch) {
      dto.customerId = null;
      dto.customerName = formValue.customerSearch;
    }

    // Service: ID o Name
    if (formValue.serviceId) {
      dto.serviceId = formValue.serviceId;
      dto.serviceName = null;
    } else if (formValue.serviceSearch) {
      dto.serviceId = null;
      dto.serviceName = formValue.serviceSearch;
    }

    this.appointmentService.update(this.appointmentId, dto).subscribe({
      next: () => {
        this.successMessage.set('¡Turno actualizado exitosamente!');
        this.isSaving.set(false);
        setTimeout(() => {
          this.success.emit(); // Emitir evento en lugar de navegar
        }, 1500);
      },
      error: (error) => {
        // Usar ErrorHelper para extraer y formatear el mensaje de error
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
    this.cancel.emit();
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
    if (!this.originalAppointment) return '';
    const date = new Date(this.originalAppointment.startTime);
    return DateTimeHelper.formatDateSpanish(date);
  }
}
