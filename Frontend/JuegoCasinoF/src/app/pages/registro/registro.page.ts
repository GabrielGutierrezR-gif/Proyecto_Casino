
import { Component, inject } from '@angular/core';
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

  nombreUsuario = '';
  correo = '';
  password = '';
  confirmarPassword = '';

  mensaje = '';
  esError = false;
  cargando = false;

  registrar(): void {
    this.mensaje = '';

    // 1. Validar que todos los campos estén completos
    if (
      !this.nombreUsuario.trim() ||
      !this.correo.trim() ||
      !this.password ||
      !this.confirmarPassword
    ) {
      this.mensaje = 'Completa todos los campos.';
      this.esError = true;
      return;
    }

    // 2. Validar el formato del correo
    const correoValido =
      /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.correo.trim());

    if (!correoValido) {
      this.mensaje = 'Ingresa una dirección de correo válida.';
      this.esError = true;
      return;
    }

    // 3. Exigir al menos 9 caracteres en la contraseña
    if (this.password.length < 8) {
      this.mensaje =
        'La contraseña debe tener al menos 8 caracteres.';
      this.esError = true;
      return;
    }

    // 4. Confirmar que las contraseñas coincidan
    if (this.password !== this.confirmarPassword) {
      this.mensaje = 'Las contraseñas no coinciden.';
      this.esError = true;
      return;
    }

    // 5. Enviar los datos a la API
    this.cargando = true;

    this.authService.registro({
      nombreUsuario: this.nombreUsuario.trim(),
      correo: this.correo.trim(),
      password: this.password
    }).subscribe({

      next: (respuesta: RespuestaAPI<Sesion>) => {

        if (!respuesta.success || !respuesta.data) {
          this.mensaje =
            respuesta.error || 'No se pudo crear la cuenta.';

          this.esError = true;
          this.cargando = false;
          return;
        }

        // 6. Registro exitoso: no iniciar sesión automáticamente
        this.cargando = false;
        this.esError = false;

        // 7. Regresar a Login para iniciar sesión manualmente
        this.router.navigateByUrl('/login', {
          replaceUrl: true
        });
      },

      error: (error: any) => {
        this.mensaje =
          error.error?.error ||
          error.error?.message ||
          'No se pudo conectar con el servidor. Inténtalo de nuevo.';

        this.esError = true;
        this.cargando = false;
      }

    });
  }
}
