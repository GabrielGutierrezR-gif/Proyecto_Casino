import { Injectable, inject, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr';

import { environment } from '../../environments/environment';
import { AuthService, RespuestaAPI } from './auth.service';

// ================================
// Lo que manda el servidor (BlackjackHub / NotificadorSignalR)
// ================================

export interface CartaMesa {
  valor: string;            // A, 2-10, J, Q, K (vacío si está oculta)
  palo: string;             // Corazones, Diamantes, Treboles, Picas
  oculta: boolean;
}

export interface ManoMesa {
  cartas: CartaMesa[];
  puntuacion: number;
  apuesta: number;
  estado: string;           // Jugando, Plantado, Pasado, Blackjack
  esTurno: boolean;
  puedeDoblar: boolean;
  puedeDividir: boolean;
}

export interface JugadorMesa {
  idUsuario: number;
  nombre: string;
  conectado: boolean;
  yaAposto: boolean;
  manos: ManoMesa[];
}

export interface EstadoMesa {
  idSala: number;
  codigoSala: string;
  nombreSala: string;
  fase: 'Apuestas' | 'Jugando';
  numeroRonda: number;
  cartasDealer: CartaMesa[];
  puntosDealer: number | null;
  jugadores: JugadorMesa[];
  turnoIdUsuario: number | null;
  finTurnoUtc: string | null;
}

export interface ResultadoMano {
  idUsuario: number;
  nombre: string;
  cartas: CartaMesa[];
  puntuacion: number;
  apuesta: number;
  resultado: string;        // Gana, Pierde, Empate, Blackjack
  ganancia: number;
}

export interface ResultadoRonda {
  numeroRonda: number;
  cartasDealer: CartaMesa[];
  puntosDealer: number;
  resultados: ResultadoMano[];
}

export type AccionBlackjack = 'Pedir' | 'Plantarse' | 'Doblar' | 'Dividir';

const MAX_MENSAJES = 30;

// Conexión en tiempo real con la mesa de Blackjack (REQ4 / REQ5)
@Injectable({
  providedIn: 'root'
})
export class BlackjackHubService {

  private authService = inject(AuthService);

  private conexion: HubConnection | null = null;
  private idSalaActual: number | null = null;

  readonly estado = signal<EstadoMesa | null>(null);
  readonly ultimoResultado = signal<ResultadoRonda | null>(null);
  readonly mensajes = signal<string[]>([]);
  readonly conectado = signal(false);

  // ================================
  // CONEXIÓN
  // ================================

  async entrarMesa(idSala: number): Promise<RespuestaAPI<EstadoMesa>> {
    await this.conectar();

    this.idSalaActual = idSala;
    this.ultimoResultado.set(null);
    this.mensajes.set([]);

    const respuesta = await this.conexion!.invoke<RespuestaAPI<EstadoMesa>>('EntrarMesa', idSala);

    if (respuesta.success && respuesta.data) {
      this.estado.set(respuesta.data);
    }

    return respuesta;
  }

  async salirMesa(): Promise<void> {
    const idSala = this.idSalaActual;
    this.idSalaActual = null;

    try {
      if (idSala !== null && this.conexion?.state === HubConnectionState.Connected) {
        await this.conexion.invoke('SalirMesa', idSala);
      }
    } finally {
      await this.desconectar();
    }
  }

  // Cierra el socket sin salir de la sala (el jugador puede volver a entrar)
  async desconectar(): Promise<void> {
    const conexion = this.conexion;
    this.conexion = null;
    this.conectado.set(false);
    this.estado.set(null);

    if (conexion) {
      await conexion.stop();
    }
  }

  // ================================
  // JUGADAS
  // ================================

  apostar(monto: number): Promise<RespuestaAPI<boolean>> {
    return this.invocar('Apostar', monto);
  }

  iniciarRonda(): Promise<RespuestaAPI<boolean>> {
    return this.invocar('IniciarRonda');
  }

  accion(accion: AccionBlackjack): Promise<RespuestaAPI<boolean>> {
    return this.invocar(accion);
  }

  // ================================
  // APOYO
  // ================================

  private async invocar(metodo: string, ...args: unknown[]): Promise<RespuestaAPI<boolean>> {
    if (!this.conexion || this.idSalaActual === null ||
        this.conexion.state !== HubConnectionState.Connected) {
      return { success: false, data: null, error: 'No hay conexión con la mesa.' };
    }

    try {
      return await this.conexion.invoke<RespuestaAPI<boolean>>(metodo, this.idSalaActual, ...args);
    } catch {
      return { success: false, data: null, error: 'Se perdió la conexión con la mesa.' };
    }
  }

  private async conectar(): Promise<void> {
    if (this.conexion?.state === HubConnectionState.Connected) {
      return;
    }

    this.conexion = new HubConnectionBuilder()
      .withUrl(environment.hubBlackjackUrl, {
        // Los WebSockets no aceptan headers: el token va en la URL
        accessTokenFactory: () => this.authService.token ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(environment.production ? LogLevel.None : LogLevel.Warning)
      .build();

    this.conexion.on('EstadoMesa', (estado: EstadoMesa) => {
      this.estado.set(estado);

      // Al empezar una ronda nueva se limpia el resultado anterior
      if (estado.fase === 'Jugando') {
        this.ultimoResultado.set(null);
      }
    });

    this.conexion.on('RondaFinalizada', (resultado: ResultadoRonda) => {
      this.ultimoResultado.set(resultado);
    });

    this.conexion.on('Mensaje', (mensaje: string) => {
      this.mensajes.update(lista => [mensaje, ...lista].slice(0, MAX_MENSAJES));
    });

    this.conexion.on('SaldoActualizado', (saldo: number) => {
      this.authService.actualizarSaldo(saldo);
    });

    this.conexion.onreconnecting(() => this.conectado.set(false));

    // Al reconectar el servidor da una conexión nueva: hay que volver a sentarse
    this.conexion.onreconnected(async () => {
      this.conectado.set(true);

      if (this.idSalaActual !== null) {
        const respuesta = await this.conexion!.invoke<RespuestaAPI<EstadoMesa>>('EntrarMesa', this.idSalaActual);
        if (respuesta.success && respuesta.data) {
          this.estado.set(respuesta.data);
        }
      }
    });

    this.conexion.onclose(() => this.conectado.set(false));

    await this.conexion.start();
    this.conectado.set(true);
  }
}
