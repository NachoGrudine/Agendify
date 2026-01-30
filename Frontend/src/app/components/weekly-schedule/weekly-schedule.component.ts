import { Component, signal, inject, computed, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Save, X, Trash2, Plus, Copy, Clock, Info, CheckCircle, XCircle } from 'lucide-angular';
import { DaySchedule, TimeRange, DayOfWeek } from '../../models/schedule.model';
import { ScheduleService } from '../../services/schedule/schedule.service';
import { ButtonComponent, LoadingSpinnerComponent, CardComponent } from '../../shared/components';

@Component({
  selector: 'app-weekly-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, ButtonComponent, LoadingSpinnerComponent, CardComponent],
  templateUrl: './weekly-schedule.component.html',
  styleUrls: ['./weekly-schedule.component.css']
})
export class WeeklyScheduleComponent implements OnInit {
  // Icons
  readonly SaveIcon = Save;
  readonly XIcon = X;
  readonly TrashIcon = Trash2;
  readonly PlusIcon = Plus;
  readonly CopyIcon = Copy;
  readonly ClockIcon = Clock;
  readonly InfoIcon = Info;
  readonly CheckCircleIcon = CheckCircle;
  readonly XCircleIcon = XCircle;

  // Services
  private readonly scheduleService = inject(ScheduleService);
  private readonly STORAGE_KEY = 'agendify_temp_schedule';

  // State signals
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  successMessage = signal<string>('');
  errorMessage = signal<string>('');

  // Data
  private originalSchedule: string = '';
  weekSchedule = signal<DaySchedule[]>([
    { dayName: 'Lunes', dayOfWeek: DayOfWeek.Monday, isOpen: false, slots: [] },
    { dayName: 'Martes', dayOfWeek: DayOfWeek.Tuesday, isOpen: false, slots: [] },
    { dayName: 'Miércoles', dayOfWeek: DayOfWeek.Wednesday, isOpen: false, slots: [] },
    { dayName: 'Jueves', dayOfWeek: DayOfWeek.Thursday, isOpen: false, slots: [] },
    { dayName: 'Viernes', dayOfWeek: DayOfWeek.Friday, isOpen: false, slots: [] },
    { dayName: 'Sábado', dayOfWeek: DayOfWeek.Saturday, isOpen: false, slots: [] },
    { dayName: 'Domingo', dayOfWeek: DayOfWeek.Sunday, isOpen: false, slots: [] }
  ]);

  // Computed
  hasUnsavedChanges = computed(() => {
    const current = JSON.stringify(this.weekSchedule());
    return current !== this.originalSchedule && this.originalSchedule !== '';
  });

  // Lifecycle
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any): void {
    if (this.hasUnsavedChanges()) {
      $event.returnValue = true;
    }
  }

  ngOnInit(): void {
    this.loadSchedules();
  }

  // Public methods - UI Actions
  toggleDay(dayIndex: number): void {
    const schedule = [...this.weekSchedule()];
    const day = schedule[dayIndex];

    day.isOpen = !day.isOpen;

    if (day.isOpen && day.slots.length === 0) {
      day.slots = [{ start: '09:00', end: '18:00' }];
    }

    if (!day.isOpen) {
      day.slots = [];
    }

    this.weekSchedule.set(schedule);
    this.saveToLocalStorage();
    this.clearMessages();
  }

  addSlot(dayIndex: number): void {
    const schedule = [...this.weekSchedule()];
    const day = schedule[dayIndex];

    const newSlot: TimeRange = { start: '09:00', end: '18:00' };

    // Validar que no exista ya un slot con el mismo rango horario
    const isDuplicate = day.slots.some(slot =>
      slot.start === newSlot.start && slot.end === newSlot.end
    );

    if (isDuplicate) {
      this.errorMessage.set('⚠️ Ya existe un horario con ese rango. Por favor, modifica los horarios.');
      setTimeout(() => this.clearMessages(), 3000);
      return;
    }

    // Validar que no se solape con otros horarios existentes
    const hasOverlap = day.slots.some(slot =>
      this.hasOverlap(newSlot, slot)
    );

    if (hasOverlap) {
      this.errorMessage.set('⚠️ El nuevo horario se solapa con uno existente. Ajusta los rangos.');
      setTimeout(() => this.clearMessages(), 3000);
      return;
    }

    day.slots.push(newSlot);

    this.weekSchedule.set(schedule);
    this.saveToLocalStorage();
    this.clearMessages();
  }

  removeSlot(dayIndex: number, slotIndex: number): void {
    const schedule = [...this.weekSchedule()];
    const day = schedule[dayIndex];

    day.slots.splice(slotIndex, 1);

    if (day.slots.length === 0) {
      day.isOpen = false;
    }

    this.weekSchedule.set(schedule);
    this.saveToLocalStorage();
    this.clearMessages();
  }

  updateSlot(dayIndex: number, slotIndex: number, field: 'start' | 'end', value: string): void {
    const schedule = [...this.weekSchedule()];
    const day = schedule[dayIndex];

    // Actualizar temporalmente el slot
    const updatedSlot = { ...day.slots[slotIndex] };
    updatedSlot[field] = value;

    // Validar que no se cree un duplicado
    const isDuplicate = day.slots.some((slot, index) =>
      index !== slotIndex &&
      slot.start === updatedSlot.start &&
      slot.end === updatedSlot.end
    );

    if (isDuplicate) {
      this.errorMessage.set('⚠️ Ya existe un horario idéntico.');
      setTimeout(() => this.clearMessages(), 3000);
      return;
    }

    // Validar que no se solape con otros horarios
    const hasOverlap = day.slots.some((slot, index) =>
      index !== slotIndex &&
      this.hasOverlap(updatedSlot, slot)
    );

    if (hasOverlap) {
      this.errorMessage.set('⚠️ El horario se solapa con otro existente.');
      setTimeout(() => this.clearMessages(), 3000);
      return;
    }

    // Aplicar el cambio
    schedule[dayIndex].slots[slotIndex][field] = value;
    this.weekSchedule.set(schedule);
    this.saveToLocalStorage();
    this.clearMessages();
  }

  copyToAll(dayIndex: number): void {
    const schedule = [...this.weekSchedule()];
    const sourceDayConfig = schedule[dayIndex];

    schedule.forEach((day, index) => {
      if (index !== dayIndex) {
        day.isOpen = sourceDayConfig.isOpen;
        day.slots = sourceDayConfig.slots.map(slot => ({ ...slot }));
      }
    });

    this.weekSchedule.set(schedule);
    this.saveToLocalStorage();
    this.successMessage.set(`Configuración del ${sourceDayConfig.dayName} copiada a todos los días`);
    setTimeout(() => this.clearMessages(), 3000);
  }

  async saveChanges(): Promise<void> {
    this.isSaving.set(true);
    this.clearMessages();

    // IMPORTANTE: Enviamos TODOS los horarios configurados (días abiertos con slots)
    // El backend eliminará TODOS los horarios existentes y los reemplazará con estos
    const schedulesToSave = this.weekSchedule()
      .filter(day => day.isOpen && day.slots.length > 0)
      .flatMap(day =>
        day.slots.map((slot: TimeRange) => ({
          dayOfWeek: day.dayOfWeek,
          startTime: `${slot.start}:00`,
          endTime: `${slot.end}:00`
        }))
      );

    const dto = {
      schedules: schedulesToSave
    };

    this.scheduleService.bulkUpdateMySchedules(dto).subscribe({
      next: () => {
        // CRÍTICO: Actualizar originalSchedule ANTES de mostrar éxito
        // Esto hace que hasUnsavedChanges() retorne false inmediatamente
        this.originalSchedule = JSON.stringify(this.weekSchedule());

        // Limpiar localStorage
        this.clearLocalStorage();

        // Forzar actualización del signal para que el computed se reevalúe
        this.weekSchedule.set([...this.weekSchedule()]);

        // Mostrar éxito
        this.successMessage.set('¡Cambios guardados exitosamente!');
        this.isSaving.set(false);

        // Limpiar mensaje después de 2 segundos
        setTimeout(() => this.clearMessages(), 2000);
      },
      error: (error: any) => {
        this.errorMessage.set(error?.error?.message || 'Error al guardar los cambios');
        this.isSaving.set(false);
      }
    });
  }

  discardChanges(): void {
    if (confirm('¿Descartar los cambios no guardados?')) {
      // Limpiar localStorage inmediatamente
      this.clearLocalStorage();
      this.clearMessages();

      // Cargar desde el backend
      this.isLoading.set(true);
      this.scheduleService.getMySchedules().subscribe({
        next: (schedules: any[]) => {
          this.mapBackendToFrontend(schedules);
          this.originalSchedule = JSON.stringify(this.weekSchedule());
          this.isLoading.set(false);

          // Mensaje breve
          this.successMessage.set('Cambios descartados');
          setTimeout(() => this.clearMessages(), 1500);
        },
        error: (err) => {
          this.errorMessage.set('Error al cargar los horarios');
          this.isLoading.set(false);
        }
      });
    }
  }

  isValidTimeRange(slot: TimeRange): boolean {
    return slot.start < slot.end;
  }

  hasValidationErrors(): boolean {
    return this.weekSchedule().some(day =>
      day.isOpen && day.slots.some(slot => !this.isValidTimeRange(slot))
    );
  }

  // Validar si dos rangos horarios se solapan
  private hasOverlap(slot1: TimeRange, slot2: TimeRange): boolean {
    return (slot1.start < slot2.end && slot1.end > slot2.start);
  }


  // Private methods - Data handling
  private loadSchedules(): void {
    const tempSchedule = this.loadFromLocalStorage();
    if (tempSchedule) {
      this.weekSchedule.set(tempSchedule);
      this.originalSchedule = JSON.stringify(tempSchedule);
      // NO mostrar mensaje de borrador cargado automáticamente
      return;
    }

    this.isLoading.set(true);
    this.scheduleService.getMySchedules().subscribe({
      next: (schedules: any[]) => {
        this.mapBackendToFrontend(schedules);
        this.originalSchedule = JSON.stringify(this.weekSchedule());
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Error al cargar los horarios');
        this.isLoading.set(false);
      }
    });
  }

  private mapBackendToFrontend(schedules: any[]): void {
    const scheduleMap = new Map<DayOfWeek, TimeRange[]>();

    // Mapeo de nombres de días a números (para cuando el backend envía strings)
    const dayNameToNumber: { [key: string]: DayOfWeek } = {
      'Sunday': DayOfWeek.Sunday,
      'Monday': DayOfWeek.Monday,
      'Tuesday': DayOfWeek.Tuesday,
      'Wednesday': DayOfWeek.Wednesday,
      'Thursday': DayOfWeek.Thursday,
      'Friday': DayOfWeek.Friday,
      'Saturday': DayOfWeek.Saturday
    };

    schedules.forEach((schedule) => {
      // El backend puede enviar dayOfWeek como número o como string ("Monday", "Tuesday", etc.)
      let rawDayOfWeek = schedule.day_of_week ?? schedule.dayOfWeek;
      let dayOfWeek: DayOfWeek;

      if (typeof rawDayOfWeek === 'string') {
        // Si es string, convertir a número usando el mapeo
        dayOfWeek = dayNameToNumber[rawDayOfWeek] ?? DayOfWeek.Sunday;
      } else {
        // Si ya es número, usarlo directamente
        dayOfWeek = rawDayOfWeek as DayOfWeek;
      }

      const startTime = schedule.start_time ?? schedule.startTime;
      const endTime = schedule.end_time ?? schedule.endTime;

      const timeRange: TimeRange = {
        start: this.formatTimeSpan(startTime),
        end: this.formatTimeSpan(endTime)
      };

      if (!scheduleMap.has(dayOfWeek)) {
        scheduleMap.set(dayOfWeek, []);
      }
      scheduleMap.get(dayOfWeek)!.push(timeRange);
    });

    const updatedSchedule = this.weekSchedule().map(day => {
      const slots = scheduleMap.get(day.dayOfWeek) || [];
      const isOpen = slots.length > 0;
      return {
        ...day,
        slots,
        isOpen
      };
    });


    this.weekSchedule.set(updatedSchedule);
  }

  private formatTimeSpan(timeSpan: string | undefined | null): string {
    if (!timeSpan) {
      return '00:00';
    }
    return timeSpan.substring(0, 5);
  }

  private loadFromLocalStorage(): DaySchedule[] | null {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        return JSON.parse(stored);
      }
    } catch {
      return null;
    }
    return null;
  }

  private saveToLocalStorage(): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.weekSchedule()));
    } catch {
      // Error silencioso
    }
  }

  private clearLocalStorage(): void {
    try {
      localStorage.removeItem(this.STORAGE_KEY);
    } catch {
      // Error silencioso
    }
  }

  private clearMessages(): void {
    this.successMessage.set('');
    this.errorMessage.set('');
  }
}
