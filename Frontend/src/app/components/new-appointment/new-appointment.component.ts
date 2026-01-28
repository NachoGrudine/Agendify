import { Component, OnInit, Input, Output, EventEmitter, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LucideAngularModule, Calendar, Clock, User, Briefcase, FileText, Save, X, Search } from 'lucide-angular';
import { AppointmentService } from '../../services/appointment.service';
import { ProviderService } from '../../services/provider.service';
import { CustomerService } from '../../services/customer.service';
import { ServiceService } from '../../services/service.service';
import { ScheduleService } from '../../services/schedule.service';
import { AuthService } from '../../services/auth.service';
import { ProviderResponse, CustomerResponse, ServiceResponse } from '../../models/appointment.model';
import { ProviderScheduleResponse } from '../../models/schedule.model';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-new-appointment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
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
  filteredCustomers = signal<CustomerResponse[]>([]);
  filteredServices = signal<ServiceResponse[]>([]);
  providerSchedules = signal<ProviderScheduleResponse[]>([]);

  // UI State
  isLoading = signal(false);
  isSaving = signal(false);
  errorMessage = signal('');
  successMessage = signal('');
  showProviderField = signal(true);
  customerSearchText = signal('');
  serviceSearchText = signal('');
  showCustomerDropdown = signal(false);
  showServiceDropdown = signal(false);

  ngOnInit(): void {
    // Leer fecha de query params si existe
    this.route.queryParams.subscribe(params => {
      if (params['date']) {
        // Parsear la fecha correctamente evitando timezone issues
        const [year, month, day] = params['date'].split('-').map(Number);
        this.selectedDate = new Date(year, month - 1, day);
      }
    });

    this.initForm();
    this.loadProviders();
    this.loadCustomers();
    this.loadServices();
    this.setupSearchListeners();
  }

  private initForm(): void {
    const today = this.selectedDate || new Date();
    const startTime = new Date(today);
    startTime.setHours(9, 0, 0, 0);
    const endTime = new Date(today);
    endTime.setHours(10, 0, 0, 0);

    this.appointmentForm = this.fb.group({
      providerId: [null, Validators.required],
      customerSearch: [''],
      customerId: [null],
      customerName: [''],
      serviceSearch: [''],
      serviceId: [null],
      serviceName: [''],
      startTime: [this.formatTime(startTime), Validators.required],
      endTime: [this.formatTime(endTime), Validators.required],
      notes: ['', Validators.maxLength(1000)]
    });
  }

  private loadProviders(): void {
    this.isLoading.set(true);
    this.providerService.getAll().subscribe({
      next: (data) => {
        console.log('✅ Respuesta de providers:', data);
        console.log('📊 Total de providers:', data.length);
        console.log('🔍 Estructura del primer provider:', data[0]);

        // Verificar si el problema es el nombre del campo
        if (data.length > 0) {
          const firstProvider = data[0] as any;
          console.log('🔑 Keys del primer provider:', Object.keys(firstProvider));
          console.log('👁️ Valor de isActive:', firstProvider.isActive);
        }

        console.log('🔍 Providers activos (isActive):', data.filter(p => p.isActive));
        console.log('❌ Providers inactivos (isActive):', data.filter(p => !p.isActive));

        this.providers.set(data.filter(p => p.isActive));

        console.log('Providers activos finales:', this.providers().length, this.providers());

        // Si hay solo 1 provider, autoseleccionarlo y ocultar el campo
        if (this.providers().length === 1) {
          console.log('✅ Solo hay 1 provider, ocultando campo');
          this.appointmentForm.patchValue({ providerId: this.providers()[0].id });
          this.showProviderField.set(false);
          this.appointmentForm.get('providerId')?.clearValidators();
          this.appointmentForm.get('providerId')?.updateValueAndValidity();
          this.loadProviderSchedules(this.providers()[0].id);
        } else if (this.providers().length > 1) {
          console.log('📋 Hay múltiples providers, mostrando campo');
          const currentProviderId = this.authService.getProviderId();
          if (currentProviderId) {
            const currentProvider = this.providers().find(p => p.id === currentProviderId);
            if (currentProvider) {
              this.appointmentForm.patchValue({ providerId: currentProviderId });
              this.loadProviderSchedules(currentProviderId);
            }
          }
        } else {
          console.error('⚠️ No hay providers activos para este negocio!');
          console.error('💡 Revisa si el campo is_active está llegando correctamente del backend');
          this.errorMessage.set('No hay providers disponibles. Por favor, contacta al administrador.');
        }

        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('❌ Error al cargar providers:', err);
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
      error: () => console.error('Error al cargar clientes')
    });
  }

  private loadServices(): void {
    this.serviceService.getAll().subscribe({
      next: (data) => {
        this.services.set(data);
      },
      error: () => console.error('Error al cargar servicios')
    });
  }

  private loadProviderSchedules(providerId: number): void {
    this.scheduleService.getProviderSchedules(providerId).subscribe({
      next: (schedules) => {
        this.providerSchedules.set(schedules);
      },
      error: () => console.error('Error al cargar horarios del provider')
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
      this.customerSearchText.set(searchText);
      if (searchText && searchText.length >= 2) {
        const filtered = this.customers().filter(c =>
          c.name.toLowerCase().includes(searchText.toLowerCase())
        );
        this.filteredCustomers.set(filtered);
        this.showCustomerDropdown.set(true);
      } else {
        this.filteredCustomers.set([]);
        this.showCustomerDropdown.set(false);
      }
    });

    // Service search
    this.appointmentForm.get('serviceSearch')?.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchText => {
      this.serviceSearchText.set(searchText);
      if (searchText && searchText.length >= 2) {
        const filtered = this.services().filter(s =>
          s.name.toLowerCase().includes(searchText.toLowerCase())
        );
        this.filteredServices.set(filtered);
        this.showServiceDropdown.set(true);
      } else {
        this.filteredServices.set([]);
        this.showServiceDropdown.set(false);
      }
    });
  }

  selectCustomer(customer: CustomerResponse): void {
    this.appointmentForm.patchValue({
      customerSearch: customer.name,
      customerId: customer.id,
      customerName: ''
    });
    this.showCustomerDropdown.set(false);
  }

  selectService(service: ServiceResponse): void {
    this.appointmentForm.patchValue({
      serviceSearch: service.name,
      serviceId: service.id,
      serviceName: ''
    });
    this.showServiceDropdown.set(false);
  }

  onSubmit(): void {
    if (this.appointmentForm.invalid) {
      Object.keys(this.appointmentForm.controls).forEach(key => {
        this.appointmentForm.get(key)?.markAsTouched();
      });
      return;
    }

    const formValue = this.appointmentForm.value;

    // Usar la fecha seleccionada del calendario
    const dateStr = this.formatDate(this.selectedDate || new Date());
    const startTime = formValue.startTime;
    const endTime = formValue.endTime;

    const startDateTime = new Date(`${dateStr}T${startTime}`);
    const endDateTime = new Date(`${dateStr}T${endTime}`);

    if (startDateTime >= endDateTime) {
      this.errorMessage.set('La hora de inicio debe ser menor a la hora de fin');
      return;
    }

    // Validar que esté dentro del horario del provider
    if (!this.isWithinProviderSchedule(startDateTime, endDateTime)) {
      this.errorMessage.set('El horario seleccionado está fuera del horario laboral del provider');
      return;
    }

    // Preparar DTO en camelCase - el interceptor lo convierte a snake_case automáticamente
    const dto: any = {
      providerId: formValue.providerId,
      startTime: startDateTime.toISOString(),
      endTime: endDateTime.toISOString(),
      notes: formValue.notes || null
    };

    // Customer: ID o Name
    if (formValue.customerId) {
      dto.customerId = formValue.customerId;
    } else if (formValue.customerSearch && !formValue.customerId) {
      dto.customerName = formValue.customerSearch;
    }

    // Service: ID o Name
    if (formValue.serviceId) {
      dto.serviceId = formValue.serviceId;
    } else if (formValue.serviceSearch && !formValue.serviceId) {
      dto.serviceName = formValue.serviceSearch;
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

  private isWithinProviderSchedule(start: Date, end: Date): boolean {
    const dayOfWeek = start.getDay();
    const daySchedules = this.providerSchedules().filter(s => s.dayOfWeek === dayOfWeek);

    if (daySchedules.length === 0) {
      return false;
    }

    const startMinutes = start.getHours() * 60 + start.getMinutes();
    const endMinutes = end.getHours() * 60 + end.getMinutes();

    return daySchedules.some(schedule => {
      const scheduleStart = this.timeSpanToMinutes(schedule.startTime);
      const scheduleEnd = this.timeSpanToMinutes(schedule.endTime);
      return startMinutes >= scheduleStart && endMinutes <= scheduleEnd;
    });
  }

  private timeSpanToMinutes(timeSpan: string): number {
    const parts = timeSpan.split(':');
    return parseInt(parts[0]) * 60 + parseInt(parts[1]);
  }

  onCancel(): void {
    this.cancel.emit();
    this.router.navigate(['/dashboard/agenda']);
  }

  private formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  private formatTime(date: Date): string {
    return date.toTimeString().slice(0, 5);
  }

  get formattedSelectedDate(): string {
    if (!this.selectedDate) return '';
    const date = new Date(this.selectedDate);
    const days = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];
    const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
                    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
    return `${days[date.getDay()]}, ${date.getDate()} de ${months[date.getMonth()]}`;
  }
}
