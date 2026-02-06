import { Component, signal, OnInit, HostListener, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastComponent, MobileBlockComponent, ConfirmDialogComponent } from './shared/components';
import { ConfirmService } from './shared/services/confirm.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, ToastComponent, MobileBlockComponent, ConfirmDialogComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('Frontend');
  protected isMobileDevice = signal(false);

  // Inyectar el servicio de confirmación
  private confirmService = inject(ConfirmService);
  protected confirmState = this.confirmService.confirmState;

  ngOnInit(): void {
    this.checkScreenSize();
  }

  @HostListener('window:resize')
  onResize(): void {
    this.checkScreenSize();
  }

  private checkScreenSize(): void {
    // Considera mobile cualquier pantalla menor a 1024px (típico tablet/mobile)
    const isMobile = window.innerWidth < 1024;
    this.isMobileDevice.set(isMobile);
  }

  // Métodos para manejar las respuestas del confirm dialog
  protected onConfirm(): void {
    this.confirmState().onConfirm?.();
  }

  protected onCancel(): void {
    this.confirmState().onCancel?.();
  }
}
