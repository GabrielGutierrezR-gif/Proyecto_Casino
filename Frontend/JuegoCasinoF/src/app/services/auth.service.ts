
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const API_URL = 'https://localhost:7187/api/Auth';

export interface Usuario {
  idUsuario: number;
  nombreUsuario: string;
  correo: string;
  saldoFichas: number;
  fechaRegistro: string;
  rol: string;
}

// 4define los datos que devuelve el backend con el token
export interface Sesion {
  token: string;
  usuario: Usuario;
}

//permite leer la respuesta real del servidor
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

  //envia el correo y la contraseña al api
  login(datos: {
    correo: string;
    password: string;
  }): Observable<RespuestaAPI<Sesion>> {
    return this.http.post<RespuestaAPI<Sesion>>(
      `${API_URL}/login`,
      datos
    );
  }

  //envia el nombre de usuario, correo y contraseña al api para registrar un nuevo usuario
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
}
