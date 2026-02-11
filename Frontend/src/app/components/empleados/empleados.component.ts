import { Component, signal, inject, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Briefcase, Plus, Trash2, Save, X, Clock, Copy, CheckCircle, XCircle, Search, User } from 'lucide-angular';
import { ProviderService } from '../../services/provider/provider.service';
import { ProviderResponse, CreateProviderDto } from '../../models/appointment.model';
import { ErrorHelper } from '../../helpers/error.helper';
import { ButtonComponent, LoadingSpinnerComponent, CardComponent, DialogComponent, InputComponent } from '../../shared/components';
import { ManageProviderComponent } from './manage-provider/manage-provider.component';

@Component({
  selector: 'app-empleados',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, ButtonComponent, LoadingSpinnerComponent, CardComponent, DialogComponent, InputComponent, ManageProviderComponent],
  templateUrl: './empleados.component.html',
  styleUrls: ['./empleados.component.css']
})
export class EmpleadosComponent implements OnInit {
  // Icons
  readonly BriefcaseIcon = Briefcase;
  readonly PlusIcon = Plus;
  readonly SaveIcon = Save;
  readonly XIcon = X;
  readonly CheckCircleIcon = CheckCircle;
  readonly XCircleIcon = XCircle;
  readonly SearchIcon = Search;
  readonly UserIcon = User;

  // Services
  private readonly providerService = inject(ProviderService);

  // State signals
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  successMessage = signal<string>('');
  errorMessage = signal<string>('');
  searchTerm = signal<string>('');

  // Data
  providers = signal<ProviderResponse[]>([]);

  // Dialog states
  showProviderDialog = signal<boolean>(false);
  showManageDialog = signal<boolean>(false);
  currentProvider = signal<ProviderResponse | null>(null);

  // Form data
  providerForm = signal<CreateProviderDto>({
    name: '',
    specialty: '',
    isActive: true
  });

  // Computed
  filteredProviders = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) return this.providers().filter(p => p.isActive);

    return this.providers().filter(p =>
      p.isActive && (
        p.name.toLowerCase().includes(term) ||
        p.specialty.toLowerCase().includes(term)
      )
    );
  });

  ngOnInit(): void {
    this.loadProviders();
  }

  // Provider CRUD
  loadProviders(): void {
    this.isLoading.set(true);
    this.providerService.getAll().subscribe({
      next: (providers) => {
        this.providers.set(providers);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(ErrorHelper.extractErrorMessage(error, 'Error al cargar los empleados'));
        this.isLoading.set(false);
      }
    });
  }

  openCreateDialog(): void {
    this.providerForm.set({
      name: '',
      specialty: '',
      isActive: true
    });
    this.showProviderDialog.set(true);
  }

  closeProviderDialog(): void {
    this.showProviderDialog.set(false);
    this.clearMessages();
  }

  openManageDialog(provider: ProviderResponse): void {
    this.currentProvider.set(provider);
    this.showManageDialog.set(true);
  }

  closeManageDialog(): void {
    this.showManageDialog.set(false);
    this.currentProvider.set(null);
    this.clearMessages();
  }

  saveProvider(): void {
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

    this.providerService.create(this.providerForm()).subscribe({
      next: (created) => {
        this.providers.set([...this.providers(), created]);
        this.successMessage.set('Empleado creado exitosamente');
        this.isSaving.set(false);
        setTimeout(() => {
          this.closeProviderDialog();
          this.clearMessages();
        }, 1000);
      },
      error: (error) => {
        this.errorMessage.set(ErrorHelper.extractErrorMessage(error, 'Error al crear el empleado'));
        this.isSaving.set(false);
      }
    });
  }

  onProviderUpdated(updated: ProviderResponse): void {
    const providersList = this.providers().map(p => p.id === updated.id ? updated : p);
    this.providers.set(providersList);
  }

  onProviderDeleted(providerId: number): void {
    this.providers.set(this.providers().filter(p => p.id !== providerId));
  }

  updateFormField(field: keyof CreateProviderDto, value: any): void {
    this.providerForm.set({
      ...this.providerForm(),
      [field]: value
    });
  }

  private clearMessages(): void {
    this.successMessage.set('');
    this.errorMessage.set('');
  }
}

