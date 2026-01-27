import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register-step1',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './step1.component.html',
  styleUrls: ['./step1.component.css']
})
export class RegisterStep1Component {
  formData = input.required<any>();
  next = output<void>();
  updateData = output<any>();
  loginClick = output<void>();

  showPassword = signal<boolean>(false);
  showConfirmPassword = signal<boolean>(false);

  email = signal<string>('');
  password = signal<string>('');
  confirmPassword = signal<string>('');

  errors = signal<{
    email?: string;
    password?: string;
    confirmPassword?: string;
  }>({});

  ngOnInit(): void {
    const data = this.formData();
    this.email.set(data.email || '');
    this.password.set(data.password || '');
    this.confirmPassword.set(data.confirmPassword || '');
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(v => !v);
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update(v => !v);
  }

  validateForm(): boolean {
    const newErrors: any = {};

    // Validar email
    if (!this.email()) {
      newErrors.email = 'El correo electrónico es requerido';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email())) {
      newErrors.email = 'Ingresa un correo electrónico válido';
    }

    // Validar contraseña
    if (!this.password()) {
      newErrors.password = 'La contraseña es requerida';
    } else if (this.password().length < 6) {
      newErrors.password = 'La contraseña debe tener al menos 6 caracteres';
    }

    // Validar confirmación de contraseña
    if (!this.confirmPassword()) {
      newErrors.confirmPassword = 'Debes confirmar tu contraseña';
    } else if (this.password() !== this.confirmPassword()) {
      newErrors.confirmPassword = 'Las contraseñas no coinciden';
    }

    this.errors.set(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  getPasswordStrength(): 'weak' | 'medium' | 'strong' {
    const pwd = this.password();
    if (pwd.length < 6) return 'weak';

    let strength = 0;
    if (pwd.length >= 8) strength++;
    if (/[A-Z]/.test(pwd)) strength++;
    if (/[a-z]/.test(pwd)) strength++;
    if (/[0-9]/.test(pwd)) strength++;
    if (/[^A-Za-z0-9]/.test(pwd)) strength++;

    if (strength <= 2) return 'weak';
    if (strength <= 3) return 'medium';
    return 'strong';
  }

  getPasswordStrengthLabel(): string {
    const strength = this.getPasswordStrength();
    if (strength === 'weak') return 'Débil';
    if (strength === 'medium') return 'Media';
    return 'Fuerte';
  }

  onContinue(): void {
    if (this.validateForm()) {
      this.updateData.emit({
        email: this.email(),
        password: this.password(),
        confirmPassword: this.confirmPassword()
      });
      this.next.emit();
    }
  }

  onLoginClick(): void {
    this.loginClick.emit();
  }
}
