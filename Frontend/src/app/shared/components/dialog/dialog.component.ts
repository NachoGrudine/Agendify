import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';

/**
 * Componente genérico de Dialog/Modal
 * Usa PrimeNG Dialog con estilos de Agendify
 */
@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [CommonModule, DialogModule],
  templateUrl: './dialog.component.html',
  styleUrl: './dialog.component.css'
})
export class DialogComponent {
  @Input() visible: boolean = false;
  @Input() header?: string;
  @Input() width: string = '50vw';
  @Input() modal: boolean = true;
  @Input() closable: boolean = true;
  @Input() draggable: boolean = false;
  @Input() resizable: boolean = false;
  @Input() customClass: string = '';
  @Input() headerTemplate: boolean = false;
  @Input() footerTemplate: boolean = false;

  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() onHide = new EventEmitter<void>();
}
