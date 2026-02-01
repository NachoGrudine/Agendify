import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from '../login/login.component';
import { RegisterWizardComponent } from '../register-wizard/register-wizard.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, LoginComponent, RegisterWizardComponent],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css']
})
export class LandingComponent {
  showAuthDialog = signal<boolean>(false);
  authMode = signal<'login' | 'register'>('login');

  showAuthModal(mode: 'login' | 'register') {
    this.authMode.set(mode);
    this.showAuthDialog.set(true);
  }

  closeAuthModal() {
    this.showAuthDialog.set(false);
  }
}
