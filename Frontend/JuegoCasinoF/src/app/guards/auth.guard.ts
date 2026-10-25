import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';

// Solo deja entrar si hay sesión iniciada
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  return authService.estaLogueado()
    ? true
    : inject(Router).createUrlTree(['/login']);
};

// Login y registro: si ya inició sesión, lo manda directo al casino
export const invitadoGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  return authService.estaLogueado()
    ? inject(Router).createUrlTree(['/home'])
    : true;
};

// Panel de administración: solo rol Admin
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  if (!authService.estaLogueado()) {
    return inject(Router).createUrlTree(['/login']);
  }

  return authService.esAdmin()
    ? true
    : inject(Router).createUrlTree(['/home']);
};
