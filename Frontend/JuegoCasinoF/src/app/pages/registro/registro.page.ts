
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import {
  IonContent,
  IonItem,
  IonIcon,
  IonInput,
  IonButton
} from '@ionic/angular';

import {
  AuthService,
  RespuestaAPI,
  Sesion
} from '../../services/auth.service';

@Component({
  selector: 'app-registro',
  templateUrl: './registro.page.html',
  styleUrls: ['./registro.page.scss'],
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
export class RegistroPage {

  private authService = inject(AuthService);
  private router = inject(Router);

  nombreUsuario = signal('');
  correo = signal('');
  password = signal('');
  confirmarPassword = signal('');

  mensaje = signal('');
  esError = signal(false);
  cargando = signal(false);

  registrar(): void {
    this.mensaje.set('');

    // 1. Validar que todos los campos estén completos
    if (
      !this.nombreUsuario().trim() ||
      !this.correo().trim() ||
      !this.password() ||
      !this.confirmarPassword()
    ) {
      this.mensaje.set('Completa todos los campos.');
      this.esError.set(true);
      return;
    }

    // 2. Validar el formato del correo
    const correoValido =
      /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.correo().trim());

    if (!correoValido) {
      this.mensaje.set('Ingresa una dirección de correo válida.');
      this.esError.set(true);
      return;
    }

    // 3. Exigir al menos 8 caracteres en la contraseña
    if (this.password().length < 8) {
      this.mensaje.set('La contraseña debe tener al menos 8 caracteres.');
      this.esError.set(true);
      return;
    }

    // 4. Confirmar que las contraseñas coincidan
    if (this.password() !== this.confirmarPassword()) {
      this.mensaje.set('Las contraseñas no coinciden.');
      this.esError.set(true);
      return;
    }

    // 5. Enviar los datos a la API
    this.cargando.set(true);

    this.authService.registro({
      nombreUsuario: this.nombreUsuario().trim(),
      correo: this.correo().trim(),
      password: this.password()
    }).subscribe({

      next: (respuesta: RespuestaAPI<Sesion>) => {

        if (!respuesta.success || !respuesta.data) {
          this.mensaje.set(respuesta.error || 'No se pudo crear la cuenta.');

          this.esError.set(true);
          this.cargando.set(false);
          return;
        }

        // 6. Registro exitoso: no iniciar sesión automáticamente
        this.cargando.set(false);
        this.esError.set(false);

        // 7. Regresar a Login para iniciar sesión manualmente
        this.router.navigateByUrl('/login', {
          replaceUrl: true
        });
      },

      error: (error: any) => {
        this.mensaje.set(error.error?.error || error.error?.message || 'No se pudo conectar con el servidor. Inténtalo de nuevo.');

        this.esError.set(true);
        this.cargando.set(false);
      }

    });
  }
}
