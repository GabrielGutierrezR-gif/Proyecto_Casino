import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { RouterLink } from '@angular/router';

import { IonContent, IonButton, IonInput } from '@ionic/angular';

import { AuthService } from '../../services/auth.service';
import {
  HistorialCompra,
  HistorialJuego,
  HistorialService
} from '../../services/historial.service';

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
    IonInput
  ]
})
export class ProfilePage implements OnInit {
  private authService = inject(AuthService);
  private historialService = inject(HistorialService);

  // El perfil lo guarda AuthService; los demás datos van en signals (la app no usa zone.js)
  usuario = this.authService.usuario;
  cargando = signal(true);
  guardando = signal(false);
  mensaje = signal('');
  mensajeExito = signal('');
  nuevoNombre = signal('');

  // Historial (REQ9)
  pestanaHistorial = signal<'juegos' | 'compras'>('juegos');
  juegos = signal<HistorialJuego[]>([]);
  compras = signal<HistorialCompra[]>([]);
  cargandoHistorial = signal(true);
  mensajeHistorial = signal('');

  ngOnInit(): void {
    this.cargarPerfil();
    this.cargarHistorial();
  }

  cargarHistorial(): void {
    this.cargandoHistorial.set(true);
    this.mensajeHistorial.set('');

    this.historialService.juegos(15).subscribe({
      next: (respuesta) => {
        this.juegos.set(respuesta.data ?? []);
        this.cargandoHistorial.set(false);
      },
      error: (error) => {
        this.mensajeHistorial.set(error.error?.error || 'No se pudo cargar el historial.');
        this.cargandoHistorial.set(false);
      }
    });

    this.historialService.compras(15).subscribe({
      next: (respuesta) => this.compras.set(respuesta.data ?? []),
      error: (error) => console.error('Error al cargar compras:', error)
    });
  }

  cerrarSesion(): void {
    this.authService.cerrarSesion();
  }

  cargarPerfil(): void {
    this.cargando.set(true);
    this.mensaje.set('');
    this.mensajeExito.set('');

    this.authService.obtenerPerfil().subscribe({
      next: (respuesta) => {
        if (!respuesta.success || !respuesta.data) {
          this.mensaje.set(respuesta.error || 'No se pudo cargar el perfil.');
          this.cargando.set(false);
          return;
        }

        this.nuevoNombre.set(respuesta.data.nombreUsuario);
        this.cargando.set(false);
      },
      error: (error) => {
        console.error('Error al cargar el perfil:', error);
        this.mensaje.set(error.error?.error || 'No se pudo cargar tu perfil. Intenta nuevamente.');
        this.cargando.set(false);
      }
    });
  }

  guardarNombre(): void {
    const nombre = this.nuevoNombre().trim();

    if (nombre.length < 3 || nombre.length > 50) {
      this.mensaje.set('El nombre debe tener entre 3 y 50 caracteres.');
      this.mensajeExito.set('');
      return;
    }

    if (this.guardando()) {
      return;
    }

    this.guardando.set(true);
    this.mensaje.set('');
    this.mensajeExito.set('');

    this.authService.actualizarNombreUsuario(nombre).subscribe({
      next: (respuesta) => {
        if (!respuesta.success || !respuesta.data) {
          this.mensaje.set(respuesta.error || 'No se pudo actualizar el nombre.');
          this.guardando.set(false);
          return;
        }

        this.nuevoNombre.set(respuesta.data.nombreUsuario);
        this.mensajeExito.set('¡Nombre de usuario actualizado!');
        this.guardando.set(false);
      },
      error: (error) => {
        console.error('Error al actualizar el nombre:', error);
        this.mensaje.set(error.error?.error || 'No se pudo actualizar el nombre. Intenta nuevamente.');
        this.guardando.set(false);
      }
    });
  }
}
