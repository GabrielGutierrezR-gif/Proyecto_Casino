import { Component, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

import { IonContent, IonButton, AlertController } from '@ionic/angular';

import { AuthService } from '../../../services/auth.service';
import { Sala, SalasService } from '../../../services/salas.service';

const SEGUNDOS_REFRESCO = 5;

@Component({
  selector: 'app-sala',
  templateUrl: './sala.page.html',
  styleUrls: ['./sala.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonContent,
    IonButton
  ]
})
export class SalaPage {

  private salasService = inject(SalasService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private alertController = inject(AlertController);

  // Viene de la ruta /blackjack/sala/:idSala
  idSala = input.required<string>();

  usuario = this.authService.usuario;

  sala = signal<Sala | null>(null);
  cargando = signal(true);
  saliendo = signal(false);
  mensaje = signal('');
  copiado = signal(false);

  private refresco: ReturnType<typeof setInterval> | null = null;

  // La lista de jugadores se actualiza sola mientras se espera en el lobby
  ionViewWillEnter(): void {
    this.cargarSala();
    this.refresco = setInterval(() => this.cargarSala(false), SEGUNDOS_REFRESCO * 1000);
  }

  ionViewWillLeave(): void {
    if (this.refresco) {
      clearInterval(this.refresco);
      this.refresco = null;
    }
  }

  get espaciosLibres(): number[] {
    const sala = this.sala();

    if (!sala) {
      return [];
    }

    return Array.from({ length: Math.max(0, sala.maxJugadores - sala.cantidadJugadores) });
  }

  cargarSala(mostrarCarga = true): void {
    if (mostrarCarga) {
      this.cargando.set(true);
    }

    this.salasService.obtener(Number(this.idSala())).subscribe({
      next: (respuesta) => {
        this.cargando.set(false);

        if (!respuesta.success || !respuesta.data) {
          this.mensaje.set(respuesta.error || 'La sala ya no existe.');
          this.sala.set(null);
          return;
        }

        this.mensaje.set('');
        this.sala.set(respuesta.data);
      },
      error: (error) => {
        this.cargando.set(false);
        this.sala.set(null);
        this.mensaje.set(error.error?.error || 'La sala no existe o ya fue cerrada.');
      }
    });
  }

  async copiarCodigo(): Promise<void> {
    const sala = this.sala();

    if (!sala) {
      return;
    }

    try {
      await navigator.clipboard.writeText(sala.codigoSala);
      this.copiado.set(true);
      setTimeout(() => this.copiado.set(false), 2000);
    } catch {
      // El navegador no permitió copiar: el código igual está visible
    }
  }

  entrarMesa(): void {
    this.router.navigate(['/blackjack/mesa', this.idSala()]);
  }

  async salirSala(): Promise<void> {
    const confirmar = await this.alertController.create({
      header: 'Salir de la sala',
      message: '¿Seguro que quieres salir? Si eres el último jugador, la sala se cerrará.',
      buttons: [
        { text: 'Cancelar', role: 'cancel' },
        { text: 'Salir', role: 'confirm' }
      ]
    });

    await confirmar.present();
    const { role } = await confirmar.onDidDismiss();

    if (role !== 'confirm') {
      return;
    }

    this.saliendo.set(true);

    this.salasService.salir(Number(this.idSala())).subscribe({
      next: () => {
        this.saliendo.set(false);
        this.router.navigateByUrl('/blackjack/salas', { replaceUrl: true });
      },
      error: (error) => {
        this.saliendo.set(false);
        this.mensaje.set(error.error?.error || 'No se pudo salir de la sala.');
      }
    });
  }
}
