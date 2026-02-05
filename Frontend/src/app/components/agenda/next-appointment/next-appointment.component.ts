import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Calendar } from 'lucide-angular';

@Component({
  selector: 'app-next-appointment',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './next-appointment.component.html',
  styleUrl: './next-appointment.component.css'
})
export class NextAppointmentComponent {
  readonly CalendarIcon = Calendar;
}
