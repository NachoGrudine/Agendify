import { Component, input, output, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register-step2',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './step2.component.html',
  styleUrls: ['./step2.component.css']
})
export class RegisterStep2Component {
  formData = input.required<any>();
  next = output<void>();
  back = output<void>();
  updateData = output<any>();

  businessName = signal<string>('');
  industry = signal<string>('');

  errors = signal<{
    businessName?: string;
    industry?: string;
  }>({});

  industries = [
    { value: '', label: 'Selecciona tu industria' },
    { value: 'Barbería', label: 'Barbería' },
    { value: 'Peluquería', label: 'Peluquería' },
    { value: 'Estética', label: 'Estética y Belleza' },
    { value: 'Consultorio Médico', label: 'Consultorio Médico' },
    { value: 'Consultorio Dental', label: 'Consultorio Dental' },
    { value: 'Spa', label: 'Spa y Masajes' },
    { value: 'Gimnasio', label: 'Gimnasio / Personal Training' },
    { value: 'Taller Mecánico', label: 'Taller Mecánico' },
    { value: 'Veterinaria', label: 'Veterinaria' },
    { value: 'Fotografía', label: 'Fotografía' },
    { value: 'Otro', label: 'Otro' }
  ];

  constructor() {
    effect(() => {
      const data = this.formData();
      this.businessName.set(data.businessName || '');
      this.industry.set(data.industry || '');
    });
  }

  validateForm(): boolean {
    const newErrors: any = {};

    if (!this.businessName() || this.businessName().trim() === '') {
      newErrors.businessName = 'El nombre del negocio es requerido';
    } else if (this.businessName().length > 200) {
      newErrors.businessName = 'El nombre del negocio no puede exceder 200 caracteres';
    }

    if (!this.industry() || this.industry() === '') {
      newErrors.industry = 'Debes seleccionar una industria';
    } else if (this.industry().length > 100) {
      newErrors.industry = 'La industria no puede exceder 100 caracteres';
    }

    this.errors.set(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  onContinue(): void {
    if (this.validateForm()) {
      this.updateData.emit({
        businessName: this.businessName(),
        industry: this.industry()
      });
      this.next.emit();
    }
  }

  onBack(): void {
    this.updateData.emit({
      businessName: this.businessName(),
      industry: this.industry()
    });
    this.back.emit();
  }
}
