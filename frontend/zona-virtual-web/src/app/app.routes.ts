import { Routes } from '@angular/router';
import { authGuard, perfilGuard } from './core/guards/auth.guard';
import { Perfil } from './core/models/models';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'pagador',
    canActivate: [authGuard, perfilGuard(Perfil.Pagador)],
    loadComponent: () => import('./features/pagador/pagador-dashboard.component').then(m => m.PagadorDashboardComponent)
  },
  {
    path: 'comercio',
    canActivate: [authGuard, perfilGuard(Perfil.Comercio)],
    loadComponent: () => import('./features/comercio/comercio-dashboard.component').then(m => m.ComercioDashboardComponent)
  },
  { path: '**', redirectTo: 'login' }
];
