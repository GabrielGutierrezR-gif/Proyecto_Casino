import { Component, OnDestroy, inject, signal } from '@angular/core';
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
  DatosRegistro,
  RespuestaAPI,
  Sesion
} from '../../services/auth.service';

const SEGUNDOS_REENVIO = 60;

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
export class RegistroPage implements OnDestroy {

  private authService = inject(AuthService);
  private router = inject(Router);

  // Paso 1: datos de la cuenta · Paso 2: código que llegó al correo
  paso = signal<'datos' | 'codigo'>('datos');

  nombreUsuario = signal('');
  correo = signal('');
  password = signal('');
  confirmarPassword = signal('');
  codigo = signal('');

  mensaje = signal('');
  esError = signal(false);
  cargando = signal(false);

  // Cuenta regresiva para poder pedir otro código
  segundosReenvio = signal(0);
  private reloj: ReturnType<typeof setInterval> | null = null;

  ngOnDestroy(): void {
    this.detenerReloj();
  }

  // ================================
  // PASO 1: validar y enviar el código
  // ================================

  enviarCodigo(): void {
    this.mensaje.set('');

    // 1. Validar que todos los campos estén completos
    if (
      !this.nombreUsuario().trim() ||
      !this.correo().trim() ||
      !this.password() ||
      !this.confirmarPassword()
    ) {
      this.mostrarError('Completa todos los campos.');
      return;
    }

    // 2. Validar el formato del correo
    const correoValido =
      /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.correo().trim());

    if (!correoValido) {
      this.mostrarError('Ingresa una dirección de correo válida.');
      return;
    }

    // 3. Exigir al menos 8 caracteres en la contraseña
    if (this.password().length < 8) {
      this.mostrarError('La contraseña debe tener al menos 8 caracteres.');
      return;
    }

    // 4. Confirmar que las contraseñas coincidan
    if (this.password() !== this.confirmarPassword()) {
      this.mostrarError('Las contraseñas no coinciden.');
      return;
    }

    // 5. El servidor revisa que el correo no exista y manda el código
    this.solicitarCodigo();
  }

  reenviarCodigo(): void {
    if (this.segundosReenvio() > 0 || this.cargando()) {
      return;
    }

    this.solicitarCodigo();
  }

  // Volver a los datos (por ejemplo, si escribió mal el correo)
  cambiarDatos(): void {
    this.paso.set('datos');
    this.codigo.set('');
    this.mensaje.set('');
  }

  // ================================
  // PASO 2: crear la cuenta con el código
  // ================================

  registrar(): void {
    const codigo = this.codigo().trim();

    if (!/^\d{6}$/.test(codigo)) {
      this.mostrarError('El código tiene 6 números.');
      return;
    }

    this.cargando.set(true);
    this.mensaje.set('');

    this.authService.registro({ ...this.datos(), codigo }).subscribe({

      next: (respuesta: RespuestaAPI<Sesion>) => {
        this.cargando.set(false);

        if (!respuesta.success || !respuesta.data) {
          this.mostrarError(respuesta.error || 'No se pudo crear la cuenta.');
          return;
        }

        // Registro exitoso: regresar a Login para iniciar sesión manualmente.
        // En ese primer ingreso se muestra la bienvenida con el regalo de fichas.
        this.router.navigate(['/login'], {
          replaceUrl: true,
          queryParams: { registrado: 1 }
        });
      },

      error: (error: any) => {
        this.cargando.set(false);
        this.mostrarError(this.textoError(error));
      }

    });
  }

  // ================================
  // APOYO
  // ================================

  private solicitarCodigo(): void {
    this.cargando.set(true);

    this.authService.enviarCodigo(this.datos()).subscribe({
      next: (respuesta) => {
        this.cargando.set(false);

        if (!respuesta.success) {
          this.mostrarError(respuesta.error || 'No se pudo enviar el código.');
          return;
        }

        this.paso.set('codigo');
        this.esError.set(false);
        this.mensaje.set(`Enviamos un código de 6 números a ${this.correo().trim()}.`);
        this.iniciarReloj();
      },
      error: (error: any) => {
        this.cargando.set(false);
        this.mostrarError(this.textoError(error));
      }
    });
  }

  private datos(): DatosRegistro {
    return {
      nombreUsuario: this.nombreUsuario().trim(),
      correo: this.correo().trim(),
      password: this.password()
    };
  }

  private mostrarError(texto: string): void {
    this.mensaje.set(texto);
    this.esError.set(true);
  }

  private textoError(error: any): string {
    return error.error?.error ||
      error.error?.message ||
      'No se pudo conectar con el servidor. Inténtalo de nuevo.';
  }

  private iniciarReloj(): void {
    this.detenerReloj();
    this.segundosReenvio.set(SEGUNDOS_REENVIO);

    this.reloj = setInterval(() => {
      this.segundosReenvio.update(s => s - 1);

      if (this.segundosReenvio() <= 0) {
        this.detenerReloj();
      }
    }, 1000);
  }

  private detenerReloj(): void {
    if (this.reloj) {
      clearInterval(this.reloj);
      this.reloj = null;
    }
  }
}
