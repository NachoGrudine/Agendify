import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

/**
 * Componente genérico de Loading Spinner
 * Usa PrimeNG ProgressSpinner con estilos de Agendify
 */
@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule],
  templateUrl: './loading-spinner.component.html',
  styleUrl: './loading-spinner.component.css'
})
export class LoadingSpinnerComponent {
  @Input() size: string = '50px';
  @Input() message: string = '';
  @Input() overlay: boolean = false;
}
