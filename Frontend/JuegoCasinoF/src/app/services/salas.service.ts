import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { RespuestaAPI } from './auth.service';

const SALAS_URL = `${environment.apiUrl}/Salas`;

export interface Sala {
  idSala: number;
  codigoSala: string;
  nombreSala: string;
  tipoSala: 'Publica' | 'Privada';
  estado: string;           // Esperando, Jugando, Cerrada
  cantidadJugadores: number;
  maxJugadores: number;
  jugadores: string[];
  fechaCreacion: string;
}

@Injectable({
  providedIn: 'root'
})
export class SalasService {

  private http = inject(HttpClient);

  listarPublicas(): Observable<RespuestaAPI<Sala[]>> {
    return this.http.get<RespuestaAPI<Sala[]>>(SALAS_URL);
  }

  // La sala donde ya estoy (para volver si cerré la app)
  miSala(): Observable<RespuestaAPI<Sala>> {
    return this.http.get<RespuestaAPI<Sala>>(`${SALAS_URL}/mia`);
  }

  obtener(idSala: number): Observable<RespuestaAPI<Sala>> {
    return this.http.get<RespuestaAPI<Sala>>(`${SALAS_URL}/${idSala}`);
  }

  crear(nombreSala: string, tipoSala: 'Publica' | 'Privada'): Observable<RespuestaAPI<Sala>> {
    return this.http.post<RespuestaAPI<Sala>>(SALAS_URL, { nombreSala, tipoSala });
  }

  unirse(codigoSala: string): Observable<RespuestaAPI<Sala>> {
    return this.http.post<RespuestaAPI<Sala>>(`${SALAS_URL}/unirse`, { codigoSala });
  }

  salir(idSala: number): Observable<RespuestaAPI<boolean>> {
    return this.http.post<RespuestaAPI<boolean>>(`${SALAS_URL}/${idSala}/salir`, {});
  }
}
