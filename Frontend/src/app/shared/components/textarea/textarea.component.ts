import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Textarea } from 'primeng/textarea';
import { LucideAngularModule, LucideIconData } from 'lucide-angular';

/**
 * Componente genérico de Textarea
 * Usa PrimeNG InputTextarea con estilos de Agendify
 */
@Component({
  selector: 'app-textarea',
  standalone: true,
  imports: [CommonModule, FormsModule, Textarea, LucideAngularModule],
  templateUrl: './textarea.component.html',
  styleUrl: './textarea.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TextareaComponent),
      multi: true
    }
  ]
})
export class TextareaComponent implements ControlValueAccessor {
  @Input() inputId: string = `textarea-${Math.random().toString(36).substr(2, 9)}`;
  @Input() label?: string;
  @Input() placeholder: string = '';
  @Input() disabled: boolean = false;
  @Input() required: boolean = false;
  @Input() error?: string;
  @Input() hint?: string;
  @Input() rows: number = 3;
  @Input() autoResize: boolean = false;
  @Input() maxLength?: number;
  @Input() showCount: boolean = false;
  @Input() iconStart?: LucideIconData;
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

  onTextareaChange(value: string): void {
    this.onChange(value);
    this.valueChange.emit(value);
  }

  get characterCount(): string {
    if (!this.maxLength) return '';
    return `${this.value.length} / ${this.maxLength}`;
  }
}
