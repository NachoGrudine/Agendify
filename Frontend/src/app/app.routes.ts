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
        loadComponent: () => import('./components/clientes/clientes.component').then(m => m.ClientesComponent)
      },
      {
        path: 'empleados',
        loadComponent: () => import('./components/empleados/empleados.component').then(m => m.EmpleadosComponent)
      },
      {
        path: 'horarios',
        loadComponent: () => import('./components/weekly-schedule/weekly-schedule.component').then(m => m.WeeklyScheduleComponent)
      },
      {
        path: 'servicios',
        loadComponent: () => import('./components/servicios/servicios.component').then(m => m.ServiciosComponent)
      },
      {
        path: 'servicios/nuevo',
        loadComponent: () => import('./components/servicios/service-form/service-form.component').then(m => m.ServiceFormComponent)
      },
      {
        path: 'servicios/editar/:id',
        loadComponent: () => import('./components/servicios/service-form/service-form.component').then(m => m.ServiceFormComponent)
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
