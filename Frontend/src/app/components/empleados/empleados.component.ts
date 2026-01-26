import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, UserPlus, Briefcase } from 'lucide-angular';

@Component({
  selector: 'app-empleados',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './empleados.component.html',
  styleUrls: ['./empleados.component.css']
})
export class EmpleadosComponent {
  // Iconos Lucide
  readonly UserPlusIcon = UserPlus;
  readonly BriefcaseIcon = Briefcase;

  empleados: any[] = [];
}
