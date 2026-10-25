import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { IonContent, IonButton } from '@ionic/angular';

import { AuthService } from '../../services/auth.service';
import {
  ApuestaRuleta,
  GiroResultado,
  RuletaService,
  TipoApuestaRuleta
} from '../../services/ruleta.service';

interface Ficha {
  valor: number;
  imagen: string;
}

interface Segmento {
  numero: number;
  color: string;
  ruta: string;
  textoX: number;
  textoY: number;
  rotacion: number;
}

// Orden real de los números en una ruleta europea (en sentido horario desde el 0)
const ORDEN_RUEDA = [
  0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10,
  5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
];

const ROJOS = new Set([1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36]);

const GRADOS_SEGMENTO = 360 / ORDEN_RUEDA.length;
const DURACION_GIRO_MS = 5000;

@Component({
  selector: 'app-ruleta',
  templateUrl: './ruleta.page.html',
  styleUrls: ['./ruleta.page.scss'],
  standalone: true,
  imports: [CommonModule, RouterLink, IonContent, IonButton]
})
export class RuletaPage {

  private ruletaService = inject(RuletaService);
  private authService = inject(AuthService);

  usuario = this.authService.usuario;

  fichas: Ficha[] = [
    { valor: 5, imagen: 'assets/fichas/fichaBlanca.png' },
    { valor: 10, imagen: 'assets/fichas/fichaAzul.png' },
    { valor: 25, imagen: 'assets/fichas/fichaRoja.png' },
    { valor: 50, imagen: 'assets/fichas/fichaVerde.jpg' },
    { valor: 100, imagen: 'assets/fichas/fichaDorada.png' }
  ];

  fichaSeleccionada = signal(10);

  // Apuestas sobre el tapete: "Numero:17" → 30 fichas
  apuestas = signal<Map<string, number>>(new Map());

  totalApostado = computed(() =>
    [...this.apuestas().values()].reduce((total, monto) => total + monto, 0));

  girando = signal(false);
  rotacion = signal(0);
  resultado = signal<GiroResultado | null>(null);
  historial = signal<{ numero: number; color: string }[]>([]);
  mensaje = signal('');

  // Filas del tapete: 3, 6, 9... arriba, igual que en el casino
  filasTapete = [
    Array.from({ length: 12 }, (_, i) => (i + 1) * 3),
    Array.from({ length: 12 }, (_, i) => (i + 1) * 3 - 1),
    Array.from({ length: 12 }, (_, i) => (i + 1) * 3 - 2)
  ];

  segmentos: Segmento[] = ORDEN_RUEDA.map((numero, i) => {
    const inicio = (i * GRADOS_SEGMENTO - 90) * Math.PI / 180;
    const fin = ((i + 1) * GRADOS_SEGMENTO - 90) * Math.PI / 180;
    const medio = (inicio + fin) / 2;

    const punto = (angulo: number, radio: number) =>
      `${100 + radio * Math.cos(angulo)} ${100 + radio * Math.sin(angulo)}`;

    return {
      numero,
      color: this.colorDe(numero),
      ruta: `M ${punto(inicio, 60)} L ${punto(inicio, 98)} A 98 98 0 0 1 ${punto(fin, 98)} L ${punto(fin, 60)} A 60 60 0 0 0 ${punto(inicio, 60)} Z`,
      textoX: 100 + 84 * Math.cos(medio),
      textoY: 100 + 84 * Math.sin(medio),
      rotacion: (i + 0.5) * GRADOS_SEGMENTO
    };
  });

  private audio: AudioContext | null = null;


  // ================================
  // TAPETE
  // ================================

  colorDe(numero: number): string {
    if (numero === 0) {
      return 'Verde';
    }

    return ROJOS.has(numero) ? 'Rojo' : 'Negro';
  }

  montoEn(tipo: TipoApuestaRuleta, valor: string | number): number {
    return this.apuestas().get(`${tipo}:${valor}`) ?? 0;
  }

  apostar(tipo: TipoApuestaRuleta, valor: string | number): void {
    if (this.girando()) {
      return;
    }

    const ficha = this.fichaSeleccionada();
    const saldo = this.usuario()?.saldoFichas ?? 0;

    if (this.totalApostado() + ficha > saldo) {
      this.mensaje.set('No tienes suficientes fichas para esa apuesta.');
      return;
    }

    const clave = `${tipo}:${valor}`;
    const nuevas = new Map(this.apuestas());
    nuevas.set(clave, (nuevas.get(clave) ?? 0) + ficha);

    this.apuestas.set(nuevas);
    this.mensaje.set('');
    this.resultado.set(null);
    this.sonidoFicha();
  }

  // Clic derecho / mantener: quitar la apuesta de esa casilla
  quitar(evento: Event, tipo: TipoApuestaRuleta, valor: string | number): void {
    evento.preventDefault();

    if (this.girando()) {
      return;
    }

    const nuevas = new Map(this.apuestas());
    nuevas.delete(`${tipo}:${valor}`);
    this.apuestas.set(nuevas);
  }

  limpiar(): void {
    if (!this.girando()) {
      this.apuestas.set(new Map());
    }
  }


  // ================================
  // GIRAR
  // ================================

  girar(): void {
    if (this.girando()) {
      return;
    }

    const apuestas: ApuestaRuleta[] = [...this.apuestas().entries()].map(([clave, monto]) => {
      const [tipoApuesta, valorApuesta] = clave.split(':');
      return { tipoApuesta: tipoApuesta as TipoApuestaRuleta, valorApuesta, monto };
    });

    if (apuestas.length === 0) {
      this.mensaje.set('Coloca al menos una ficha en el tapete.');
      return;
    }

    this.girando.set(true);
    this.mensaje.set('');
    this.resultado.set(null);

    // El número ganador lo decide el servidor; la rueda solo lo anima
    this.ruletaService.girar(apuestas).subscribe({
      next: (respuesta) => {
        if (!respuesta.success || !respuesta.data) {
          this.girando.set(false);
          this.mensaje.set(respuesta.error || 'No se pudo girar la ruleta.');
          return;
        }

        this.animarGiro(respuesta.data);
      },
      error: (error) => {
        this.girando.set(false);
        this.mensaje.set(error.error?.error || 'No se pudo conectar con el servidor.');
      }
    });
  }

  private animarGiro(giro: GiroResultado): void {
    const indice = ORDEN_RUEDA.indexOf(giro.numeroGanador);

    // Ángulo para que el número quede debajo del indicador (arriba), más 5 vueltas
    const destino = 360 - (indice + 0.5) * GRADOS_SEGMENTO;
    const actual = this.rotacion();
    const vueltaActual = Math.ceil(actual / 360) * 360;

    this.rotacion.set(vueltaActual + 360 * 5 + destino);
    this.sonidoGiro();

    setTimeout(() => {
      this.sonidoBola();

      this.resultado.set(giro);
      this.girando.set(false);
      this.apuestas.set(new Map());
      this.authService.actualizarSaldo(giro.nuevoSaldo);

      this.historial.update(lista =>
        [{ numero: giro.numeroGanador, color: giro.colorGanador }, ...lista].slice(0, 12));
    }, DURACION_GIRO_MS);
  }

  textoApuesta(tipo: string, valor: string): string {
    switch (tipo) {
      case 'Numero': return `Número ${valor}`;
      case 'Docena': return `${valor}ª docena`;
      default: return valor;
    }
  }


  // ================================
  // SONIDOS (generados con Web Audio, no necesitan archivos)
  // ================================

  private contexto(): AudioContext | null {
    try {
      this.audio ??= new AudioContext();
      return this.audio;
    } catch {
      return null;
    }
  }

  private tono(frecuencia: number, inicio: number, duracion: number, volumen: number, tipo: OscillatorType = 'square'): void {
    const ctx = this.contexto();

    if (!ctx) {
      return;
    }

    const oscilador = ctx.createOscillator();
    const ganancia = ctx.createGain();

    oscilador.type = tipo;
    oscilador.frequency.value = frecuencia;

    ganancia.gain.setValueAtTime(volumen, ctx.currentTime + inicio);
    ganancia.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + inicio + duracion);

    oscilador.connect(ganancia).connect(ctx.destination);
    oscilador.start(ctx.currentTime + inicio);
    oscilador.stop(ctx.currentTime + inicio + duracion);
  }

  private sonidoFicha(): void {
    this.tono(1800, 0, 0.05, 0.05, 'triangle');
  }

  // "Tic, tic, tic" que se va frenando como la bola sobre los separadores
  private sonidoGiro(): void {
    let tiempo = 0;
    let intervalo = 0.04;

    while (tiempo < DURACION_GIRO_MS / 1000 - 0.3) {
      this.tono(2400, tiempo, 0.02, 0.025);
      tiempo += intervalo;
      intervalo *= 1.045;
    }
  }

  private sonidoBola(): void {
    this.tono(900, 0, 0.12, 0.08, 'triangle');
    this.tono(600, 0.1, 0.15, 0.06, 'triangle');
  }
}
