import { Component, input, output, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register-step3',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './step3.component.html',
  styleUrls: ['./step3.component.css']
})
export class RegisterStep3Component {
  formData = input.required<any>();
  isSubmitting = input<boolean>(false);
  back = output<void>();
  updateData = output<any>();
  submit = output<void>();

  providerName = signal<string>('');
  providerSpecialty = signal<string>('');
  avatarPreview = signal<string | null>(null);

  // Variable local para prevenir doble submit
  private isProcessing = signal<boolean>(false);

  errors = signal<{
    providerName?: string;
    providerSpecialty?: string;
  }>({});

  constructor() {
    // Usar effect para sincronizar con formData cuando cambie
    effect(() => {
      const data = this.formData();
      console.log('Step3: Inicializando/Actualizando con formData:', data);
      if (data.providerName !== undefined) {
        this.providerName.set(data.providerName || '');
      }
      if (data.providerSpecialty !== undefined) {
        this.providerSpecialty.set(data.providerSpecialty || '');
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      const reader = new FileReader();

      reader.onload = (e) => {
        this.avatarPreview.set(e.target?.result as string);
      };

      reader.readAsDataURL(file);
    }
  }

  validateForm(): boolean {
    const newErrors: any = {};

    if (!this.providerName() || this.providerName().trim() === '') {
      newErrors.providerName = 'El nombre profesional es requerido';
    } else if (this.providerName().length > 200) {
      newErrors.providerName = 'El nombre no puede exceder 200 caracteres';
    }

    // Validar specialty solo si no está vacía (es opcional)
    if (this.providerSpecialty() && this.providerSpecialty().length > 200) {
      newErrors.providerSpecialty = 'La especialidad no puede exceder 200 caracteres';
    }

    this.errors.set(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  onSubmit(): void {
    // Prevenir múltiples envíos - verificar tanto el input como la variable local
    if (this.isSubmitting() || this.isProcessing()) {
      console.warn('Step3: Ya se está procesando, ignorando submit');
      return;
    }

    if (this.validateForm()) {
      // Marcar como procesando INMEDIATAMENTE
      this.isProcessing.set(true);

      console.log('Step3: Validación exitosa, emitiendo datos:', {
        providerName: this.providerName(),
        providerSpecialty: this.providerSpecialty()
      });

      this.updateData.emit({
        providerName: this.providerName().trim(),
        providerSpecialty: this.providerSpecialty().trim()
      });

      this.submit.emit();

      // Reset después de 2 segundos por seguridad (en caso de error)
      setTimeout(() => this.isProcessing.set(false), 2000);
    } else {
      console.error('Step3: Validación falló, errores:', this.errors());
    }
  }

  onBack(): void {
    this.updateData.emit({
      providerName: this.providerName(),
      providerSpecialty: this.providerSpecialty()
    });
    this.back.emit();
  }
}
