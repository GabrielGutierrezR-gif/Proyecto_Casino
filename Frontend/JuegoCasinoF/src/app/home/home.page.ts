import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { IonContent, IonButton } from '@ionic/angular';

import { AuthService, Usuario } from '../services/auth.service';

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

  usuario: Usuario | null = null;
  cargando = true;
  mensaje = '';

  ngOnInit(): void {
    // Mostrar primero el usuario guardado durante el login.
    const usuarioGuardado = sessionStorage.getItem('usuario');

    if (usuarioGuardado) {
      try {
        this.usuario = JSON.parse(usuarioGuardado) as Usuario;
        this.cargando = false;
      } catch (error) {
        console.error('No se pudo leer el usuario guardado:', error);
        sessionStorage.removeItem('usuario');
      }
    }

    // Actualizar los datos con la información real del servidor.
    this.cargarPerfil();
  }

  cargarPerfil(): void {
    // Solo mostrar la carga si todavía no tenemos datos.
    if (!this.usuario) {
      this.cargando = true;
    }

    this.mensaje = '';

    this.authService.obtenerPerfil().subscribe({
      next: (respuesta) => {
        if (!respuesta.success || !respuesta.data) {
          this.mensaje = respuesta.error || 'No se pudo actualizar el perfil.';
          this.cargando = false;
          return;
        }

        this.usuario = respuesta.data;

        // Mantener actualizados los datos para la próxima entrada.
        sessionStorage.setItem(
          'usuario',
          JSON.stringify(respuesta.data)
        );

        this.cargando = false;
      },

      error: (error) => {
        console.error('Error al obtener perfil:', error);

        // No borrar los datos que ya pudimos mostrar.
        if (!this.usuario) {
          this.mensaje =
            error.error?.error ||
            'No se pudo conectar con el servidor.';
        }

        this.cargando = false;
      }
    });
  }
}
