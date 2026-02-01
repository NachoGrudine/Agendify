import { Component, input } from '@angular/core';
import { LucideAngularModule, LucideIconData } from 'lucide-angular';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-section-icon',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './section-icon.component.html',
  styleUrls: ['./section-icon.component.css']
})
export class SectionIconComponent {
  icon = input.required<LucideIconData>();
  size = input<number>(28);
}
