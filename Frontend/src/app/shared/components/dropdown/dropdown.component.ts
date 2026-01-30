import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Select } from 'primeng/select';
import { LucideAngularModule, LucideIconData } from 'lucide-angular';

/**
 * Componente genérico de Dropdown/Select
 * Usa PrimeNG Dropdown con estilos de Agendify
 */
@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule, FormsModule, Select, LucideAngularModule],
  templateUrl: './dropdown.component.html',
  styleUrl: './dropdown.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DropdownComponent),
      multi: true
    }
  ]
})
export class DropdownComponent implements ControlValueAccessor {
  @Input() inputId: string = `dropdown-${Math.random().toString(36).substr(2, 9)}`;
  @Input() label?: string;
  @Input() placeholder: string = 'Seleccionar...';
  @Input() options: any[] = [];
  @Input() optionLabel: string = 'label';
  @Input() optionValue: string = 'value';
  @Input() disabled: boolean = false;
  @Input() required: boolean = false;
  @Input() error?: string;
  @Input() hint?: string;
  @Input() filter: boolean = false;
  @Input() filterPlaceholder: string = 'Buscar...';
  @Input() showClear: boolean = false;
  @Input() iconStart?: LucideIconData;
  @Input() iconSize: number = 18;

  @Output() valueChange = new EventEmitter<any>();
  @Output() onChange = new EventEmitter<any>();

  value: any = null;
  onChangeFn: any = () => {};
  onTouched: any = () => {};

  writeValue(value: any): void {
    this.value = value;
  }

  registerOnChange(fn: any): void {
    this.onChangeFn = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onDropdownChange(value: any): void {
    this.onChangeFn(value);
    this.valueChange.emit(value);
    this.onChange.emit(value);
  }
}
