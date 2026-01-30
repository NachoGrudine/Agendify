import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { LucideAngularModule, LucideIconData, Eye, EyeOff } from 'lucide-angular';

/**
 * Componente genérico de Password Input
 * Usa input nativo con toggle de visibilidad
 */
@Component({
  selector: 'app-password-input',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './password-input.component.html',
  styleUrl: './password-input.component.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PasswordInputComponent),
      multi: true
    }
  ]
})
export class PasswordInputComponent implements ControlValueAccessor {
  @Input() inputId: string = `password-${Math.random().toString(36).substr(2, 9)}`;
  @Input() label?: string;
  @Input() placeholder: string = '••••••••';
  @Input() disabled: boolean = false;
  @Input() required: boolean = false;
  @Input() error?: string;
  @Input() hint?: string;
  @Input() feedback: boolean = true;
  @Input() toggleMask: boolean = true;
  @Input() weakLabel: string = 'Débil';
  @Input() mediumLabel: string = 'Media';
  @Input() strongLabel: string = 'Fuerte';
  @Input() promptLabel: string = 'Ingresa una contraseña';
  @Input() iconStart?: LucideIconData;
  @Input() iconSize: number = 18;

  @Output() valueChange = new EventEmitter<string>();

  // Iconos
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;

  value: string = '';
  showPassword: boolean = false;
  onChange: any = () => {};
  onTouched: any = () => {};

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

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

  onPasswordChange(value: string): void {
    this.onChange(value);
    this.valueChange.emit(value);
  }
}
