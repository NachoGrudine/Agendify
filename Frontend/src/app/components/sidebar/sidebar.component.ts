import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LucideAngularModule, Calendar, Users, Briefcase, BarChart3, Settings, LogOut } from 'lucide-angular';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  @Input() isOpen = false;
  @Output() closeRequest = new EventEmitter<void>();

  // Iconos Lucide
  readonly CalendarIcon = Calendar;
  readonly UsersIcon = Users;
  readonly BriefcaseIcon = Briefcase;
  readonly BarChart3Icon = BarChart3;
  readonly SettingsIcon = Settings;
  readonly LogOutIcon = LogOut;

  menuItems = [
    { icon: this.CalendarIcon, label: 'Agenda', route: '/dashboard/agenda', active: true },
    { icon: this.UsersIcon, label: 'Clientes', route: '/dashboard/clientes', active: false },
    { icon: this.BriefcaseIcon, label: 'Empleados', route: '/dashboard/empleados', active: false },
    { icon: this.BarChart3Icon, label: 'Reportes', route: '/dashboard/reportes', active: false }
  ];

  onNavItemClick() {
    // Cerrar sidebar en mobile cuando se hace click en un item
    this.closeRequest.emit();
  }

  logout(): void {
    this.authService.logout();
  }
}
