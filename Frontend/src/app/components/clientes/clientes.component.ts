import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, UserPlus, Users } from 'lucide-angular';
import { ButtonComponent, CardComponent } from '../../shared/components';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, ButtonComponent, CardComponent],
  templateUrl: './clientes.component.html',
  styleUrls: ['./clientes.component.css']
})
export class ClientesComponent {
  // Iconos Lucide
  readonly UserPlusIcon = UserPlus;
  readonly UsersIcon = Users;

  clientes: any[] = [];

  nuevoCliente(): void {
    console.log('Abrir modal para nuevo cliente');
    // TODO: Implementar modal
  }
}
