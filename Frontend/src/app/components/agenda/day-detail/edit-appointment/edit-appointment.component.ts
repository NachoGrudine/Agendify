import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LucideAngularModule, User, Briefcase, X } from 'lucide-angular';
import { AppointmentService } from '../../../../services/appointment/appointment.service';
import { ProviderService } from '../../../../services/provider/provider.service';
import { CustomerService } from '../../../../services/customer/customer.service';
import { ServiceService } from '../../../../services/service-catalog/service.service';
import { ScheduleService } from '../../../../services/schedule/schedule.service';
import { AppointmentFormService } from '../../../../services/appointment/appointment-form.service';
import { ProviderResponse, CustomerResponse, ServiceResponse, AppointmentResponse } from '../../../../models/appointment.model';
import { ProviderScheduleResponse } from '../../../../models/schedule.model';
import { TimeRangePickerComponent, TimeRange } from '../time-range-picker/time-range-picker.component';
import { DateTimeHelper } from '../../../../helpers/date-time.helper';
import { ScheduleValidator } from '../../../../validators/schedule.validator';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

/**
 * Componente para EDITAR appointments existentes
 * Para crear nuevos, ver NewAppointmentComponent
 */
@Component({
  selector: 'app-edit-appointment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule, TimeRangePickerComponent],
  templateUrl: './edit-appointment.component.html',
  styleUrls: ['./edit-appointment.component.css']
})
export class EditAppointmentComponent implements OnInit {
  // Services
  private readonly appointmentService = inject(AppointmentService);
  private readonly providerService = inject(ProviderService);
  private readonly customerService = inject(CustomerService);
  private readonly serviceService = inject(ServiceService);
  private readonly scheduleService = inject(ScheduleService);
  private readonly formService = inject(AppointmentFormService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  // Icons
  readonly UserIcon = User;
  readonly BriefcaseIcon = Briefcase;
  readonly XIcon = X;

  // Form
  appointmentForm!: FormGroup;

  // Data
  appointmentId: number | null = null;
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

  // Delegated to formService
  get filteredCustomers() { return this.formService.filteredCustomers; }
  get filteredServices() { return this.formService.filteredServices; }
  get showCustomerDropdown() { return this.formService.showCustomerDropdown; }
  get showServiceDropdown() { return this.formService.showServiceDropdown; }

  ngOnInit(): void {
    this.initForm();
    this.setupSearchListeners();

    // Obtener el ID del appointment de los params
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.appointmentId = parseInt(id);
        // Cargar datos necesarios ANTES del appointment
        this.loadInitialData();
      } else {
        this.errorMessage.set('ID de turno no válido');
        setTimeout(() => this.router.navigate(['/dashboard/agenda']), 2000);
      }
    });
  }

  /**
   * Carga los datos necesarios (providers, customers, services) antes de cargar el appointment
   */
  private loadInitialData(): void {
    this.isLoading.set(true);
    let loadedCount = 0;
    const totalLoads = 3;

    const checkAllLoaded = () => {
      loadedCount++;
      if (loadedCount === totalLoads) {
        // Todos los datos están cargados, ahora sí cargar el appointment
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
    if (!this.appointmentId) return;

    this.isLoading.set(true);
    this.appointmentService.getById(this.appointmentId).subscribe({
      next: (appointment) => {
        this.originalAppointment = appointment;
        this.populateForm(appointment);
        this.loadProviderSchedules(appointment.providerId);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set('Error al cargar el turno');
        this.isLoading.set(false);
        setTimeout(() => this.router.navigate(['/dashboard/agenda']), 2000);
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

    // Popular el formulario con TODOS los valores de una vez
    this.appointmentForm.patchValue({
      providerId: appointment.providerId,
      customerId: appointment.customerId,
      customerSearch: customerDisplayName,
      serviceId: appointment.serviceId,
      serviceSearch: serviceDisplayName,
      startTime: DateTimeHelper.formatTime(startDateTime),
      endTime: DateTimeHelper.formatTime(endDateTime),
      notes: appointment.notes || ''
    });
  }

  private loadProviders(onComplete?: () => void): void {
    this.providerService.getAll().subscribe({
      next: (data) => {
        this.providers.set(data.filter(p => p.isActive));
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
    // Customer search - LIMPIAR customerId cuando el usuario escribe
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

      this.formService.filterCustomers(searchText, this.customers());
    });

    // Service search - LIMPIAR serviceId cuando el usuario escribe
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

      this.formService.filterServices(searchText, this.services());
    });
  }

  selectCustomer(customer: CustomerResponse): void {
    this.formService.selectCustomer(this.appointmentForm, customer);
  }

  selectService(service: ServiceResponse): void {
    this.formService.selectService(this.appointmentForm, service);
  }

  onTimeRangeChange(timeRange: TimeRange): void {
    this.appointmentForm.patchValue({
      startTime: timeRange.startTime,
      endTime: timeRange.endTime
    });
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
          this.router.navigate(['/dashboard/agenda']);
        }, 1500);
      },
      error: (error) => {
        this.errorMessage.set(error?.error?.message || 'Error al actualizar el turno');
        this.isSaving.set(false);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/dashboard/agenda']);
  }

  get formattedSelectedDate(): string {
    if (!this.originalAppointment) return '';
    const date = new Date(this.originalAppointment.startTime);
    return DateTimeHelper.formatDateSpanish(date);
  }
}
