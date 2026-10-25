import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { RespuestaAPI } from './auth.service';

const HISTORIAL_URL = `${environment.apiUrl}/Historial`;

export interface HistorialCompra {
  idCompra: number;
  fechaCompra: string;
  paquete: string;
  cantidadFichas: number;
  precio: number;
  tipoTarjeta: string;
  ultimosDigitos: string;
}

export interface HistorialJuego {
  juego: string;            // Ruleta o Blackjack
  fecha: string;
  detalle: string;
  resultadoJuego: string;
  apuesta: number;
  resultado: string;        // Gana, Pierde, Empate, Blackjack
  ganancia: number;
}

@Injectable({
  providedIn: 'root'
})
export class HistorialService {

  private http = inject(HttpClient);

  compras(cantidad = 10): Observable<RespuestaAPI<HistorialCompra[]>> {
    return this.http.get<RespuestaAPI<HistorialCompra[]>>(
      `${HISTORIAL_URL}/compras`,
      { params: { cantidad } }
    );
  }

  juegos(cantidad = 10): Observable<RespuestaAPI<HistorialJuego[]>> {
    return this.http.get<RespuestaAPI<HistorialJuego[]>>(
      `${HISTORIAL_URL}/juegos`,
      { params: { cantidad } }
    );
  }
}
