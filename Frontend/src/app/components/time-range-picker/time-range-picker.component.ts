import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Clock, Plus, Minus } from 'lucide-angular';

export interface TimeRange {
  startTime: string; // "09:00"
  endTime: string;   // "10:30"
}

@Component({
  selector: 'app-time-range-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './time-range-picker.component.html',
  styleUrl: './time-range-picker.component.css'
})
export class TimeRangePickerComponent {
  readonly ClockIcon = Clock;
  readonly PlusIcon = Plus;
  readonly MinusIcon = Minus;

  @Input() startTime: string = '09:00';
  @Input() endTime: string = '10:00';
  @Input() minTime: string = '00:00';
  @Input() maxTime: string = '23:59';
  @Input() step: number = 15; // Incremento en minutos
  @Output() timeRangeChange = new EventEmitter<TimeRange>();

  // Duraciones rápidas predefinidas (en minutos)
  quickDurations = [30, 45, 60, 90, 120];

  onStartTimeChange(newStartTime: string): void {
    this.startTime = newStartTime;
    this.emitChange();
  }

  onEndTimeChange(newEndTime: string): void {
    this.endTime = newEndTime;
    this.emitChange();
  }

  adjustStartTime(minutes: number): void {
    const [hours, mins] = this.startTime.split(':').map(Number);
    const totalMinutes = hours * 60 + mins + minutes;
    this.startTime = this.minutesToTime(totalMinutes);
    this.emitChange();
  }

  adjustEndTime(minutes: number): void {
    const [hours, mins] = this.endTime.split(':').map(Number);
    const totalMinutes = hours * 60 + mins + minutes;
    this.endTime = this.minutesToTime(totalMinutes);
    this.emitChange();
  }

  setQuickDuration(durationMinutes: number): void {
    const [hours, mins] = this.startTime.split(':').map(Number);
    const startTotalMinutes = hours * 60 + mins;
    const endTotalMinutes = startTotalMinutes + durationMinutes;
    this.endTime = this.minutesToTime(endTotalMinutes);
    this.emitChange();
  }

  private minutesToTime(totalMinutes: number): string {
    // Mantener dentro de rango 0-1439 (23:59)
    totalMinutes = Math.max(0, Math.min(1439, totalMinutes));
    const hours = Math.floor(totalMinutes / 60);
    const mins = totalMinutes % 60;
    return `${String(hours).padStart(2, '0')}:${String(mins).padStart(2, '0')}`;
  }

  private emitChange(): void {
    this.timeRangeChange.emit({
      startTime: this.startTime,
      endTime: this.endTime
    });
  }

  get durationMinutes(): number {
    const [startH, startM] = this.startTime.split(':').map(Number);
    const [endH, endM] = this.endTime.split(':').map(Number);
    return (endH * 60 + endM) - (startH * 60 + startM);
  }

  get durationFormatted(): string {
    const duration = this.durationMinutes;
    if (duration < 0) return 'Fin debe ser mayor';
    const hours = Math.floor(duration / 60);
    const mins = duration % 60;
    if (hours === 0) return `${mins}min`;
    if (mins === 0) return `${hours}h`;
    return `${hours}h ${mins}min`;
  }
}
