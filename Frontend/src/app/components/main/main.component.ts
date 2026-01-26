import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.css']
})
export class MainComponent {
  private readonly authService = inject(AuthService);

  // Computed signal para obtener el email del usuario actual
  userEmail = computed(() => this.authService.currentUser()?.email ?? 'Usuario');

  logout(): void {
    this.authService.logout();
  }
}
