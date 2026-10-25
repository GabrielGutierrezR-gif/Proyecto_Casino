import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { RespuestaAPI } from './auth.service';

const ADMIN_URL = `${environment.apiUrl}/Admin`;

export interface Configuracion {
  clave: string;
  valor: string;
  descripcion: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  private http = inject(HttpClient);

  listarConfiguracion(): Observable<RespuestaAPI<Configuracion[]>> {
    return this.http.get<RespuestaAPI<Configuracion[]>>(`${ADMIN_URL}/configuracion`);
  }

  modificarConfiguracion(clave: string, valor: string): Observable<RespuestaAPI<Configuracion>> {
    return this.http.put<RespuestaAPI<Configuracion>>(
      `${ADMIN_URL}/configuracion/${encodeURIComponent(clave)}`,
      { valor }
    );
  }
}
