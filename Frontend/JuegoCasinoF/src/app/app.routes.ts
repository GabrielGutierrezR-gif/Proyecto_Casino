import { Routes } from '@angular/router';

import { adminGuard, authGuard, invitadoGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    canActivate: [invitadoGuard],
    loadComponent: () =>
      import('./pages/login/login.page').then((m) => m.LoginPage),
  },
  {
    path: 'registro',
    canActivate: [invitadoGuard],
    loadComponent: () =>
      import('./pages/registro/registro.page').then((m) => m.RegistroPage),
  },
  {
    path: 'home',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./home/home.page').then((m) => m.HomePage),
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/profile/profile.page').then((m) => m.ProfilePage),
  },
  {
    path: 'comprar-fichas',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/comprar-fichas/comprar-fichas.page').then(
        (m) => m.ComprarFichasPage
      ),
  },
  {
    path: 'ruleta',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/ruleta/ruleta.page').then((m) => m.RuletaPage),
  },
  {
    path: 'blackjack/salas',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/blackjack/salas/salas.page').then((m) => m.SalasPage),
  },
  {
    path: 'blackjack/crear-sala',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/blackjack/crear-sala/crear-sala.page').then((m) => m.CrearSalaPage),
  },
  {
    path: 'blackjack/sala/:idSala',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/blackjack/sala/sala.page').then((m) => m.SalaPage),
  },
  {
    path: 'blackjack/mesa/:idSala',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/blackjack/mesa/mesa.page').then((m) => m.MesaPage),
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./pages/admin/admin.page').then((m) => m.AdminPage),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];
