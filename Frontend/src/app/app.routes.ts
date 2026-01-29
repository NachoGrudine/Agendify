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
        loadComponent: () => import('./components/day-detail/day-detail').then(m => m.DayDetailComponent)
      },
      {
        path: 'nuevo-turno',
        loadComponent: () => import('./components/new-appointment/new-appointment.component').then(m => m.NewAppointmentComponent)
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
        path: 'reportes',
        loadComponent: () => import('./components/reportes/reportes.component').then(m => m.ReportesComponent)
      },
      {
        path: 'horarios',
        loadComponent: () => import('./components/weekly-schedule/weekly-schedule.component').then(m => m.WeeklyScheduleComponent)
      },
      {
        path: 'mi-negocio',
        loadComponent: () => import('./components/mi-negocio/mi-negocio.component').then(m => m.MiNegocioComponent)
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
