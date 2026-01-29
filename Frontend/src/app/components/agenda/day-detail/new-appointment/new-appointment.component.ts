import { Component, OnInit, Input, Output, EventEmitter, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LucideAngularModule, Calendar, Clock, User, Briefcase, FileText, Save, X, Search } from 'lucide-angular';
import { AppointmentService } from '../../../../services/appointment/appointment.service';
import { ProviderService } from '../../../../services/provider/provider.service';
import { CustomerService } from '../../../../services/customer/customer.service';
import { ServiceService } from '../../../../services/service-catalog/service.service';
import { ScheduleService } from '../../../../services/schedule/schedule.service';
import { AuthService } from '../../../../services/auth/auth.service';
import { AppointmentFormService } from '../../../../services/appointment/appointment-form.service';
import { ProviderResponse, CustomerResponse, ServiceResponse } from '../../../../models/appointment.model';
import { ProviderScheduleResponse } from '../../../../models/schedule.model';
import { TimeRangePickerComponent, TimeRange } from '../time-range-picker/time-range-picker.component';
import { DateTimeHelper } from '../../../../helpers/date-time.helper';
import { ScheduleValidator } from '../../../../validators/schedule.validator';
import { AppointmentDTOBuilder } from '../../../../builders/appointment-dto.builder';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-new-appointment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule, TimeRangePickerComponent],
  templateUrl: './new-appointment.component.html',
  styleUrls: ['./new-appointment.component.css']
})
export class NewAppointmentComponent implements OnInit {
  @Input() selectedDate: Date | null = null;
  @Output() cancel = new EventEmitter<void>();
  @Output() success = new EventEmitter<void>();

  // Services
  private readonly fb = inject(FormBuilder);
  private readonly appointmentService = inject(AppointmentService);
  private readonly providerService = inject(ProviderService);
  private readonly customerService = inject(CustomerService);
  private readonly serviceService = inject(ServiceService);
  private readonly scheduleService = inject(ScheduleService);
  private readonly authService = inject(AuthService);
  private readonly formService = inject(AppointmentFormService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  // Icons
  readonly CalendarIcon = Calendar;
  readonly ClockIcon = Clock;
  readonly UserIcon = User;
  readonly BriefcaseIcon = Briefcase;
  readonly FileTextIcon = FileText;
  readonly SaveIcon = Save;
  readonly XIcon = X;
  readonly SearchIcon = Search;

  // Form
  appointmentForm!: FormGroup;

  // Data
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

  // Delegated to formService
  get filteredCustomers() { return this.formService.filteredCustomers; }
  get filteredServices() { return this.formService.filteredServices; }
  get showCustomerDropdown() { return this.formService.showCustomerDropdown; }
  get showServiceDropdown() { return this.formService.showServiceDropdown; }


  ngOnInit(): void {
    // Leer fecha de query params si existe
    this.route.queryParams.subscribe(params => {
      if (params['date']) {
        this.selectedDate = DateTimeHelper.parseDate(params['date']);

        if (!DateTimeHelper.isValidDate(this.selectedDate)) {
          this.selectedDate = new Date();
        }
      }
    });

    this.initForm();
    this.loadProviders();
    this.loadCustomers();
    this.loadServices();
    this.setupSearchListeners();
  }

  private initForm(): void {
    this.appointmentForm = this.formService.createForm(this.selectedDate);
  }

  private loadProviders(): void {
    this.isLoading.set(true);
    this.providerService.getAll().subscribe({
      next: (data) => {
        this.providers.set(data.filter(p => p.isActive));

        // Si hay solo 1 provider, autoseleccionarlo y ocultar el campo
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
        } else {
          this.errorMessage.set('No hay providers disponibles. Por favor, contacta al administrador.');
        }

        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set('Error al cargar providers');
        this.isLoading.set(false);
      }
    });
  }

  private loadCustomers(): void {
    this.customerService.getAll().subscribe({
      next: (data) => {
        this.customers.set(data);
      },
      error: () => {}
    });
  }

  private loadServices(): void {
    this.serviceService.getAll().subscribe({
      next: (data) => {
        this.services.set(data);
      },
      error: () => {}
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
    // Customer search
    this.appointmentForm.get('customerSearch')?.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchText => {
      this.formService.filterCustomers(searchText, this.customers());
    });

    // Service search
    this.appointmentForm.get('serviceSearch')?.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchText => {
      this.formService.filterServices(searchText, this.services());
    });
  }

  selectCustomer(customer: CustomerResponse): void {
    this.formService.selectCustomer(this.appointmentForm, customer);
  }

  selectService(service: ServiceResponse): void {
    this.formService.selectService(this.appointmentForm, service);
  }

  /**
   * Maneja el cambio de rango de tiempo desde el TimeRangePicker
   */
  onTimeRangeChange(timeRange: TimeRange): void {
    this.appointmentForm.patchValue({
      startTime: timeRange.startTime,
      endTime: timeRange.endTime
    });
  }

  onSubmit(): void {
    // Validar formulario
    if (!this.formService.validateForm(this.appointmentForm)) {
      return;
    }

    const formValue = this.appointmentForm.value;
    const baseDate = this.selectedDate || new Date();

    // Construir DTO usando el builder
    const dto = AppointmentDTOBuilder.buildCreateDTO(formValue, baseDate);

    // Parsear tiempos para validación
    const { hours: startHour, minutes: startMinute } = DateTimeHelper.parseTime(formValue.startTime);
    const { hours: endHour, minutes: endMinute } = DateTimeHelper.parseTime(formValue.endTime);

    const startDateTime = DateTimeHelper.createDateTime(baseDate, startHour, startMinute);
    const endDateTime = DateTimeHelper.createDateTime(baseDate, endHour, endMinute);

    // Validar tiempos
    const timeValidation = AppointmentDTOBuilder.validateTimes(startDateTime, endDateTime);
    if (!timeValidation.valid) {
      this.errorMessage.set(timeValidation.error!);
      return;
    }

    // Validar que esté dentro del horario del provider
    if (!ScheduleValidator.isWithinProviderSchedule(startDateTime, endDateTime, this.providerSchedules())) {
      this.errorMessage.set('El horario seleccionado está fuera del horario laboral del provider');
      return;
    }


    this.isSaving.set(true);
    this.errorMessage.set('');

    this.appointmentService.create(dto).subscribe({
      next: () => {
        this.successMessage.set('¡Turno creado exitosamente!');
        this.isSaving.set(false);
        setTimeout(() => {
          this.success.emit();
          this.router.navigate(['/dashboard/agenda']);
        }, 1500);
      },
      error: (error) => {
        this.errorMessage.set(error?.error?.message || 'Error al crear el turno');
        this.isSaving.set(false);
      }
    });
  }

  onCancel(): void {
    this.cancel.emit();
    this.router.navigate(['/dashboard/agenda']);
  }

  get formattedSelectedDate(): string {
    if (!this.selectedDate) return '';
    return DateTimeHelper.formatDateSpanish(this.selectedDate);
  }
}
