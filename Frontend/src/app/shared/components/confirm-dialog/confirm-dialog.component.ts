import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogComponent } from '../dialog/dialog.component';
import { ButtonComponent } from '../button/button.component';
import { LucideAngularModule, AlertCircle, AlertTriangle, Info } from 'lucide-angular';

export type ConfirmSeverity = 'danger' | 'warning' | 'info';

/**
 * Componente de diálogo de confirmación
 * Usado para confirmar acciones críticas con el usuario
 */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, DialogComponent, ButtonComponent, LucideAngularModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css'
})
export class ConfirmDialogComponent {
  @Input() visible: boolean = false;
  @Input() header: string = 'Confirmar';
  @Input() message: string = '¿Estás seguro?';
  @Input() detail?: string;
  @Input() confirmLabel: string = 'Confirmar';
  @Input() cancelLabel: string = 'Cancelar';
  @Input() severity: ConfirmSeverity = 'warning';

  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();


  get confirmButtonSeverity(): 'primary' | 'danger' | 'secondary' {
    if (this.severity === 'danger') return 'danger';
    if (this.severity === 'info') return 'primary';
    return 'primary';
  }

  getIcon() {
    if (this.severity === 'danger') return AlertCircle;
    if (this.severity === 'warning') return AlertTriangle;
    return Info;
  }

  onConfirm(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.confirm.emit();
  }

  onCancel(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.cancel.emit();
  }
}
