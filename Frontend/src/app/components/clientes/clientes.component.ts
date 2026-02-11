import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { LucideAngularModule, UserPlus, Search, Edit, Trash2, Mail, Phone, Users } from 'lucide-angular';
import { ButtonComponent, InputComponent, ToastComponent, DialogComponent, TableComponent, TableColumn, TableAction } from '../../shared/components';
import { CustomerService } from '../../services/customer/customer.service';
import { CustomerResponse, CreateCustomerDto, UpdateCustomerDto } from '../../models/appointment.model';
import { ConfirmService } from '../../shared/services/confirm.service';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule,
    ButtonComponent,
    InputComponent,
    ToastComponent,
    DialogComponent,
    TableComponent
  ],
  templateUrl: './clientes.component.html',
  styleUrls: ['./clientes.component.css'],
  providers: [MessageService]
})
export class ClientesComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly messageService = inject(MessageService);
  private readonly confirmService = inject(ConfirmService);

  // Icons
  readonly UsersIcon = Users;
  readonly UserPlusIcon = UserPlus;
  readonly SearchIcon = Search;
  readonly EditIcon = Edit;
  readonly TrashIcon = Trash2;
  readonly MailIcon = Mail;
  readonly PhoneIcon = Phone;

  // Signals
  customers = signal<CustomerResponse[]>([]);
  filteredCustomers = signal<CustomerResponse[]>([]);
  isLoading = signal<boolean>(false);
  searchTerm = signal<string>('');

  // Paginación
  currentPage = signal<number>(1);
  pageSize = signal<number>(5);
  readonly pageSizeOptions = [5, 10, 15, 25, 50];

  // Dialog states
  showFormDialog = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  // Form fields
  selectedCustomer = signal<CustomerResponse | null>(null);
  customerName = signal<string>('');
  customerPhone = signal<string>('');
  customerEmail = signal<string>('');

  errors = signal<{
    name?: string;
    phone?: string;
    email?: string;
  }>({});

  // Table configuration
  tableColumns: TableColumn[] = [
    { field: 'name', header: 'Nombre' },
    { field: 'phone', header: 'Teléfono' },
    { field: 'email', header: 'Email' }
  ];

  tableActions: TableAction[] = [
    {
      icon: Edit,
      label: 'Editar',
      severity: 'primary',
      onClick: (row: any) => this.onEdit(row)
    },
    {
      icon: Trash2,
      label: 'Eliminar',
      severity: 'danger',
      onClick: (row: any) => this.onDelete(row)
    }
  ];

  // Computed properties para paginación
  paginatedCustomers = computed(() => {
    const customers = this.filteredCustomers();
    const page = this.currentPage();
    const size = this.pageSize();

    const startIndex = (page - 1) * size;
    const endIndex = startIndex + size;

    return customers.slice(startIndex, endIndex);
  });

  totalPages = computed(() => {
    const total = this.filteredCustomers().length;
    const size = this.pageSize();
    return Math.ceil(total / size);
  });

  hasPreviousPage = computed(() => this.currentPage() > 1);
  hasNextPage = computed(() => this.currentPage() < this.totalPages());

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading.set(true);
    this.customerService.getAll().subscribe({
      next: (data) => {
        this.customers.set(data);
        this.filteredCustomers.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error al cargar clientes:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudieron cargar los clientes'
        });
        this.isLoading.set(false);
      }
    });
  }

  onSearch(): void {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) {
      this.filteredCustomers.set(this.customers());
    } else {
      const filtered = this.customers().filter(customer =>
        customer.name.toLowerCase().includes(term) ||
        customer.email?.toLowerCase().includes(term) ||
        customer.phone?.includes(term)
      );
      this.filteredCustomers.set(filtered);
    }
    // Resetear a página 1 al buscar
    this.currentPage.set(1);
  }

  onCreateCustomer(): void {
    this.isEditMode.set(false);
    this.selectedCustomer.set(null);
    this.clearForm();
    this.showFormDialog.set(true);
  }

  onEdit(customer: CustomerResponse): void {
    this.isEditMode.set(true);
    this.selectedCustomer.set(customer);
    this.customerName.set(customer.name);
    this.customerPhone.set(customer.phone || '');
    this.customerEmail.set(customer.email || '');
    this.showFormDialog.set(true);
  }

  async onDelete(customer: CustomerResponse): Promise<void> {
    const confirmed = await this.confirmService.confirmDelete(customer.name);

    if (!confirmed) {
      return;
    }

    this.customerService.delete(customer.id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Cliente eliminado correctamente'
        });
        this.loadCustomers();
      },
      error: (error) => {
        console.error('Error al eliminar cliente:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo eliminar el cliente'
        });
      }
    });
  }

  validateForm(): boolean {
    const newErrors: any = {};

    // Validar nombre
    const name = this.customerName().trim();
    if (!name) {
      newErrors.name = 'El nombre es obligatorio';
    } else if (name.length < 2) {
      newErrors.name = 'El nombre debe tener al menos 2 caracteres';
    } else if (name.length > 100) {
      newErrors.name = 'El nombre no puede exceder 100 caracteres';
    }

    // Validar email (opcional pero si existe debe ser válido)
    const email = this.customerEmail().trim();
    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = 'El email no es válido';
    }

    // Validar teléfono (opcional)
    const phone = this.customerPhone().trim();
    if (phone && phone.length < 6) {
      newErrors.phone = 'El teléfono debe tener al menos 6 dígitos';
    }

    this.errors.set(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  onSubmit(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isSaving.set(true);

    const dto = {
      name: this.customerName().trim(),
      phone: this.customerPhone().trim() || undefined,
      email: this.customerEmail().trim() || undefined
    };

    if (this.isEditMode()) {
      this.updateCustomer(dto as UpdateCustomerDto);
    } else {
      this.createCustomer(dto as CreateCustomerDto);
    }
  }

  createCustomer(dto: CreateCustomerDto): void {
    this.customerService.create(dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Cliente creado correctamente'
        });
        this.isSaving.set(false);
        this.showFormDialog.set(false);
        this.loadCustomers();
      },
      error: (error) => {
        console.error('Error al crear cliente:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo crear el cliente'
        });
        this.isSaving.set(false);
      }
    });
  }

  updateCustomer(dto: UpdateCustomerDto): void {
    const id = this.selectedCustomer()?.id;
    if (!id) return;

    this.customerService.update(id, dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Cliente actualizado correctamente'
        });
        this.isSaving.set(false);
        this.showFormDialog.set(false);
        this.loadCustomers();
      },
      error: (error) => {
        console.error('Error al actualizar cliente:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo actualizar el cliente'
        });
        this.isSaving.set(false);
      }
    });
  }

  cancelForm(): void {
    this.showFormDialog.set(false);
    this.clearForm();
  }


  clearForm(): void {
    this.customerName.set('');
    this.customerPhone.set('');
    this.customerEmail.set('');
    this.errors.set({});
  }

  getDialogTitle(): string {
    return this.isEditMode() ? 'Editar Cliente' : 'Nuevo Cliente';
  }

  // Métodos de paginación
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  previousPage(): void {
    if (this.hasPreviousPage()) {
      this.currentPage.update(page => page - 1);
    }
  }

  nextPage(): void {
    if (this.hasNextPage()) {
      this.currentPage.update(page => page + 1);
    }
  }

  changePageSize(newSize: number): void {
    this.pageSize.set(newSize);
    this.currentPage.set(1);
  }

  getPageNumbers(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];

    let startPage = Math.max(1, current - 2);
    let endPage = Math.min(total, current + 2);

    if (endPage - startPage < 4) {
      if (startPage === 1) {
        endPage = Math.min(5, total);
      } else if (endPage === total) {
        startPage = Math.max(1, total - 4);
      }
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }
}
