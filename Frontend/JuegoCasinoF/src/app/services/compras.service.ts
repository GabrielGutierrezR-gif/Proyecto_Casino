import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { RespuestaAPI } from './auth.service';

const COMPRAS_URL = `${environment.apiUrl}/Compras`;

export interface PaqueteFichas {
  idPaquete: number;
  nombre: string;
  cantidadFichas: number;
  precio: number;
}

export interface MetodoPago {
  idMetodoPago: number;
  tipoTarjeta: string;
  numeroEnmascarado: string;
  nombreTitular: string;
}

export interface NuevoMetodoPago {
  tipoTarjeta: 'Credito' | 'Debito';
  numeroTarjeta: string;
  nombreTitular: string;
}

export interface CompraResultado {
  idCompra: number;
  paquete: string;
  fichasAgregadas: number;
  precioPagado: number;
  nuevoSaldo: number;
  fechaCompra: string;
}

@Injectable({
  providedIn: 'root'
})
export class ComprasService {

  private http = inject(HttpClient);

  listarPaquetes(): Observable<RespuestaAPI<PaqueteFichas[]>> {
    return this.http.get<RespuestaAPI<PaqueteFichas[]>>(`${COMPRAS_URL}/paquetes`);
  }

  listarMetodos(): Observable<RespuestaAPI<MetodoPago[]>> {
    return this.http.get<RespuestaAPI<MetodoPago[]>>(`${COMPRAS_URL}/metodos`);
  }

  // El backend solo guarda los últimos 4 dígitos
  agregarMetodo(datos: NuevoMetodoPago): Observable<RespuestaAPI<MetodoPago>> {
    return this.http.post<RespuestaAPI<MetodoPago>>(`${COMPRAS_URL}/metodos`, datos);
  }

  comprar(idPaquete: number, idMetodoPago: number): Observable<RespuestaAPI<CompraResultado>> {
    return this.http.post<RespuestaAPI<CompraResultado>>(COMPRAS_URL, { idPaquete, idMetodoPago });
  }
}
