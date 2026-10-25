import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { IonContent, IonButton } from '@ionic/angular';

import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonContent,
    IonButton,
    RouterLink
  ]
})
export class HomePage implements OnInit {

  private authService = inject(AuthService);

  // Se muestra de inmediato el usuario guardado durante el login
  // y se actualiza solo cuando llega el perfil del servidor.
  usuario = this.authService.usuario;
  esAdmin = this.authService.esAdmin;
  mensaje = '';

  ngOnInit(): void {
    this.cargarPerfil();
  }

  // Ionic reutiliza la página: al volver de jugar o comprar se refresca el saldo
  ionViewWillEnter(): void {
    this.cargarPerfil();
  }

  cargarPerfil(): void {
    this.mensaje = '';

    this.authService.obtenerPerfil().subscribe({
      next: (respuesta) => {
        if (!respuesta.success || !respuesta.data) {
          this.mensaje = respuesta.error || 'No se pudo actualizar el perfil.';
        }
      },

      error: (error) => {
        console.error('Error al obtener perfil:', error);

        // No borrar los datos que ya pudimos mostrar.
        if (!this.usuario()) {
          this.mensaje =
            error.error?.error ||
            'No se pudo conectar con el servidor.';
        }
      }
    });
  }

  cerrarSesion(): void {
    this.authService.cerrarSesion();
  }
}
