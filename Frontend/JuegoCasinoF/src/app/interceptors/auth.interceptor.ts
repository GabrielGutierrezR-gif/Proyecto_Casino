import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

// Agrega el token a todas las peticiones a la API.
// Si la API responde 401 (token vencido o inválido), cierra la sesión y vuelve al login.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.token;

  const esApi = req.url.startsWith(environment.apiUrl);
  const esLogin = req.url.includes('/Auth/');

  if (token && esApi && !esLogin) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && esApi && !esLogin) {
        authService.cerrarSesion('Tu sesión terminó: venció o se abrió en otro dispositivo.');
      }

      return throwError(() => error);
    })
  );
};
