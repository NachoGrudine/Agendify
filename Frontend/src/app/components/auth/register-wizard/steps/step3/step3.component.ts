import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register-step3',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './step3.component.html',
  styleUrls: ['./step3.component.css']
})
export class RegisterStep3Component {
  formData = input.required<any>();
  isSubmitting = input<boolean>(false);
  back = output<void>();
  updateData = output<any>();
  submit = output<void>();

  providerName = signal<string>('');
  providerSpecialty = signal<string>('');
  avatarPreview = signal<string | null>(null);

  errors = signal<{
    providerName?: string;
  }>({});

  ngOnInit(): void {
    const data = this.formData();
    this.providerName.set(data.providerName || '');
    this.providerSpecialty.set(data.providerSpecialty || '');
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      const reader = new FileReader();

      reader.onload = (e) => {
        this.avatarPreview.set(e.target?.result as string);
      };

      reader.readAsDataURL(file);
    }
  }

  validateForm(): boolean {
    const newErrors: any = {};

    if (!this.providerName() || this.providerName().trim() === '') {
      newErrors.providerName = 'El nombre profesional es requerido';
    }

    this.errors.set(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  onSubmit(): void {
    if (this.validateForm()) {
      this.updateData.emit({
        providerName: this.providerName(),
        providerSpecialty: this.providerSpecialty()
      });
      this.submit.emit();
    }
  }

  onBack(): void {
    this.updateData.emit({
      providerName: this.providerName(),
      providerSpecialty: this.providerSpecialty()
    });
    this.back.emit();
  }
}
