import { Component, inject, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { LucideAngularModule, Calendar, Users, Briefcase, BarChart3, Settings, LogOut, Clock, Wrench } from 'lucide-angular';

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
  readonly ClockIcon = Clock;
  readonly WrenchIcon = Wrench;
  readonly SettingsIcon = Settings;
  readonly LogOutIcon = LogOut;

  menuItems = [
    { icon: this.CalendarIcon, label: 'Agenda', route: '/dashboard/agenda', active: true },
    { icon: this.ClockIcon, label: 'Mis Horarios', route: '/dashboard/horarios', active: false },
    { icon: this.WrenchIcon, label: 'Servicios', route: '/dashboard/servicios', active: false },
    { icon: this.UsersIcon, label: 'Clientes', route: '/dashboard/clientes', active: false },
    { icon: this.BriefcaseIcon, label: 'Empleados', route: '/dashboard/empleados', active: false },
    { icon: this.BarChart3Icon, label: 'Reportes', route: '/dashboard/reportes', active: false }
  ];

  /**
   * Obtiene el índice de la ruta activa actual
   */
  private getCurrentMenuIndex(): number {
    const currentRoute = this.router.url;
    return this.menuItems.findIndex(item => currentRoute.includes(item.route));
  }

  /**
   * Navega al índice especificado del menú
   */
  private navigateToMenuIndex(index: number): void {
    if (index >= 0 && index < this.menuItems.length) {
      const menuItem = this.menuItems[index];
      this.router.navigate([menuItem.route]);

      // Cerrar sidebar en mobile si está abierto
      if (this.isOpen) {
        this.closeRequest.emit();
      }
    }
  }

  /**
   * Maneja atajos de teclado:
   * - Alt + (1-6) para navegar por las secciones
   * - Alt + ↑ para subir a la sección anterior
   * - Alt + ↓ para bajar a la siguiente sección
   */
  @HostListener('window:keydown', ['$event'])
  handleKeyboardShortcut(event: KeyboardEvent): void {
    // Solo procesar si Alt está presionado (sin Ctrl ni Shift para evitar conflictos)
    if (!event.altKey || event.ctrlKey || event.shiftKey) {
      return;
    }

    // Alt + Flecha arriba: ir a la sección anterior
    if (event.key === 'ArrowUp') {
      event.preventDefault();
      const currentIndex = this.getCurrentMenuIndex();
      const newIndex = currentIndex - 1;

      // Si estamos en la primera sección, ir a la última (circular)
      const targetIndex = newIndex < 0 ? this.menuItems.length - 1 : newIndex;
      this.navigateToMenuIndex(targetIndex);
      return;
    }

    // Alt + Flecha abajo: ir a la siguiente sección
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      const currentIndex = this.getCurrentMenuIndex();
      const newIndex = currentIndex + 1;

      // Si estamos en la última sección, ir a la primera (circular)
      const targetIndex = newIndex >= this.menuItems.length ? 0 : newIndex;
      this.navigateToMenuIndex(targetIndex);
      return;
    }

    // Alt + número (1-6): ir a la sección específica
    const keyNumber = parseInt(event.key);
    if (!isNaN(keyNumber) && keyNumber >= 1 && keyNumber <= 6) {
      event.preventDefault();
      this.navigateToMenuIndex(keyNumber - 1);
    }
  }


  onNavItemClick() {
    // Cerrar sidebar en mobile cuando se hace click en un item
    this.closeRequest.emit();
  }

  logout(): void {
    this.authService.logout();
  }
}
