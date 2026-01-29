import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Clock, CheckCircle } from 'lucide-angular';

export interface TimeSlot {
  start: string; // "09:00"
  end: string;   // "10:00"
  available: boolean;
}

@Component({
  selector: 'app-time-slot-picker',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './time-slot-picker.component.html',
  styleUrl: './time-slot-picker.component.css'
})
export class TimeSlotPickerComponent {
  readonly ClockIcon = Clock;
  readonly CheckIcon = CheckCircle;

  @Input() slots: TimeSlot[] = [];
  @Input() selectedSlot: TimeSlot | null = null;
  @Input() loading = false;
  @Output() slotSelected = new EventEmitter<TimeSlot>();

  selectSlot(slot: TimeSlot): void {
    if (!slot.available) return;
    this.slotSelected.emit(slot);
  }

  formatTimeRange(slot: TimeSlot): string {
    return `${slot.start} - ${slot.end}`;
  }
}
