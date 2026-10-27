import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../environments/environment';
import { ToastController } from '@ionic/angular';

const API_URL = `${environment.apiUrl}/Auth`;
const USUARIOS_URL = `${environment.apiUrl}/Usuarios`;

export interface Usuario {
  idUsuario: number;
  nombreUsuario: string;
  correo: string;
  saldoFichas: number;
  fechaRegistro: string;
  rol: string;
}

export interface Sesion {
  token: string;
  usuario: Usuario;
  primerIngreso: boolean;     // la primera vez se muestra la bienvenida
  fichasBienvenida: number;
}

export interface DatosRegistro {
  nombreUsuario: string;
  correo: string;
  password: string;
}

export interface RespuestaAPI<T> {
  success: boolean;
  data: T | null;
  error: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http = inject(HttpClient);
  private router = inject(Router);
  private toastController = inject(ToastController);

  // Usuario logueado, compartido por todas las pantallas (el saldo se ve igual en todas)
  private usuarioActual = signal<Usuario | null>(this.leerUsuarioGuardado());

  readonly usuario = this.usuarioActual.asReadonly();
  readonly esAdmin = computed(() => this.usuarioActual()?.rol === 'Admin');

  // LOGIN
  login(datos: {
    correo: string;
    password: string;
  }): Observable<RespuestaAPI<Sesion>> {

    return this.http.post<RespuestaAPI<Sesion>>(
      `${API_URL}/login`,
      datos
    );
  }

  // REGISTRO, PASO 1: el servidor valida los datos y manda un código al correo
  enviarCodigo(datos: DatosRegistro): Observable<RespuestaAPI<boolean>> {
    return this.http.post<RespuestaAPI<boolean>>(
      `${API_URL}/enviar-codigo`,
      datos
    );
  }

  // REGISTRO, PASO 2: con el código del correo se crea la cuenta
  registro(datos: DatosRegistro & { codigo: string }): Observable<RespuestaAPI<Sesion>> {

    return this.http.post<RespuestaAPI<Sesion>>(
      `${API_URL}/registro`,
      datos
    );
  }

  // OBTENER PERFIL DEL USUARIO LOGUEADO
  // El token lo agrega el authInterceptor.
  obtenerPerfil(): Observable<RespuestaAPI<Usuario>> {
    return this.http
      .get<RespuestaAPI<Usuario>>(`${USUARIOS_URL}/perfil`)
      .pipe(tap(respuesta => {
        if (respuesta.success && respuesta.data) {
          this.guardarUsuario(respuesta.data);
        }
      }));
  }

  actualizarNombreUsuario(nuevoNombre: string): Observable<RespuestaAPI<Usuario>> {
    return this.http
      .put<RespuestaAPI<Usuario>>(`${USUARIOS_URL}/nombre`, { nuevoNombre })
      .pipe(tap(respuesta => {
        if (respuesta.success && respuesta.data) {
          this.guardarUsuario(respuesta.data);
        }
      }));
  }

  // ================================
  // SESIÓN
  // ================================

  get token(): string | null {
    return sessionStorage.getItem('token');
  }

  estaLogueado(): boolean {
    return !!this.token;
  }

  iniciarSesion(sesion: Sesion): void {
    sessionStorage.setItem('token', sesion.token);
    this.guardarUsuario(sesion.usuario);

    // El inicio muestra la bienvenida una sola vez
    if (sesion.primerIngreso) {
      sessionStorage.setItem('bienvenida', String(sesion.fichasBienvenida));
    }
  }

  // Fichas del regalo de bienvenida pendiente de mostrar (null si ya se mostró)
  tomarBienvenida(): number | null {
    const fichas = sessionStorage.getItem('bienvenida');
    sessionStorage.removeItem('bienvenida');
    return fichas === null ? null : Number(fichas);
  }

  guardarUsuario(usuario: Usuario): void {
    sessionStorage.setItem('usuario', JSON.stringify(usuario));
    this.usuarioActual.set(usuario);
  }

  // Después de apostar, comprar o cobrar: el saldo cambia en todas las pantallas
  actualizarSaldo(saldo: number): void {
    const usuario = this.usuarioActual();

    if (usuario) {
      this.guardarUsuario({ ...usuario, saldoFichas: saldo });
    }
  }

  // motivo: si viene, se muestra un aviso arriba (ej. "Se inició sesión en otro dispositivo")
  cerrarSesion(motivo?: string): void {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('usuario');
    sessionStorage.removeItem('bienvenida');
    this.usuarioActual.set(null);

    this.router.navigateByUrl('/login', { replaceUrl: true });

    if (motivo) {
      this.toastController
        .create({ message: motivo, duration: 4000, color: 'warning', position: 'top' })
        .then(toast => toast.present());
    }
  }

  private leerUsuarioGuardado(): Usuario | null {
    const guardado = sessionStorage.getItem('usuario');

    if (!guardado) {
      return null;
    }

    try {
      return JSON.parse(guardado) as Usuario;
    } catch {
      sessionStorage.removeItem('usuario');
      return null;
    }
  }
}
