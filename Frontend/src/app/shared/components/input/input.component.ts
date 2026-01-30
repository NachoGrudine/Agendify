import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { LucideAngularModule, LucideIconData } from 'lucide-angular';

/**
 * Componente genérico de Input
 * Usa PrimeNG InputText con soporte para Lucide Icons y estilos de Agendify
 */
@Component({
  selector: 'app-input',
  standalone: true,
  imports: [CommonModule, FormsModule, InputTextModule, LucideAngularModule],
  templateUrl: './input.component.html',
  styleUrl: './input.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true
    }
  ]
})
export class InputComponent implements ControlValueAccessor {
  @Input() inputId: string = `input-${Math.random().toString(36).substr(2, 9)}`;
  @Input() label?: string;
  @Input() type: string = 'text';
  @Input() placeholder: string = '';
  @Input() disabled: boolean = false;
  @Input() required: boolean = false;
  @Input() error?: string;
  @Input() hint?: string;
  @Input() iconStart?: LucideIconData;
  @Input() iconEnd?: LucideIconData;
  @Input() iconSize: number = 18;

  @Output() valueChange = new EventEmitter<string>();

  value: string = '';
  onChange: any = () => {};
  onTouched: any = () => {};

  writeValue(value: string): void {
    this.value = value || '';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInputChange(value: string): void {
    this.onChange(value);
    this.valueChange.emit(value);
  }
}
