
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { IonContent, IonButton } from '@ionic/angular';

interface Carta {
  imagen: string;
  nombre: string;
}

interface Ficha {
  valor: number;
  imagen: string;
}

@Component({
  selector: 'app-mesa',
  templateUrl: './mesa.page.html',
  styleUrls: ['./mesa.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonContent,
    IonButton
  ]
})
export class MesaPage {

  nombreJugador = 'Tú';

  saldo = 500;

  apuesta = 50;

  estadoJuego = 'Tu turno';

  mensajeJuego = 'Elige una acción';

  partidaTerminada = false;


  // ================================
  // FICHAS
  // ================================

  fichas: Ficha[] = [
    {
      valor: 5,
      imagen: 'assets/fichas/ficha_blanca.png'
    },
    {
      valor: 10,
      imagen: 'assets/fichas/ficha_azul.png'
    },
    {
      valor: 25,
      imagen: 'assets/fichas/ficha_roja.png'
    },
    {
      valor: 50,
      imagen: 'assets/fichas/ficha_verde.png'
    },
    {
      valor: 100,
      imagen: 'assets/fichas/ficha_dorada.png'
    }
  ];


  // ================================
  // CARTAS DEALER
  // ================================

  cartasDealer: Carta[] = [
    {
      imagen: 'assets/cartas/card_back.png',
      nombre: 'Carta oculta'
    },
    {
      imagen: 'assets/cartas/card_hearts_10.png',
      nombre: '10 de corazones'
    }
  ];


  // ================================
  // CARTAS JUGADOR
  // ================================

  cartasJugador: Carta[] = [
    {
      imagen: 'assets/cartas/card_spades_A.png',
      nombre: 'As de espadas'
    },
    {
      imagen: 'assets/cartas/card_hearts_7.png',
      nombre: '7 de corazones'
    }
  ];


  valorJugador = 18;

  valorDealer: string | number = '?';


  // ================================
  // SONIDOS
  // ================================

  reproducirSonido(nombre: string): void {

    const audio = new Audio(
      `assets/sonidos/${nombre}`
    );

    audio.volume = 0.6;

    audio.play().catch(() => {

      console.log(
        'No se pudo reproducir el sonido:',
        nombre
      );

    });
  }


  // ================================
  // SELECCIONAR FICHA
  // ================================

  seleccionarFicha(valor: number): void {

    if (this.partidaTerminada) {
      return;
    }

    if (this.saldo < valor) {

      this.mensajeJuego =
        'No tienes suficientes fichas.';

      return;
    }

    this.apuesta += valor;

    this.saldo -= valor;

    this.reproducirSonido('chip-lay.mp3');

    this.mensajeJuego =
      `Agregaste una ficha de ${valor}.`;
  }


  // ================================
  // PEDIR CARTA
  // ================================

  pedirCarta(): void {

    if (this.partidaTerminada) {
      return;
    }

    this.reproducirSonido('card-place.mp3');

    this.mensajeJuego =
      'Has pedido una carta';

    this.cartasJugador.push({
      imagen: 'assets/cartas/card_clubs_2.png',
      nombre: '2 de tréboles'
    });

    this.valorJugador += 2;

    if (this.valorJugador > 21) {

      this.estadoJuego = 'Te pasaste';

      this.mensajeJuego =
        '¡Has perdido! Superaste 21.';

      this.partidaTerminada = true;
    }
  }


  // ================================
  // PLANTARSE
  // ================================

  plantarse(): void {

    if (this.partidaTerminada) {
      return;
    }

    this.reproducirSonido('card-slide.mp3');

    this.estadoJuego =
      'Turno del dealer';

    this.mensajeJuego =
      'El dealer está jugando...';

    setTimeout(() => {

      this.valorDealer = 17;

      this.estadoJuego =
        'Resultado';

      this.mensajeJuego =
        '¡Ganaste!';

      this.partidaTerminada = true;

    }, 1000);
  }


  // ================================
  // DOBLAR
  // ================================

  doblar(): void {

    if (this.partidaTerminada) {
      return;
    }

    if (this.saldo < this.apuesta) {

      this.mensajeJuego =
        'No tienes suficientes fichas para doblar.';

      return;
    }

    this.saldo -= this.apuesta;

    this.apuesta *= 2;

    this.reproducirSonido('chip-lay.mp3');

    this.mensajeJuego =
      'Apuesta doblada. Recibirás una última carta.';

    this.cartasJugador.push({
      imagen: 'assets/cartas/card_diamonds_3.png',
      nombre: '3 de diamantes'
    });

    this.valorJugador += 3;

    this.estadoJuego =
      'Última carta';
  }


  // ================================
  // DIVIDIR
  // ================================

  dividir(): void {

    this.mensajeJuego =
      'Dividir estará disponible cuando tengas una pareja.';
  }


  // ================================
  // SALIR
  // ================================

  salir(): void {

    console.log(
      'El jugador salió de la mesa.'
    );
  }

}
