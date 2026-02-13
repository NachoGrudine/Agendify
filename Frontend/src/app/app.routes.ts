import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () => import('./components/auth/landing/landing.component').then(m => m.LandingComponent)
  },
  {
    path: 'login',
    redirectTo: 'auth',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'agenda',
        loadComponent: () => import('./components/agenda/agenda.component').then(m => m.AgendaComponent)
      },
      {
        path: 'agenda/dia',
        loadComponent: () => import('./components/agenda/day-detail/day-detail').then(m => m.DayDetailComponent)
      },
      {
        path: 'clientes',
        loadComponent: () => import('./components/customers/customers.component').then(m => m.CustomersComponent)
      },
      {
        path: 'empleados',
        loadComponent: () => import('./components/employees/employees.component').then(m => m.EmployeesComponent)
      },
      {
        path: 'horarios',
        loadComponent: () => import('./components/weekly-schedule/weekly-schedule.component').then(m => m.WeeklyScheduleComponent)
      },
      {
        path: 'servicios',
        loadComponent: () => import('./components/services/services.component').then(m => m.ServicesComponent)
      },
      {
        path: 'servicios/nuevo',
        loadComponent: () => import('./components/services/service-form/service-form.component').then(m => m.ServiceFormComponent)
      },
      {
        path: 'servicios/editar/:id',
        loadComponent: () => import('./components/services/service-form/service-form.component').then(m => m.ServiceFormComponent)
      },
      {
        path: '',
        redirectTo: 'agenda',
        pathMatch: 'full'
      }
    ]
  },
  // Ruta legacy para compatibilidad
  {
    path: 'main',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: '',
    redirectTo: 'auth',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'auth'
  }
];
