import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from '../login/login.component';
import { RegisterWizardComponent } from '../register-wizard/register-wizard.component';
import { LucideAngularModule, Clock, CheckCircle, LogIn, UserPlus } from 'lucide-angular';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, LoginComponent, RegisterWizardComponent, LucideAngularModule],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css']
})
export class LandingComponent implements OnInit, OnDestroy {
  // Iconos de Lucide
  readonly ClockIcon = Clock;
  readonly CheckCircleIcon = CheckCircle;
  readonly LogInIcon = LogIn;
  readonly UserPlusIcon = UserPlus;
  showAuthDialog = signal<boolean>(false);
  authMode = signal<'login' | 'register'>('login');

  // Showcase de pantallas reales
  currentImageIndex = signal<number>(0);
  showcaseImages = [
    { type: 'image', src: 'assets/Calendar.png', alt: 'Vista de calendario mensual' },
    { type: 'image', src: 'assets/day-detail.png', alt: 'Detalle diario con métricas' },
    { type: 'image', src: 'assets/Services.png', alt: 'Gestión de servicios' },
    { type: 'image', src: 'assets/Schedules.png', alt: 'Configuración de horarios' },
    { type: 'image', src: 'assets/Providers.png', alt: 'Gestión de proveedores' },
  ];
  private imageInterval: any;

  ngOnInit() {
    // Cambiar imagen automáticamente cada 4 segundos
    this.imageInterval = setInterval(() => {
      this.nextImage();
    }, 4000);
  }

  ngOnDestroy() {
    if (this.imageInterval) {
      clearInterval(this.imageInterval);
    }
  }

  nextImage() {
    const next = (this.currentImageIndex() + 1) % this.showcaseImages.length;
    this.currentImageIndex.set(next);
  }

  prevImage() {
    const prev = this.currentImageIndex() === 0
      ? this.showcaseImages.length - 1
      : this.currentImageIndex() - 1;
    this.currentImageIndex.set(prev);
  }

  goToImage(index: number) {
    this.currentImageIndex.set(index);
  }

  showAuthModal(mode: 'login' | 'register') {
    this.authMode.set(mode);
    this.showAuthDialog.set(true);
  }

  closeAuthModal() {
    this.showAuthDialog.set(false);
  }

  scrollToSection(event: Event, sectionId: string) {
    event.preventDefault();
    const element = document.getElementById(sectionId);
    if (element) {
      const headerOffset = 80; // altura del header fijo
      const elementPosition = element.getBoundingClientRect().top;
      const offsetPosition = elementPosition + window.scrollY - headerOffset;

      window.scrollTo({
        top: offsetPosition,
        behavior: 'smooth'
      });
    }
  }
}
