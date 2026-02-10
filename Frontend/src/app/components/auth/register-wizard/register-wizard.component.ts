import { Component, signal, inject, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RegisterStep1Component } from './steps/step1/step1.component';
import { RegisterStep2Component } from './steps/step2/step2.component';
import { RegisterStep3Component } from './steps/step3/step3.component';
import { AuthService } from '../../../services/auth/auth.service';
import { RegisterDto } from '../../../models/auth.model';
import { ErrorHelper } from '../../../helpers/error.helper';
import { ProgressBarComponent } from '../../../shared/components';

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
    RegisterStep3Component,
    ProgressBarComponent
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
    this.formData.update(current => ({ ...current, ...data }));
  }

  onFinalSubmit(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const data = this.formData();

    if (!data.email || !data.password || !data.businessName || !data.industry || !data.providerName) {
      this.errorMessage.set('Por favor, completa todos los campos requeridos.');
      this.isSubmitting.set(false);
      return;
    }

    const registerDto: RegisterDto = {
      email: data.email.trim(),
      password: data.password,
      businessName: data.businessName.trim(),
      industry: data.industry.trim(),
      providerName: data.providerName.trim(),
      providerSpecialty: data.providerSpecialty?.trim() || ''
    };

    this.authService.register(registerDto).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 100);
      },
      error: (error) => {
        this.isSubmitting.set(false);

        if (error.status === 409) {
          this.errorMessage.set('El email ya está registrado. Por favor, usa otro o inicia sesión.');
        } else if (error.status === 400) {
          const detail = ErrorHelper.extractErrorMessage(error);
          this.errorMessage.set(detail || 'Hay errores en el formulario. Por favor, revisa los datos ingresados.');
        } else if (error.status === 500) {
          this.errorMessage.set('Ocurrió un error en el servidor. Por favor, intenta de nuevo más tarde.');
        } else {
          const detail = ErrorHelper.extractErrorMessage(error, 'Error al registrar. Por favor, intenta de nuevo.');
          this.errorMessage.set(detail);
        }
      }
    });
  }

  onLoginClick(): void {
    this.loginClick.emit();
  }
}
