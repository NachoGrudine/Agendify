import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, UserPlus, Users } from 'lucide-angular';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './clientes.component.html',
  styleUrls: ['./clientes.component.css']
})
export class ClientesComponent {
  // Iconos Lucide
  readonly UserPlusIcon = UserPlus;
  readonly UsersIcon = Users;

  clientes: any[] = [];
}
