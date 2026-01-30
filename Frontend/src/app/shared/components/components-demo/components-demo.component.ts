import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import {
  ButtonComponent,
  CardComponent,
  InputComponent,
  DialogComponent,
  ToastComponent,
  LoadingSpinnerComponent
} from '../index';
import { Save, User, Mail, Plus, Trash2 } from 'lucide-angular';

/**
 * Componente de ejemplo mostrando el uso de todos los componentes compartidos
 * Este archivo sirve como referencia para implementar los componentes en tu app
 */
@Component({
  selector: 'app-components-demo',
  standalone: true,
  imports: [
    FormsModule,
    ButtonComponent,
    CardComponent,
    InputComponent,
    DialogComponent,
    ToastComponent,
    LoadingSpinnerComponent
  ],
  templateUrl: './components-demo.component.html',
  styleUrl: './components-demo.component.css',
  providers: [MessageService]
})
export class ComponentsDemoComponent {
  // Iconos Lucide
  readonly Save = Save;
  readonly User = User;
  readonly Mail = Mail;
  readonly Plus = Plus;
  readonly Trash2 = Trash2;

  // Estados del formulario
  nombre = '';
  email = '';
  errorNombre = '';
  errorEmail = '';

  // Estados de carga
  guardando = false;
  cargandoPagina = false;

  // Estado del dialog
  showDialog = false;

  constructor(private messageService: MessageService) {}

  openDialog(): void {
    this.showDialog = true;
  }

  closeDialog(): void {
    this.showDialog = false;
  }

  guardar(): void {
    if (!this.validar()) {
      return;
    }

    this.guardando = true;

    // Simular llamada a API
    setTimeout(() => {
      this.guardando = false;

      this.messageService.add({
        severity: 'success',
        summary: '¡Éxito!',
        detail: 'Usuario guardado correctamente'
      });

      this.limpiarFormulario();
    }, 2000);
  }

  eliminar(): void {
    this.messageService.add({
      severity: 'warn',
      summary: 'Advertencia',
      detail: '¿Estás seguro de eliminar este elemento?'
    });
  }

  mostrarInfo(): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Información',
      detail: 'Estos son componentes genéricos de Agendify'
    });
  }

  mostrarError(): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Este es un mensaje de error de ejemplo'
    });
  }

  simularCarga(): void {
    this.cargandoPagina = true;

    setTimeout(() => {
      this.cargandoPagina = false;
      this.messageService.add({
        severity: 'success',
        summary: 'Completado',
        detail: 'La carga ha terminado'
      });
    }, 3000);
  }

  private validar(): boolean {
    this.errorNombre = '';
    this.errorEmail = '';

    if (!this.nombre.trim()) {
      this.errorNombre = 'El nombre es requerido';
      return false;
    }

    if (!this.email.trim()) {
      this.errorEmail = 'El email es requerido';
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email)) {
      this.errorEmail = 'El email no es válido';
      return false;
    }

    return true;
  }

  private limpiarFormulario(): void {
    this.nombre = '';
    this.email = '';
    this.errorNombre = '';
    this.errorEmail = '';
  }
}
