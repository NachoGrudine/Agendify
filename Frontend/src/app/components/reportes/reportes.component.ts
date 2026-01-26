import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, BarChart3 } from 'lucide-angular';

@Component({
  selector: 'app-reportes',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './reportes.component.html',
  styleUrls: ['./reportes.component.css']
})
export class ReportesComponent {
  // Iconos Lucide
  readonly BarChart3Icon = BarChart3;
}
