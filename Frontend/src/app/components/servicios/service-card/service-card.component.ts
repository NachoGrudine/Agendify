import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Edit, Trash2, Clock, DollarSign } from 'lucide-angular';
import { ServiceResponse } from '../../../models/service.model';

@Component({
  selector: 'app-service-card',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './service-card.component.html',
  styleUrls: ['./service-card.component.css']
})
export class ServiceCardComponent {
  @Input({ required: true }) service!: ServiceResponse;
  @Output() edit = new EventEmitter<ServiceResponse>();
  @Output() delete = new EventEmitter<ServiceResponse>();

  // Icons
  readonly EditIcon = Edit;
  readonly TrashIcon = Trash2;
  readonly ClockIcon = Clock;
  readonly DollarIcon = DollarSign;

  onEdit(): void {
    this.edit.emit(this.service);
  }

  onDelete(): void {
    this.delete.emit(this.service);
  }

  formatPrice(price: number | null): string {
    if (price === null) return 'Gratis';
    return `$${price.toFixed(2)}`;
  }

  formatDuration(minutes: number): string {
    if (minutes < 60) {
      return `${minutes} min`;
    }
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return mins > 0 ? `${hours}h ${mins}min` : `${hours}h`;
  }

  getPriceClass(): string {
    return this.service.price === null || this.service.price === 0 ? 'price-free' : 'price-paid';
  }
}
