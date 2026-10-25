import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { IonContent, IonButton, IonInput, IonItem, IonLabel } from '@ionic/angular';

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

  nombreSala = '';
  salaPrivada = false;
  maxJugadores = 4;
  apuestaMinima = 50;
  apuestaMaxima = 500;

  crearSala(): void {
    console.log('Configuración de sala:', {
      nombreSala: this.nombreSala,
      salaPrivada: this.salaPrivada,
      maxJugadores: this.maxJugadores,
      apuestaMinima: this.apuestaMinima,
      apuestaMaxima: this.apuestaMaxima
    });
  }
}
