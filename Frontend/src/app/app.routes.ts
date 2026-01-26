import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
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
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];
