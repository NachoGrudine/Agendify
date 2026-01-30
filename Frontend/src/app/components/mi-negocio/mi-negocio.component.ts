import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
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
  // Iconos Lucide
  readonly StoreIcon = Store;
  readonly SettingsIcon = Settings;
}
