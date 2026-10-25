
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import {
  IonContent,
  IonItem,
  IonIcon,
  IonInput,
  IonButton,
  AlertController
} from '@ionic/angular';

import { AuthService, RespuestaAPI, Sesion } from '../../services/auth.service';


@Component({
  selector: 'app-login',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    IonContent,
    IonItem,
    IonIcon,
    IonInput,
    IonButton
  ]
})
export class LoginPage {

  private authService = inject(AuthService);
  private router = inject(Router);
  private alertController = inject(AlertController);
  private route = inject(ActivatedRoute);

  correo = signal('');
  password = signal('');

  mensaje = signal('');
  esError = signal(false);
  cargando = signal(false);

  // Al volver del registro se avisa que la cuenta ya está lista
  ionViewWillEnter(): void {
    if (this.route.snapshot.queryParamMap.get('registrado')) {
      this.mensaje.set('¡Cuenta creada! Inicia sesión para recibir tu regalo de bienvenida.');
      this.esError.set(false);
    }
  }


async iniciarSesion() {
  this.mensaje.set('');

  if (!this.correo().trim() || !this.password()) {
    this.mensaje.set('Ingresa tu correo y contraseña.');
    this.esError.set(true);
    return;
  }

  this.cargando.set(true);

  this.authService.login({
    correo: this.correo().trim(),
    password: this.password()
  }).subscribe({
    next: async (respuesta: RespuestaAPI<Sesion>) => {
      if (!respuesta.success || !respuesta.data) {
        this.cargando.set(false);

        const alerta = await this.alertController.create({
          header: 'Datos incorrectos',
          message: respuesta.error || 'No se pudo iniciar sesión. Verifica tus datos.',
          buttons: ['Aceptar']
        });

        await alerta.present();
        return;
      }

      this.authService.iniciarSesion(respuesta.data);

      this.correo.set('');
      this.password.set('');

      this.esError.set(false);
      this.cargando.set(false);

      await this.router.navigateByUrl('/home', { replaceUrl: true });
    },

    error: async (error: any) => {
      this.cargando.set(false);

      if (error.status === 401) {
        const alerta = await this.alertController.create({
          header: 'Datos incorrectos',
          message: 'El correo electrónico o la contraseña son incorrectos. Verifica tus datos e inténtalo de nuevo.',
          buttons: ['Aceptar']
        });

        await alerta.present();
        return;
      }

      const alerta = await this.alertController.create({
        header: 'Error de conexión',
        message:
          error.error?.error ||
          error.error?.message ||
          'No se pudo conectar con el servidor. Inténtalo de nuevo.',
        buttons: ['Aceptar']
      });

      await alerta.present();
    }
  });
}
}
