import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { IonContent, IonButton, AlertController } from '@ionic/angular';

import { AuthService } from '../../../services/auth.service';
import {
  AccionBlackjack,
  BlackjackHubService,
  CartaMesa,
  EstadoMesa
} from '../../../services/blackjack-hub.service';

interface Ficha {
  valor: number;
  imagen: string;
}

// Nombre del palo en el servidor → nombre del archivo en assets/cartas
const PALOS: Record<string, string> = {
  Corazones: 'hearts',
  Diamantes: 'diamonds',
  Treboles: 'clubs',
  Picas: 'spades'
};

const SONIDOS_CARTA = ['card-place-1.ogg', 'card-place-2.ogg', 'card-place-3.ogg', 'card-place-4.ogg'];

@Component({
  selector: 'app-mesa',
  templateUrl: './mesa.page.html',
  styleUrls: ['./mesa.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonContent,
    IonButton
  ]
})
export class MesaPage {

  private hub = inject(BlackjackHubService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private alertController = inject(AlertController);

  // Viene de la ruta /blackjack/mesa/:idSala
  idSala = input.required<string>();

  usuario = this.authService.usuario;

  estado = this.hub.estado;
  resultado = this.hub.ultimoResultado;
  mensajes = this.hub.mensajes;
  conectado = this.hub.conectado;

  cargando = signal(true);
  error = signal('');
  aviso = signal('');
  enviando = signal(false);

  // Fichas que el jugador va juntando antes de confirmar la apuesta
  apuestaPendiente = signal(0);

  segundosRestantes = signal<number | null>(null);
  private reloj: ReturnType<typeof setInterval> | null = null;
  private cartasVistas = 0;


  // ================================
  // FICHAS
  // ================================

  fichas: Ficha[] = [
    { valor: 10, imagen: 'assets/fichas/Azul.png' },
    { valor: 25, imagen: 'assets/fichas/Verde.png' },
    { valor: 50, imagen: 'assets/fichas/Negro.png' },
    { valor: 100, imagen: 'assets/fichas/Amarillo.png' }
  ];


  // ================================
  // DATOS CALCULADOS DE LA MESA
  // ================================

  yo = computed(() => {
    const id = this.usuario()?.idUsuario;
    return this.estado()?.jugadores.find(j => j.idUsuario === id) ?? null;
  });

  otrosJugadores = computed(() => {
    const id = this.usuario()?.idUsuario;
    return this.estado()?.jugadores.filter(j => j.idUsuario !== id) ?? [];
  });

  enApuestas = computed(() => this.estado()?.fase === 'Apuestas');

  esMiTurno = computed(() =>
    !!this.estado() && this.estado()!.turnoIdUsuario === this.usuario()?.idUsuario);

  manoEnTurno = computed(() => this.yo()?.manos.find(m => m.esTurno) ?? null);

  nombreTurno = computed(() => {
    const estado = this.estado();
    return estado?.jugadores.find(j => j.idUsuario === estado.turnoIdUsuario)?.nombre ?? '';
  });

  alguienAposto = computed(() => this.estado()?.jugadores.some(j => j.yaAposto) ?? false);

  // Resultados de MIS manos en la última ronda
  misResultados = computed(() => {
    const id = this.usuario()?.idUsuario;
    return this.resultado()?.resultados.filter(r => r.idUsuario === id) ?? [];
  });

  textoEstado = computed(() => {
    const estado = this.estado();

    if (!estado) {
      return 'Conectando...';
    }

    if (estado.fase === 'Apuestas') {
      return this.yo()?.yaAposto ? 'Esperando a los demás' : 'Haz tu apuesta';
    }

    return this.esMiTurno() ? 'Tu turno' : `Turno de ${this.nombreTurno() || 'el dealer'}`;
  });


  constructor() {
    // Sonido cuando aparecen cartas nuevas en la mesa
    effect(() => {
      const total = this.contarCartas(this.estado());

      if (total > this.cartasVistas) {
        this.reproducirSonido(SONIDOS_CARTA[total % SONIDOS_CARTA.length]);
      }

      this.cartasVistas = total;
    });

    // Cada vez que cambia el turno se recalcula el contador
    effect(() => {
      this.estado();
      this.actualizarReloj();
    });
  }


  // ================================
  // CICLO DE VIDA
  // ================================

  async ionViewWillEnter(): Promise<void> {
    this.cargando.set(true);
    this.error.set('');

    try {
      const respuesta = await this.hub.entrarMesa(Number(this.idSala()));

      if (!respuesta.success) {
        this.error.set(respuesta.error || 'No se pudo entrar a la mesa.');
      }
    } catch {
      this.error.set('No se pudo conectar con el servidor de juego.');
    } finally {
      this.cargando.set(false);
    }

    this.reloj = setInterval(() => this.actualizarReloj(), 500);
  }

  // Si sale de la pantalla sin "Salir", queda en la sala y puede volver a entrar
  ionViewWillLeave(): void {
    if (this.reloj) {
      clearInterval(this.reloj);
      this.reloj = null;
    }

    this.hub.desconectar();
  }


  // ================================
  // APUESTAS
  // ================================

  seleccionarFicha(valor: number): void {
    if (!this.enApuestas() || this.yo()?.yaAposto) {
      return;
    }

    const saldo = this.usuario()?.saldoFichas ?? 0;

    if (this.apuestaPendiente() + valor > saldo) {
      this.mostrarAviso('No tienes suficientes fichas.');
      return;
    }

    this.apuestaPendiente.update(total => total + valor);
    this.reproducirSonido('card-shove-1.ogg');
  }

  limpiarApuesta(): void {
    this.apuestaPendiente.set(0);
  }

  async confirmarApuesta(): Promise<void> {
    const monto = this.apuestaPendiente();

    if (monto <= 0) {
      this.mostrarAviso('Toca las fichas para armar tu apuesta.');
      return;
    }

    await this.ejecutar(() => this.hub.apostar(monto));

    if (this.yo()?.yaAposto) {
      this.apuestaPendiente.set(0);
    }
  }

  iniciarRonda(): Promise<void> {
    return this.ejecutar(() => this.hub.iniciarRonda());
  }


  // ================================
  // ACCIONES: PEDIR, PLANTARSE, DOBLAR, DIVIDIR
  // ================================

  jugar(accion: AccionBlackjack): Promise<void> {
    return this.ejecutar(() => this.hub.accion(accion));
  }


  // ================================
  // SALIR
  // ================================

  async salir(): Promise<void> {
    const enRonda = !this.enApuestas() && (this.yo()?.manos.length ?? 0) > 0;

    const confirmar = await this.alertController.create({
      header: 'Salir de la mesa',
      message: enRonda
        ? 'Estás en plena ronda: tu mano se plantará y perderás lo que no ganes. ¿Salir de todas formas?'
        : '¿Quieres salir de la sala? Si ya apostaste, se te devolverán las fichas.',
      buttons: [
        { text: 'Quedarme', role: 'cancel' },
        { text: 'Salir', role: 'confirm' }
      ]
    });

    await confirmar.present();
    const { role } = await confirmar.onDidDismiss();

    if (role !== 'confirm') {
      return;
    }

    await this.hub.salirMesa();
    this.router.navigateByUrl('/blackjack/salas', { replaceUrl: true });
  }

  // Vuelve a la pantalla de la sala sin abandonarla
  volverASala(): void {
    this.router.navigate(['/blackjack/sala', this.idSala()]);
  }

  cerrarResultado(): void {
    this.hub.ultimoResultado.set(null);
  }


  // ================================
  // APOYO
  // ================================

  imagenCarta(carta: CartaMesa): string {
    if (carta.oculta || !carta.valor) {
      return 'assets/cartas/card_back.png';
    }

    const valor = /^\d+$/.test(carta.valor) ? carta.valor.padStart(2, '0') : carta.valor;
    return `assets/cartas/card_${PALOS[carta.palo] ?? 'spades'}_${valor}.png`;
  }

  nombreCarta(carta: CartaMesa): string {
    return carta.oculta ? 'Carta oculta' : `${carta.valor} de ${carta.palo}`;
  }

  textoResultado(resultado: string): string {
    switch (resultado) {
      case 'Blackjack': return '¡Blackjack!';
      case 'Gana': return '¡Ganaste!';
      case 'Empate': return 'Empate';
      default: return 'Perdiste';
    }
  }

  private async ejecutar(accion: () => Promise<{ success: boolean; error: string }>): Promise<void> {
    if (this.enviando()) {
      return;
    }

    this.enviando.set(true);

    try {
      const respuesta = await accion();

      if (!respuesta.success) {
        this.mostrarAviso(respuesta.error || 'No se pudo realizar la acción.');
      }
    } finally {
      this.enviando.set(false);
    }
  }

  private mostrarAviso(texto: string): void {
    this.aviso.set(texto);
    setTimeout(() => {
      if (this.aviso() === texto) {
        this.aviso.set('');
      }
    }, 3500);
  }

  private actualizarReloj(): void {
    const fin = this.estado()?.finTurnoUtc;

    if (!fin) {
      this.segundosRestantes.set(null);
      return;
    }

    // El servidor manda la hora en UTC sin la "Z" final
    const finMs = Date.parse(fin.endsWith('Z') ? fin : `${fin}Z`);
    this.segundosRestantes.set(Math.max(0, Math.ceil((finMs - Date.now()) / 1000)));
  }

  private contarCartas(estado: EstadoMesa | null): number {
    if (!estado) {
      return 0;
    }

    return estado.cartasDealer.length +
      estado.jugadores.reduce((total, j) =>
        total + j.manos.reduce((suma, m) => suma + m.cartas.length, 0), 0);
  }

  private reproducirSonido(nombre: string): void {
    const audio = new Audio(`assets/sonidos/${nombre}`);
    audio.volume = 0.6;

    audio.play().catch(() => {
      // El navegador bloquea el audio hasta que el usuario toca la pantalla
    });
  }
}
