import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { LucideAngularModule, LucideIconData } from 'lucide-angular';

/**
 * Componente genérico de Button
 * Usa PrimeNG Button con soporte para Lucide Icons y estilos de Agendify
 */
@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule, ButtonModule, LucideAngularModule],
  templateUrl: './button.component.html',
  styleUrl: './button.component.css'
})
export class ButtonComponent {
  @Input() label?: string;
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() severity: 'primary' | 'secondary' | 'success' | 'danger' | 'outlined' = 'primary';
  @Input() disabled: boolean = false;
  @Input() loading: boolean = false;
  @Input() iconStart?: LucideIconData;
  @Input() iconEnd?: LucideIconData;
  @Input() iconSize: number = 18;
  @Input() fullWidth: boolean = false;
  @Input() customClass: string = '';

  @Output() onClick = new EventEmitter<Event>();

  handleClick(event: Event): void {
    if (!this.disabled && !this.loading) {
      this.onClick.emit(event);
    }
  }
}
