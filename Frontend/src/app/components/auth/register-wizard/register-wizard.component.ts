import { Component, signal, inject, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RegisterStep1Component } from './steps/step1/step1.component';
import { RegisterStep2Component } from './steps/step2/step2.component';
import { RegisterStep3Component } from './steps/step3/step3.component';
import { AuthService } from '../../../services/auth.service';
import { RegisterDto } from '../../../models/auth.model';

interface RegisterFormData {
  email: string;
  password: string;
  confirmPassword: string;
  businessName: string;
  industry: string;
  providerName: string;
  providerSpecialty: string;
}

@Component({
  selector: 'app-register-wizard',
  standalone: true,
  imports: [
    CommonModule,
    RegisterStep1Component,
    RegisterStep2Component,
    RegisterStep3Component
  ],
  templateUrl: './register-wizard.component.html',
  styleUrls: ['./register-wizard.component.css']
})
export class RegisterWizardComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  loginClick = output<void>();

  currentStep = signal<number>(1);
  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string>('');

  formData = signal<RegisterFormData>({
    email: '',
    password: '',
    confirmPassword: '',
    businessName: '',
    industry: '',
    providerName: '',
    providerSpecialty: ''
  });

  progressPercentage = computed(() => Math.round((this.currentStep() / 3) * 100));

  nextStep(): void {
    if (this.currentStep() < 3) {
      this.currentStep.update(step => step + 1);
      this.errorMessage.set('');
    }
  }

  prevStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update(step => step - 1);
      this.errorMessage.set('');
    }
  }

  updateFormData(data: Partial<RegisterFormData>): void {
    this.formData.update(current => {
      const updated = { ...current, ...data };
      console.log('Form data actualizada:', updated);
      return updated;
    });
  }

  onFinalSubmit(): void {
    // Prevenir múltiples envíos
    if (this.isSubmitting()) {
      console.warn('Ya se está procesando un registro, ignorando envío duplicado');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const data = this.formData();

    console.log('Datos del formulario completo:', data);

    // Validar que todos los campos requeridos estén completos
    if (!data.email || !data.password || !data.businessName || !data.industry || !data.providerName) {
      console.error('Faltan campos requeridos:', {
        email: !!data.email,
        password: !!data.password,
        businessName: !!data.businessName,
        industry: !!data.industry,
        providerName: !!data.providerName
      });
      this.errorMessage.set('Por favor, completa todos los campos requeridos.');
      this.isSubmitting.set(false);
      return;
    }

    // Convertir a snake_case para el backend (el backend espera snake_case)
    const registerDto: any = {
      email: data.email.trim(),
      password: data.password,
      business_name: data.businessName.trim(),
      industry: data.industry.trim(),
      provider_name: data.providerName.trim(),
      provider_specialty: data.providerSpecialty?.trim() || ''
    };

    console.log('DTO de registro a enviar:', registerDto);

    this.authService.register(registerDto).subscribe({
      next: (response) => {
        console.log('✅ Registro exitoso:', response);
        console.log('✅ Token guardado:', this.authService.getToken() ? 'SI' : 'NO');
        console.log('✅ Usuario autenticado:', this.authService.isAuthenticated());
        this.isSubmitting.set(false);

        // Pequeño delay para asegurar que el token se guarde correctamente
        setTimeout(() => {
          console.log('🚀 Navegando al dashboard...');
          this.router.navigate(['/dashboard']);
        }, 100);
      },
      error: (error) => {
        console.error('❌ Error en registro:', error);
        this.isSubmitting.set(false);

        // Manejo específico de errores
        if (error.status === 409) {
          // Conflicto - Email ya registrado
          this.errorMessage.set('El email ya está registrado. Por favor, usa otro o inicia sesión.');
        } else if (error.status === 400) {
          // Validación - Mostrar el mensaje específico del backend
          const detail = error.error?.detail || error.error?.message;
          this.errorMessage.set(detail || 'Hay errores en el formulario. Por favor, revisa los datos ingresados.');
        } else if (error.status === 500) {
          // Error interno del servidor
          this.errorMessage.set('Ocurrió un error en el servidor. Por favor, intenta de nuevo más tarde.');
        } else {
          // Error genérico
          this.errorMessage.set(
            error?.error?.detail || error?.error?.message || 'Error al registrar. Por favor, intenta de nuevo.'
          );
        }
      }
    });
  }

  onLoginClick(): void {
    this.loginClick.emit();
  }
}
