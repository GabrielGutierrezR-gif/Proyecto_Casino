import { Injectable, signal } from '@angular/core';
import { App } from '@capacitor/app';

// Controla la música de fondo y el volumen de los efectos de toda la app
@Injectable({ providedIn: 'root' })
export class AudioService {

  // Volúmenes de 0 a 1, guardados en el teléfono
  volumenMusica = signal(this.leer('volumenMusica', 0.4));
  volumenEfectos = signal(this.leer('volumenEfectos', 0.6));

  private musica = new Audio('assets/musica/fondo.mp3');

  constructor() {
    this.musica.loop = true;
    this.musica.volume = this.volumenMusica();
    // Pausa la música al salir de la app (o cambiar de pestaña) y la reanuda al volver
    App.addListener('appStateChange', ({ isActive }) => {
      if (isActive) {
        this.iniciarMusica();
      } else {
        this.musica.pause();
      }
    });
  }

  // El navegador no deja sonar audio hasta que el usuario toca la pantalla
  iniciarMusica(): void {
    if (this.musica.paused && this.volumenMusica() > 0) {
      this.musica.play().catch(() => { });
    }
  }

  cambiarVolumenMusica(valor: number): void {
    this.volumenMusica.set(valor);
    this.musica.volume = valor;
    this.guardar('volumenMusica', valor);

    if (valor === 0) {
      this.musica.pause();
    } else {
      this.iniciarMusica();
    }
  }

  cambiarVolumenEfectos(valor: number): void {
    this.volumenEfectos.set(valor);
    this.guardar('volumenEfectos', valor);
  }

  // Reproduce un efecto de assets/sonidos con el volumen elegido
  efecto(nombre: string): void {
    if (this.volumenEfectos() === 0) {
      return;
    }

    const audio = new Audio(`assets/sonidos/${nombre}`);
    audio.volume = this.volumenEfectos();
    audio.play().catch(() => { });
  }

  private leer(clave: string, porDefecto: number): number {
    try {
      const valor = localStorage.getItem(clave);
      return valor === null ? porDefecto : Number(valor);
    } catch {
      return porDefecto;
    }
  }

  private guardar(clave: string, valor: number): void {
    try {
      localStorage.setItem(clave, String(valor));
    } catch { }
  }
}
