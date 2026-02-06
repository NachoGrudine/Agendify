import { Injectable, signal } from '@angular/core';
import { ConfirmSeverity } from '../components/confirm-dialog/confirm-dialog.component';

export interface ConfirmOptions {
  header?: string;
  message: string;
  detail?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  severity?: ConfirmSeverity;
}

export interface ConfirmState extends ConfirmOptions {
  visible: boolean;
  onConfirm?: () => void;
  onCancel?: () => void;
}

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  confirmState = signal<ConfirmState>({
    visible: false,
    message: '',
    header: 'Confirmar',
    confirmLabel: 'Confirmar',
    cancelLabel: 'Cancelar',
    severity: 'warning'
  });

  confirm(options: ConfirmOptions): Promise<boolean> {
    return new Promise((resolve) => {
      this.confirmState.set({
        visible: true,
        header: options.header || 'Confirmar',
        message: options.message,
        detail: options.detail,
        confirmLabel: options.confirmLabel || 'Confirmar',
        cancelLabel: options.cancelLabel || 'Cancelar',
        severity: options.severity || 'warning',
        onConfirm: () => {
          this.hide();
          resolve(true);
        },
        onCancel: () => {
          this.hide();
          resolve(false);
        }
      });
    });
  }

  confirmDelete(itemName: string): Promise<boolean> {
    return this.confirm({
      header: 'Confirmar Eliminación',
      message: `¿Estás seguro de que deseas eliminar <strong>${itemName}</strong>?`,
      detail: 'Esta acción no se puede deshacer.',
      confirmLabel: 'Eliminar',
      cancelLabel: 'Cancelar',
      severity: 'danger'
    });
  }

  confirmDiscard(): Promise<boolean> {
    return this.confirm({
      header: 'Descartar Cambios',
      message: '¿Descartar los cambios no guardados?',
      detail: 'Los cambios que no se hayan guardado se perderán.',
      confirmLabel: 'Descartar',
      cancelLabel: 'Cancelar',
      severity: 'warning'
    });
  }

  private hide(): void {
    this.confirmState.update(state => ({ ...state, visible: false }));
  }
}
