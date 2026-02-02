import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Edit, Trash2 } from 'lucide-angular';

export interface TableColumn {
  field: string;
  header: string;
  width?: string;
  align?: 'left' | 'center' | 'right';
}

export interface TableAction {
  icon: any;
  label: string;
  severity?: 'primary' | 'danger' | 'secondary';
  onClick: (item: any) => void;
}

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.css']
})
export class TableComponent {
  // Icons
  readonly EditIcon = Edit;
  readonly TrashIcon = Trash2;

  columns = input.required<TableColumn[]>();
  data = input.required<any[]>();
  actions = input<TableAction[]>([]);
  loading = input<boolean>(false);
  emptyMessage = input<string>('No hay datos disponibles');
}
