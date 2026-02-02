import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { LucideAngularModule, Plus, Search } from 'lucide-angular';
import { ButtonComponent, InputComponent, ToastComponent, DialogComponent } from '../../shared/components';
import { ServiceService } from '../../services/service-catalog/service.service';
import { ServiceResponse } from '../../models/service.model';
import { ServiceCardComponent } from './service-card/service-card.component';

@Component({
  selector: 'app-servicios',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule,
    ButtonComponent,
    InputComponent,
    ToastComponent,
    DialogComponent,
    ServiceCardComponent
  ],
  templateUrl: './servicios.component.html',
  styleUrls: ['./servicios.component.css'],
  providers: [MessageService]
})
export class ServiciosComponent implements OnInit {
  private readonly serviceService = inject(ServiceService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);

  // Icons
  readonly PlusIcon = Plus;
  readonly SearchIcon = Search;

  // Signals
  services = signal<ServiceResponse[]>([]);
  filteredServices = signal<ServiceResponse[]>([]);
  isLoading = signal<boolean>(false);
  searchTerm = signal<string>('');
  showDeleteDialog = signal<boolean>(false);
  selectedService = signal<ServiceResponse | null>(null);

  ngOnInit(): void {
    this.loadServices();
  }

  loadServices(): void {
    this.isLoading.set(true);
    this.serviceService.getAll().subscribe({
      next: (data) => {
        this.services.set(data);
        this.filteredServices.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error al cargar servicios:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudieron cargar los servicios'
        });
        this.isLoading.set(false);
      }
    });
  }

  onSearch(): void {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) {
      this.filteredServices.set(this.services());
      return;
    }

    const filtered = this.services().filter(service =>
      service.name.toLowerCase().includes(term)
    );
    this.filteredServices.set(filtered);
  }

  onCreateService(): void {
    this.router.navigate(['/dashboard/servicios/nuevo']);
  }

  onEditService(service: ServiceResponse): void {
    this.router.navigate(['/dashboard/servicios/editar', service.id]);
  }

  onDeleteService(service: ServiceResponse): void {
    this.selectedService.set(service);
    this.showDeleteDialog.set(true);
  }

  confirmDelete(): void {
    const service = this.selectedService();
    if (!service) return;

    this.serviceService.delete(service.id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Servicio eliminado correctamente'
        });
        this.showDeleteDialog.set(false);
        this.selectedService.set(null);
        this.loadServices();
      },
      error: (error) => {
        console.error('Error al eliminar servicio:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo eliminar el servicio'
        });
        this.showDeleteDialog.set(false);
      }
    });
  }

  cancelDelete(): void {
    this.showDeleteDialog.set(false);
    this.selectedService.set(null);
  }
}
