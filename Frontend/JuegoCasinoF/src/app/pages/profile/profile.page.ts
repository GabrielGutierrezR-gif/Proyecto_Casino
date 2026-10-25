import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { IonContent } from '@ionic/angular/ion-content';
import { IonButton } from '@ionic/angular/ion-button';
import { IonInput } from '@ionic/angular/ion-input';
import { RouterLink } from '@angular/router';

import { AuthService, Usuario } from '../../services/auth.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.page.html',
  styleUrls: ['./profile.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    IonContent,
    IonButton,
    IonInput,

  ]
})
export class ProfilePage implements OnInit {
  private authService = inject(AuthService);

  usuario: Usuario | null = null;
  cargando = true;
  guardando = false;
  mensaje = '';
  mensajeExito = '';
  nuevoNombre = '';

  ngOnInit(): void {
    this.cargarPerfil();
  }

  cargarPerfil(): void {
    this.cargando = true;
    this.mensaje = '';
    this.mensajeExito = '';

    this.authService.obtenerPerfil().subscribe({
      next: (respuesta) => {
        if (!respuesta.success || !respuesta.data) {
          this.mensaje =
            respuesta.error || 'No se pudo cargar el perfil.';
          this.cargando = false;
          return;
        }

        this.usuario = respuesta.data;
        this.nuevoNombre = respuesta.data.nombreUsuario;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar el perfil:', error);
        this.mensaje =
          error.error?.error ||
          'No se pudo cargar tu perfil. Intenta nuevamente.';
        this.cargando = false;
      }
    });
  }

  guardarNombre(): void {
    const nombre = this.nuevoNombre.trim();

    if (nombre.length < 3 || nombre.length > 50) {
      this.mensaje =
        'El nombre debe tener entre 3 y 50 caracteres.';
      this.mensajeExito = '';
      return;
    }

    if (this.guardando) {
      return;
    }

    this.guardando = true;
    this.mensaje = '';
    this.mensajeExito = '';

    this.authService.actualizarNombreUsuario(nombre).subscribe({
      next: (respuesta) => {
        if (!respuesta.success || !respuesta.data) {
          this.mensaje =
            respuesta.error || 'No se pudo actualizar el nombre.';
          this.guardando = false;
          return;
        }

        this.usuario = respuesta.data;
        this.nuevoNombre = respuesta.data.nombreUsuario;
        this.mensajeExito = '¡Nombre de usuario actualizado!';
        this.guardando = false;
      },
      error: (error) => {
        console.error('Error al actualizar el nombre:', error);
        this.mensaje =
          error.error?.error ||
          'No se pudo actualizar el nombre. Intenta nuevamente.';
        this.guardando = false;
      }
    });
  }
}
