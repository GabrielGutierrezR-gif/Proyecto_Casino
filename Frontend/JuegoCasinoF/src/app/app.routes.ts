
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'home',
    loadComponent: () =>
      import('./home/home.page').then((m) => m.HomePage),
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.page').then((m) => m.LoginPage),
  },
  {
    path: 'registro',
    loadComponent: () =>
      import('./pages/registro/registro.page').then((m) => m.RegistroPage),
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./pages/profile/profile.page').then((m) => m.ProfilePage),
  },
  {
    path: 'comprar-fichas',
    loadComponent: () =>
      import('./pages/comprar-fichas/comprar-fichas.page').then(
        (m) => m.ComprarFichasPage
      ),
  },
  {
  path: 'blackjack/salas',
  loadComponent: () =>
    import('./pages/blackjack/salas/salas.page')
      .then((m) => m.SalasPage),
},
{
  path: 'blackjack/crear-sala',
  loadComponent: () =>
    import('./pages/blackjack/crear-sala/crear-sala.page')
      .then((m) => m.CrearSalaPage),
},
{
  path: 'blackjack/sala',
  loadComponent: () =>
    import('./pages/blackjack/sala/sala.page')
      .then((m) => m.SalaPage),
},
{
  path: 'blackjack/mesa',
  loadComponent: () =>
    import('./pages/blackjack/mesa/mesa.page')
      .then((m) => m.MesaPage),
},
  {
    path: '**',
    redirectTo: 'login',
  },
];
