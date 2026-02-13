import { Component, signal, inject, computed, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, User, Clock, Briefcase, Save, X, Copy, Plus, Trash2, CheckCircle, XCircle } from 'lucide-angular';
import { ProviderService } from '../../../services/provider/provider.service';
import { ScheduleService } from '../../../services/schedule/schedule.service';
import { ProviderResponse, UpdateProviderDto } from '../../../models/appointment.model';
import { DaySchedule, TimeRange, DayOfWeek } from '../../../models/schedule.model';
import { ErrorHelper } from '../../../helpers/error.helper';
import { ConfirmService } from '../../../shared/services/confirm.service';

@Component({
  selector: 'app-manage-provider',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './manage-provider.component.html',
  styleUrls: ['./manage-provider.component.css']
})
export class ManageProviderComponent implements OnInit {
  // Icons
  readonly UserIcon = User;
  readonly ClockIcon = Clock;
  readonly BriefcaseIcon = Briefcase;
  readonly SaveIcon = Save;
  readonly XIcon = X;
  readonly CopyIcon = Copy;
  readonly PlusIcon = Plus;
  readonly TrashIcon = Trash2;
  readonly CheckCircleIcon = CheckCircle;
  readonly XCircleIcon = XCircle;

  // Services
  private readonly providerService = inject(ProviderService);
  private readonly scheduleService = inject(ScheduleService);
  private readonly confirmService = inject(ConfirmService);

  // Inputs
  @Input() provider!: ProviderResponse;

  // Outputs
  @Output() providerUpdated = new EventEmitter<ProviderResponse>();
  @Output() providerDeleted = new EventEmitter<number>();
  @Output() close = new EventEmitter<void>();

  // State
  activeTab = signal<'profile' | 'schedule'>('profile');
  isSaving = signal<boolean>(false);
  successMessage = signal<string>('');
  errorMessage = signal<string>('');

  // Form data
  providerForm = signal<UpdateProviderDto>({
    name: '',
    specialty: '',
    isActive: true
  });

  // Schedule data
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
  hasUnsavedScheduleChanges = computed(() => {
    const current = JSON.stringify(this.weekSchedule());
    return current !== this.originalSchedule && this.originalSchedule !== '';
  });

  ngOnInit(): void {
    this.providerForm.set({
      name: this.provider.name,
      specialty: this.provider.specialty,
      isActive: this.provider.isActive
    });
    this.loadSchedule();
  }

  setActiveTab(tab: 'profile' | 'schedule'): void {
    this.activeTab.set(tab);
    this.clearMessages();
  }

  // Profile methods
  updateProvider(): void {
    this.clearMessages();

    if (!this.providerForm().name.trim()) {
      this.errorMessage.set('El nombre es requerido');
      return;
    }

    if (!this.providerForm().specialty.trim()) {
      this.errorMessage.set('La especialidad es requerida');
      return;
    }

    this.isSaving.set(true);

    this.providerService.update(this.provider.id, this.providerForm()).subscribe({
      next: (updated) => {
        this.provider = updated;
        this.providerUpdated.emit(updated);
        this.successMessage.set('Empleado actualizado exitosamente');
        this.isSaving.set(false);
        setTimeout(() => this.clearMessages(), 2000);
      },
      error: (error) => {
        this.errorMessage.set(ErrorHelper.extractErrorMessage(error, 'Error al actualizar el empleado'));
        this.isSaving.set(false);
      }
    });
  }

  async deleteProvider(): Promise<void> {
    const confirmed = await this.confirmService.confirm({
      header: 'Confirmar Eliminación',
      message: `¿Estás seguro de que deseas eliminar a <strong>${this.provider.name}</strong>?`,
      detail: 'Esta acción no se puede deshacer.',
      confirmLabel: 'Eliminar',
      cancelLabel: 'Cancelar',
      severity: 'danger'
    });

    if (confirmed) {
      this.providerService.delete(this.provider.id).subscribe({
        next: () => {
          this.providerDeleted.emit(this.provider.id);
          this.successMessage.set('Empleado eliminado exitosamente');
          setTimeout(() => this.close.emit(), 1000);
        },
        error: (error) => {
          this.errorMessage.set(ErrorHelper.extractErrorMessage(error, 'Error al eliminar el empleado'));
        }
      });
    }
  }

  // Schedule methods
  loadSchedule(): void {
    this.scheduleService.getProviderSchedules(this.provider.id).subscribe({
      next: (schedules: any[]) => {
        this.mapBackendToFrontend(schedules);
        this.originalSchedule = JSON.stringify(this.weekSchedule());
      },
      error: (error) => {
        this.errorMessage.set(ErrorHelper.extractErrorMessage(error, 'Error al cargar los horarios'));
      }
    });
  }

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
    this.clearMessages();
  }

  addSlot(dayIndex: number): void {
    const schedule = [...this.weekSchedule()];
    schedule[dayIndex].slots.push({ start: '09:00', end: '18:00' });
    this.weekSchedule.set(schedule);
    this.clearMessages();
  }

  removeSlot(dayIndex: number, slotIndex: number): void {
    const schedule = [...this.weekSchedule()];
    schedule[dayIndex].slots.splice(slotIndex, 1);

    if (schedule[dayIndex].slots.length === 0) {
      schedule[dayIndex].isOpen = false;
    }

    this.weekSchedule.set(schedule);
    this.clearMessages();
  }

  updateSlot(dayIndex: number, slotIndex: number, field: 'start' | 'end', value: string): void {
    const schedule = [...this.weekSchedule()];
    schedule[dayIndex].slots[slotIndex][field] = value;
    this.weekSchedule.set(schedule);
    this.clearMessages();
  }

  copyToAll(dayIndex: number): void {
    const schedule = [...this.weekSchedule()];
    const source = schedule[dayIndex];

    schedule.forEach((day, index) => {
      if (index !== dayIndex) {
        day.isOpen = source.isOpen;
        day.slots = source.slots.map(slot => ({ ...slot }));
      }
    });

    this.weekSchedule.set(schedule);
    this.successMessage.set(`Configuración del ${source.dayName} copiada a todos los días`);
    setTimeout(() => this.clearMessages(), 3000);
  }

  async saveSchedule(): Promise<void> {
    this.clearMessages();

    const validation = this.validateSchedules();
    if (!validation.isValid) {
      this.errorMessage.set(validation.errorMessage || 'Error de validación');
      return;
    }

    this.isSaving.set(true);

    const schedulesToSave = this.weekSchedule()
      .filter(day => day.isOpen && day.slots.length > 0)
      .flatMap(day =>
        day.slots.map(slot => ({
          dayOfWeek: day.dayOfWeek,
          startTime: `${slot.start}:00`,
          endTime: `${slot.end}:00`
        }))
      );

    this.scheduleService.bulkUpdateProviderSchedules(this.provider.id, { schedules: schedulesToSave }).subscribe({
      next: () => {
        this.originalSchedule = JSON.stringify(this.weekSchedule());
        this.weekSchedule.set([...this.weekSchedule()]);
        this.successMessage.set('¡Horarios guardados exitosamente!');
        this.isSaving.set(false);
        setTimeout(() => this.clearMessages(), 2000);
      },
      error: (error: any) => {
        this.errorMessage.set(ErrorHelper.extractErrorMessage(error, 'Error al guardar los horarios'));
        this.isSaving.set(false);
      }
    });
  }

  async discardSchedule(): Promise<void> {
    const confirmed = await this.confirmService.confirmDiscard();
    if (confirmed) {
      this.loadSchedule();
      this.successMessage.set('Cambios descartados');
      setTimeout(() => this.clearMessages(), 1500);
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

  updateFormField(field: keyof UpdateProviderDto, value: any): void {
    this.providerForm.set({
      ...this.providerForm(),
      [field]: value
    });
  }

  onClose(): void {
    this.close.emit();
  }

  // Private methods
  private validateSchedules(): { isValid: boolean; errorMessage?: string } {
    const errors: string[] = [];

    for (const day of this.weekSchedule()) {
      if (!day.isOpen || day.slots.length === 0) continue;

      for (let i = 0; i < day.slots.length; i++) {
        const slot = day.slots[i];
        if (!this.isValidTimeRange(slot)) {
          errors.push(`${day.dayName}: Horario ${i + 1} inválido (${slot.start} - ${slot.end})`);
        }
      }

      for (let i = 0; i < day.slots.length; i++) {
        for (let j = i + 1; j < day.slots.length; j++) {
          const slot1 = day.slots[i];
          const slot2 = day.slots[j];

          if (slot1.start === slot2.start && slot1.end === slot2.end) {
            errors.push(`${day.dayName}: Horarios duplicados`);
          }

          if (!(slot1.end <= slot2.start || slot1.start >= slot2.end)) {
            errors.push(`${day.dayName}: Horarios solapados`);
          }
        }
      }
    }

    if (errors.length > 0) {
      return {
        isValid: false,
        errorMessage: `⚠️ Errores:\n• ${errors.join('\n• ')}`
      };
    }

    return { isValid: true };
  }

  private mapBackendToFrontend(schedules: any[]): void {
    const scheduleMap = new Map<DayOfWeek, TimeRange[]>();

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
      let rawDayOfWeek = schedule.day_of_week ?? schedule.dayOfWeek;
      let dayOfWeek: DayOfWeek;

      if (typeof rawDayOfWeek === 'string') {
        dayOfWeek = dayNameToNumber[rawDayOfWeek] ?? DayOfWeek.Sunday;
      } else {
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
      return {
        ...day,
        slots,
        isOpen: slots.length > 0
      };
    });

    this.weekSchedule.set(updatedSchedule);
  }

  private formatTimeSpan(timeSpan: string | undefined | null): string {
    if (!timeSpan) return '00:00';
    return timeSpan.substring(0, 5);
  }

  private clearMessages(): void {
    this.successMessage.set('');
    this.errorMessage.set('');
  }
}
