import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { IonContent, IonButton, IonInput, IonItem, IonLabel } from '@ionic/angular';

import { SalasService } from '../../../services/salas.service';

@Component({
  selector: 'app-crear-sala',
  templateUrl: './crear-sala.page.html',
  styleUrls: ['./crear-sala.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    IonContent,
    IonButton,
    IonInput,
    IonItem,
    IonLabel
  ]
})
export class CrearSalaPage {

  private salasService = inject(SalasService);
  private router = inject(Router);

  nombreSala = signal('');
  salaPrivada = signal(false);

  creando = signal(false);
  mensaje = signal('');

  // El máximo de jugadores y los límites de apuesta los define el administrador
  // en la tabla Configuracion, por eso aquí solo se informan.
  crearSala(): void {
    const nombre = this.nombreSala().trim();

    if (nombre.length < 3 || nombre.length > 50) {
      this.mensaje.set('El nombre de la sala debe tener entre 3 y 50 caracteres.');
      return;
    }

    this.creando.set(true);
    this.mensaje.set('');

    this.salasService.crear(nombre, this.salaPrivada() ? 'Privada' : 'Publica').subscribe({
      next: (respuesta) => {
        this.creando.set(false);

        if (!respuesta.success || !respuesta.data) {
          this.mensaje.set(respuesta.error || 'No se pudo crear la sala.');
          return;
        }

        this.nombreSala.set('');
        this.router.navigate(['/blackjack/sala', respuesta.data.idSala], { replaceUrl: true });
      },
      error: (error) => {
        this.creando.set(false);
        this.mensaje.set(error.error?.error || 'No se pudo crear la sala.');
      }
    });
  }
}
