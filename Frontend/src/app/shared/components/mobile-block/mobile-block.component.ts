import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Monitor, Calendar } from 'lucide-angular';

/**
 * Componente de bloqueo para dispositivos móviles
 * Se muestra cuando se accede desde un dispositivo con pantalla pequeña
 */
@Component({
  selector: 'app-mobile-block',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './mobile-block.component.html',
  styleUrl: './mobile-block.component.css'
})
export class MobileBlockComponent {
  protected readonly Monitor = Monitor;
  protected readonly Calendar = Calendar;
}
