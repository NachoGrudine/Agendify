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

    console.log('Intentando login con:', { email: this.email, password: this.password });

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (response) => {
        console.log('Login exitoso:', response);
        if (response) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (error) => {
        console.error('Error en login:', error);
        if (error.status === 401) {
          this.errorMessage = 'Credenciales incorrectas';
        } else if (error.status === 400) {
          this.errorMessage = 'Por favor, completa todos los campos';
        } else {
          this.errorMessage = error.error?.message || 'Error al iniciar sesión';
        }
      }
    });
  }

  onRegisterClick(): void {
    this.registerClick.emit();
  }
}
