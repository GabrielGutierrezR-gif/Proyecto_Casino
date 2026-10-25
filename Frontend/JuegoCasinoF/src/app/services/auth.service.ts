import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

const API_URL = 'https://localhost:7187/api/Auth';
const USUARIOS_URL = 'https://localhost:7187/api/Usuarios';

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

  // REGISTRO
  registro(datos: {
    nombreUsuario: string;
    correo: string;
    password: string;
  }): Observable<RespuestaAPI<Sesion>> {

    return this.http.post<RespuestaAPI<Sesion>>(
      `${API_URL}/registro`,
      datos
    );
  }

  // OBTENER PERFIL DEL USUARIO LOGUEADO
  obtenerPerfil(): Observable<RespuestaAPI<Usuario>> {

    const token = sessionStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RespuestaAPI<Usuario>>(
      `${USUARIOS_URL}/perfil`,
      { headers }
    );
  }

  actualizarNombreUsuario(nuevoNombre: string): Observable<RespuestaAPI<Usuario>> {
    const token = sessionStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.put<RespuestaAPI<Usuario>>(
      'https://localhost:7187/api/Usuarios/nombre',
      { nuevoNombre },
      { headers }
    );
  }
}
