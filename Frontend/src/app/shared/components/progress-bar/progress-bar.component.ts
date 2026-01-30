import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Componente genérico de Progress Bar
 * Muestra una barra de progreso con label y porcentaje
 */
@Component({
  selector: 'app-progress-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './progress-bar.component.html',
  styleUrl: './progress-bar.component.css'
})
export class ProgressBarComponent {
  @Input() progress: number = 0; // 0-100
  @Input() label?: string;
  @Input() showPercentage: boolean = true;
  @Input() color: 'primary' | 'success' | 'warning' | 'danger' = 'primary';
  @Input() height: string = '6px';
  @Input() animated: boolean = true;
}
