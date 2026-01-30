import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, UserPlus, Briefcase } from 'lucide-angular';
import { ButtonComponent, CardComponent } from '../../shared/components';

@Component({
  selector: 'app-empleados',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, ButtonComponent, CardComponent],
  templateUrl: './empleados.component.html',
  styleUrls: ['./empleados.component.css']
})
export class EmpleadosComponent {
  // Iconos Lucide
  readonly UserPlusIcon = UserPlus;
  readonly BriefcaseIcon = Briefcase;

  empleados: any[] = [];

  nuevoEmpleado(): void {
    console.log('Abrir modal para nuevo empleado');
    // TODO: Implementar modal
  }
}
