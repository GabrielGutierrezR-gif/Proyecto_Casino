import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { RespuestaAPI } from './auth.service';

const RULETA_URL = `${environment.apiUrl}/Ruleta`;

export type TipoApuestaRuleta = 'Numero' | 'Color' | 'ParImpar' | 'Docena';

export interface ApuestaRuleta {
  tipoApuesta: TipoApuestaRuleta;
  valorApuesta: string;     // "17", "Rojo", "Par", "2"...
  monto: number;
}

export interface ResultadoApuestaRuleta extends ApuestaRuleta {
  resultado: string;        // Gana / Pierde
  ganancia: number;
}

export interface GiroResultado {
  idPartida: number;
  numeroGanador: number;
  colorGanador: string;
  totalApostado: number;
  totalGanado: number;
  nuevoSaldo: number;
  detalle: ResultadoApuestaRuleta[];
}

@Injectable({
  providedIn: 'root'
})
export class RuletaService {

  private http = inject(HttpClient);

  // El número ganador lo genera el servidor
  girar(apuestas: ApuestaRuleta[]): Observable<RespuestaAPI<GiroResultado>> {
    return this.http.post<RespuestaAPI<GiroResultado>>(`${RULETA_URL}/girar`, { apuestas });
  }
}
