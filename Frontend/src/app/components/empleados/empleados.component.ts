import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Briefcase } from 'lucide-angular';

@Component({
  selector: 'app-empleados',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './empleados.component.html',
  styleUrls: ['./empleados.component.css']
})
export class EmpleadosComponent {
  // Iconos Lucide
  readonly BriefcaseIcon = Briefcase;
}
