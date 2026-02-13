import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MessageService } from 'primeng/api';
import { LucideAngularModule, ArrowLeft, Save } from 'lucide-angular';
import { InputComponent, ButtonComponent, ToastComponent } from '../../../shared/components';
import { ServiceService } from '../../../services/service-catalog/service.service';
import { CreateServiceDto, UpdateServiceDto } from '../../../models/service.model';

@Component({
  selector: 'app-service-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule,
    InputComponent,
    ButtonComponent,
    ToastComponent
  ],
  templateUrl: './service-form.component.html',
  styleUrls: ['./service-form.component.css'],
  providers: [MessageService]
})
export class ServiceFormComponent implements OnInit {
  private readonly serviceService = inject(ServiceService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  // Icons
  readonly ArrowLeftIcon = ArrowLeft;
  readonly SaveIcon = Save;

  // Signals
  serviceId = signal<number | null>(null);
  isEditMode = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  // Form fields
  name = signal<string>('');
  defaultDuration = signal<number>(30);
  price = signal<number | null>(null);

  errors = signal<{
    name?: string;
    defaultDuration?: string;
    price?: string;
  }>({});

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.serviceId.set(parseInt(id));
      this.isEditMode.set(true);
      this.loadService();
    }
  }

  loadService(): void {
    const id = this.serviceId();
    if (!id) return;

    this.isLoading.set(true);
    this.serviceService.getById(id).subscribe({
      next: (service) => {
        this.name.set(service.name);
        this.defaultDuration.set(service.defaultDuration);
        this.price.set(service.price);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error al cargar servicio:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo cargar el servicio'
        });
        this.isLoading.set(false);
        this.router.navigate(['/dashboard/servicios']);
      }
    });
  }

  validateForm(): boolean {
    const newErrors: any = {};

    // Validar nombre
    const nameValue = this.name().trim();
    if (!nameValue) {
      newErrors.name = 'El nombre es obligatorio';
    } else if (nameValue.length < 3) {
      newErrors.name = 'El nombre debe tener al menos 3 caracteres';
    } else if (nameValue.length > 100) {
      newErrors.name = 'El nombre no puede exceder 100 caracteres';
    }

    // Validar duración
    const duration = this.defaultDuration();
    if (!duration || duration <= 0) {
      newErrors.defaultDuration = 'La duración debe ser mayor a 0';
    } else if (duration > 480) {
      newErrors.defaultDuration = 'La duración no puede exceder 480 minutos (8 horas)';
    }

    // Validar precio (opcional pero si existe debe ser válido)
    const priceValue = this.price();
    if (priceValue !== null && priceValue < 0) {
      newErrors.price = 'El precio no puede ser negativo';
    }

    this.errors.set(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  onSubmit(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isSaving.set(true);

    const dto = {
      name: this.name().trim(),
      defaultDuration: this.defaultDuration(),
      price: this.price()
    };

    if (this.isEditMode()) {
      this.updateService(dto as UpdateServiceDto);
    } else {
      this.createService(dto as CreateServiceDto);
    }
  }

  createService(dto: CreateServiceDto): void {
    this.serviceService.create(dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Servicio creado correctamente'
        });
        this.isSaving.set(false);
        this.router.navigate(['/dashboard/servicios']);
      },
      error: (error) => {
        console.error('Error al crear servicio:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo crear el servicio'
        });
        this.isSaving.set(false);
      }
    });
  }

  updateService(dto: UpdateServiceDto): void {
    const id = this.serviceId();
    if (!id) return;

    this.serviceService.update(id, dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Servicio actualizado correctamente'
        });
        this.isSaving.set(false);
        this.router.navigate(['/dashboard/servicios']);
      },
      error: (error) => {
        console.error('Error al actualizar servicio:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo actualizar el servicio'
        });
        this.isSaving.set(false);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/dashboard/servicios']);
  }

  getTitle(): string {
    return this.isEditMode() ? 'Editar Servicio' : 'Nuevo Servicio';
  }
}
