import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { IonContent, IonButton } from '@ionic/angular';

interface JugadorSala {
  nombre: string;
  esCreador: boolean;
  conectado: boolean;
}

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

  nombreSala = 'Mesa Royal';

  codigoSala = 'ROYAL-7421';

  salaPrivada = false;

  jugadores: JugadorSala[] = [
    {
      nombre: 'Tú',
      esCreador: true,
      conectado: true
    }
  ];

  maxJugadores = 4;

  partidaIniciada = false;


  comenzarPartida(): void {

    if (this.jugadores.length < 1) {
      return;
    }

    this.partidaIniciada = true;

    console.log('La partida está lista para comenzar');
  }

}
