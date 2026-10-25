import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { IonContent, IonButton } from '@ionic/angular';

@Component({
  selector: 'app-comprar-fichas',
  templateUrl: './comprar-fichas.page.html',
  styleUrls: ['./comprar-fichas.page.scss'],
  standalone: true,
  imports: [CommonModule, RouterLink, IonContent, IonButton]
})
export class ComprarFichasPage {}
