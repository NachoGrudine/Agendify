import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth/auth.service';
import { InputComponent, ButtonComponent, PasswordInputComponent } from '../../../shared/components';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, InputComponent, ButtonComponent, PasswordInputComponent],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  registerClick = output<void>();

  email = '';
  password = '';
  errorMessage = '';

  onSubmit(): void {
    this.errorMessage = '';

    // Validaciones antes de enviar
    if (!this.email || !this.password) {
      this.errorMessage = 'Por favor, completa todos los campos';
      return;
    }

    if (!this.isValidEmail(this.email)) {
      this.errorMessage = 'Por favor, ingresa un email válido';
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = 'La contraseña debe tener al menos 6 caracteres';
      return;
    }

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (response) => {
        if (response) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (error) => {
        if (error.status === 401) {
          this.errorMessage = 'Credenciales incorrectas';
        } else if (error.status === 400) {
          this.errorMessage = 'Por favor, completa todos los campos correctamente';
        } else if (error.status === 500) {
          this.errorMessage = 'Error del servidor. Intenta de nuevo más tarde';
        } else {
          this.errorMessage = error.error?.message || 'Error al iniciar sesión';
        }
      }
    });
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  onRegisterClick(): void {
    this.registerClick.emit();
  }
}
