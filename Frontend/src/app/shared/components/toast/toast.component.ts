import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';

/**
 * Componente genérico de Toast/Mensajes
 * Usa PrimeNG Toast con estilos de Agendify
 *
 * Para usar este componente:
 * 1. Agrégalo en tu template: <app-toast />
 * 2. Inyecta MessageService en tu componente
 * 3. Usa messageService.add({ severity: 'success', summary: 'Título', detail: 'Mensaje' })
 */
@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule, ToastModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.css'
})
export class ToastComponent {}
