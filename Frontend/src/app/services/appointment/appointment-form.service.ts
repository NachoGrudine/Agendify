import { Injectable, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProviderResponse, CustomerResponse, ServiceResponse } from '../../models/appointment.model';

/**
 * Servicio para manejar la lógica del formulario de appointments
 * Separa la lógica de UI del componente
 */
@Injectable({
  providedIn: 'root'
})
export class AppointmentFormService {
  // Signals para datos del formulario
  filteredCustomers = signal<CustomerResponse[]>([]);
  filteredServices = signal<ServiceResponse[]>([]);
  showCustomerDropdown = signal(false);
  showServiceDropdown = signal(false);

  constructor(private fb: FormBuilder) {}

  /**
   * Crea el formulario de appointment con validaciones
   */
  createForm(selectedDate: Date | null): FormGroup {
    const today = selectedDate || new Date();
    const startTime = new Date(today);
    startTime.setHours(9, 0, 0, 0);
    const endTime = new Date(today);
    endTime.setHours(10, 0, 0, 0);

    return this.fb.group({
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

  /**
   * Filtra customers según el texto de búsqueda
   */
  filterCustomers(searchText: string, allCustomers: CustomerResponse[]): void {
    if (searchText && searchText.length >= 2) {
      const filtered = allCustomers.filter(c =>
        c.name.toLowerCase().includes(searchText.toLowerCase()) ||
        c.email?.toLowerCase().includes(searchText.toLowerCase()) ||
        c.phone?.includes(searchText)
      );
      this.filteredCustomers.set(filtered);
      this.showCustomerDropdown.set(true);
    } else {
      this.filteredCustomers.set([]);
      this.showCustomerDropdown.set(false);
    }
  }

  /**
   * Filtra servicios según el texto de búsqueda
   */
  filterServices(searchText: string, allServices: ServiceResponse[]): void {
    if (searchText && searchText.length >= 2) {
      const filtered = allServices.filter(s =>
        s.name.toLowerCase().includes(searchText.toLowerCase())
      );
      this.filteredServices.set(filtered);
      this.showServiceDropdown.set(true);
    } else {
      this.filteredServices.set([]);
      this.showServiceDropdown.set(false);
    }
  }

  /**
   * Selecciona un customer y actualiza el formulario
   */
  selectCustomer(form: FormGroup, customer: CustomerResponse): void {
    form.patchValue({
      customerSearch: customer.name,
      customerId: customer.id,
      customerName: ''
    });
    this.showCustomerDropdown.set(false);
  }

  /**
   * Selecciona un service y actualiza el formulario
   */
  selectService(form: FormGroup, service: ServiceResponse): void {
    form.patchValue({
      serviceSearch: service.name,
      serviceId: service.id,
      serviceName: ''
    });
    this.showServiceDropdown.set(false);
  }

  /**
   * Valida el formulario y marca todos los campos como touched si es inválido
   */
  validateForm(form: FormGroup): boolean {
    if (form.invalid) {
      Object.keys(form.controls).forEach(key => {
        form.get(key)?.markAsTouched();
      });
      return false;
    }
    return true;
  }

  private formatTime(date: Date): string {
    return date.toTimeString().slice(0, 5);
  }
}
