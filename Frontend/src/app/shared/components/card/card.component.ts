import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';

/**
 * Componente genérico de Card
 * Usa PrimeNG Card con estilos de Agendify
 */
@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule, CardModule],
  templateUrl: './card.component.html',
  styleUrl: './card.component.css'
})
export class CardComponent {
  @Input() header?: string;
  @Input() subheader?: string;
  @Input() customClass: string = '';
  @Input() headerTemplate: boolean = false;
  @Input() footerTemplate: boolean = false;
}
