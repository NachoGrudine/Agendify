import { Component, signal, OnInit, HostListener } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastComponent, MobileBlockComponent } from './shared/components';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, ToastComponent, MobileBlockComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('Frontend');
  protected isMobileDevice = signal(false);

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
}
