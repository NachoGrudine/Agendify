import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { LucideAngularModule, Store, Scissors, ArrowLeft } from 'lucide-angular';
import { InputComponent, DropdownComponent, ButtonComponent, ToastComponent } from '../../../shared/components';
import { BusinessService } from '../../../services/business/business.service';
import { BusinessResponse, UpdateBusinessDto } from '../../../models/business.model';

@Component({
  selector: 'app-configuracion-negocio',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule,
    InputComponent,
    DropdownComponent,
    ButtonComponent,
    ToastComponent
  ],
  templateUrl: './configuracion-negocio.component.html',
  styleUrls: ['./configuracion-negocio.component.css'],
  providers: [MessageService]
})
export class ConfiguracionNegocioComponent implements OnInit {
  private readonly businessService = inject(BusinessService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);

  // Icons
  readonly StoreIcon = Store;
  readonly ScissorsIcon = Scissors;
  readonly ArrowLeftIcon = ArrowLeft;

  // Signals
  businessName = signal<string>('');
  industry = signal<string>('');
  originalData = signal<BusinessResponse | null>(null);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  errors = signal<{
    businessName?: string;
    industry?: string;
  }>({});

  // Opciones de industrias (las mismas del registro)
  industries = [
    { value: 'Barbería', label: 'Barbería' },
    { value: 'Peluquería', label: 'Peluquería' },
    { value: 'Barbería & Peluquería', label: 'Barbería & Peluquería' },
    { value: 'Estética', label: 'Estética y Belleza' },
    { value: 'Consultorio Médico', label: 'Consultorio Médico' },
    { value: 'Consultorio Odontológico', label: 'Consultorio Odontológico' },
    { value: 'Spa', label: 'Spa y Wellness' },
    { value: 'Gimnasio', label: 'Gimnasio' },
    { value: 'Centro de Masajes', label: 'Centro de Masajes' },
    { value: 'Estudio de Tatuajes', label: 'Estudio de Tatuajes' },
    { value: 'Veterinaria', label: 'Veterinaria' },
    { value: 'Otro', label: 'Otro' }
  ];

  ngOnInit(): void {
    this.loadBusinessData();
  }

  loadBusinessData(): void {
    this.isLoading.set(true);
    this.businessService.get().subscribe({
      next: (data) => {
        this.originalData.set(data);
        this.businessName.set(data.name);
        this.industry.set(data.industry);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error al cargar datos del negocio:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo cargar la información del negocio'
        });
        this.isLoading.set(false);
      }
    });
  }

  hasChanges(): boolean {
    const original = this.originalData();
    if (!original) return false;

    return (
      this.businessName() !== original.name ||
      this.industry() !== original.industry
    );
  }

  validateForm(): boolean {
    const newErrors: any = {};

    // Validar nombre del negocio
    const name = this.businessName().trim();
    if (!name) {
      newErrors.businessName = 'El nombre del negocio es obligatorio';
    } else if (name.length < 3) {
      newErrors.businessName = 'El nombre debe tener al menos 3 caracteres';
    } else if (name.length > 200) {
      newErrors.businessName = 'El nombre no puede exceder 200 caracteres';
    }

    // Validar industria
    const industry = this.industry();
    if (!industry) {
      newErrors.industry = 'Debes seleccionar una industria';
    } else if (industry.length > 100) {
      newErrors.industry = 'La industria no puede exceder 100 caracteres';
    }

    this.errors.set(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  onSave(): void {
    if (!this.validateForm()) {
      return;
    }

    if (!this.hasChanges()) {
      this.messageService.add({
        severity: 'info',
        summary: 'Sin cambios',
        detail: 'No hay cambios para guardar'
      });
      return;
    }

    this.isSaving.set(true);

    const updateDto: UpdateBusinessDto = {
      name: this.businessName().trim(),
      industry: this.industry()
    };

    this.businessService.update(updateDto).subscribe({
      next: (data) => {
        this.originalData.set(data);
        this.businessName.set(data.name);
        this.industry.set(data.industry);
        this.isSaving.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Los cambios se guardaron correctamente'
        });
      },
      error: (error) => {
        console.error('Error al actualizar negocio:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudieron guardar los cambios'
        });
        this.isSaving.set(false);
      }
    });
  }

  onCancel(): void {
    const original = this.originalData();
    if (original) {
      this.businessName.set(original.name);
      this.industry.set(original.industry);
      this.errors.set({});
    }
    this.router.navigate(['/dashboard/mi-negocio']);
  }
}
