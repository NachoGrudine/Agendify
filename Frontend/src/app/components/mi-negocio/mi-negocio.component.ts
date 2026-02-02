import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, Store, Settings } from 'lucide-angular';
import { ButtonComponent, CardComponent } from '../../shared/components';

@Component({
  selector: 'app-mi-negocio',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, ButtonComponent, CardComponent],
  templateUrl: './mi-negocio.component.html',
  styleUrls: ['./mi-negocio.component.css']
})
export class MiNegocioComponent {
  private readonly router = inject(Router);

  // Iconos Lucide
  readonly StoreIcon = Store;
  readonly SettingsIcon = Settings;

  navigateToConfiguracion(): void {
    this.router.navigate(['/dashboard/mi-negocio/configuracion']);
  }
}
