import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  menuItems = [
    { icon: '📅', label: 'Agenda', route: '/dashboard/agenda', active: true },
    { icon: '👥', label: 'Clientes', route: '/dashboard/clientes', active: false },
    { icon: '👔', label: 'Empleados', route: '/dashboard/empleados', active: false },
    { icon: '📊', label: 'Reportes', route: '/dashboard/reportes', active: false }
  ];

  logout(): void {
    this.authService.logout();
  }
}

