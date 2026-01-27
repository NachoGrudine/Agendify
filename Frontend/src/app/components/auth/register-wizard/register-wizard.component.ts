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
    this.formData.update(current => ({ ...current, ...data }));
  }

  async onFinalSubmit(): Promise<void> {
    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const data = this.formData();
    const registerDto: RegisterDto = {
      email: data.email,
      password: data.password,
      businessName: data.businessName,
      industry: data.industry,
      providerName: data.providerName,
      providerSpecialty: data.providerSpecialty
    };

    this.authService.register(registerDto).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        // Redirigir al dashboard después del registro exitoso
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(
          error?.error?.message || 'Error al registrar. Por favor, intenta de nuevo.'
        );
      }
    });
  }

  onLoginClick(): void {
    this.loginClick.emit();
  }
}
