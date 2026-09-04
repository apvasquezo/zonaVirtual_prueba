import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Perfil } from '../models/models';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.session()) return true;
  router.navigate(['/login']);
  return false;
};

export const perfilGuard = (perfilRequerido: Perfil): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const session = auth.session();
    if (session && session.perfil === perfilRequerido) return true;
    router.navigate(['/login']);
    return false;
  };
};
