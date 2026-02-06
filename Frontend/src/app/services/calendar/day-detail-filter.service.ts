import { Injectable, signal } from '@angular/core';

/**
 * Servicio para manejar filtros y paginación de day-detail
 */
@Injectable({
  providedIn: 'root'
})
export class DayDetailFilterService {
  // Paginación
  currentPage = signal(1);
  pageSize = signal(15);
  readonly pageSizeOptions = [5, 10, 15, 25, 50];

  // Filtros
  showFilters = signal(false);
  searchText = signal('');
  filters = signal({
    status: '',
    startTimeFrom: '',
    startTimeTo: ''
  });

  // Estados disponibles
  readonly availableStatuses = [
    { value: '', label: 'Todos los Estados' },
    { value: 'Pending', label: 'Pendiente' },
    { value: 'Confirmed', label: 'Confirmado' },
    { value: 'Completed', label: 'Completado' },
    { value: 'Cancelled', label: 'Cancelado' }
  ];

  /**
   * Obtiene los parámetros de filtro actuales
   */
  getFilterParams(): any {
    const currentFilters = this.filters();
    return {
      status: currentFilters.status || undefined,
      startTimeFrom: currentFilters.startTimeFrom || undefined,
      startTimeTo: currentFilters.startTimeTo || undefined,
      searchText: this.searchText() || undefined
    };
  }

  /**
   * Limpia todos los filtros
   */
  clearFilters(): void {
    this.searchText.set('');
    this.filters.set({
      status: '',
      startTimeFrom: '',
      startTimeTo: ''
    });
    this.showFilters.set(false);
    this.currentPage.set(1);
  }

  /**
   * Cambia el tamaño de página y resetea a la primera página
   */
  changePageSize(newSize: number): void {
    this.pageSize.set(newSize);
    this.currentPage.set(1);
  }

  /**
   * Aplica filtros y resetea a la primera página
   */
  applyFilters(): void {
    this.currentPage.set(1);
  }

  /**
   * Alterna la visibilidad del panel de filtros
   */
  toggleFilters(): void {
    this.showFilters.set(!this.showFilters());
  }

  /**
   * Calcula los números de página a mostrar
   */
  calculatePageNumbers(totalPages: number): number[] {
    const current = this.currentPage();
    const pages: number[] = [];

    let startPage = Math.max(1, current - 2);
    let endPage = Math.min(totalPages, current + 2);

    if (endPage - startPage < 4) {
      if (startPage === 1) {
        endPage = Math.min(5, totalPages);
      } else if (endPage === totalPages) {
        startPage = Math.max(1, totalPages - 4);
      }
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  /**
   * Navega a una página específica
   */
  goToPage(page: number, totalPages: number): boolean {
    if (page < 1 || page > totalPages) return false;
    this.currentPage.set(page);
    return true;
  }

  /**
   * Navega a la página siguiente
   */
  nextPage(totalPages: number): boolean {
    if (this.currentPage() < totalPages) {
      return this.goToPage(this.currentPage() + 1, totalPages);
    }
    return false;
  }

  /**
   * Navega a la página anterior
   */
  previousPage(): boolean {
    if (this.currentPage() > 1) {
      return this.goToPage(this.currentPage() - 1, Number.MAX_SAFE_INTEGER);
    }
    return false;
  }
}
