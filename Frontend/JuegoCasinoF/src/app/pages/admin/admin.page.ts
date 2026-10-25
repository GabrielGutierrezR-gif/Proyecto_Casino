import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { IonContent, IonButton, IonInput } from '@ionic/angular';

import { AdminService, Configuracion } from '../../services/admin.service';

interface FilaConfiguracion extends Configuracion {
  nuevoValor: string;
  guardando: boolean;
  mensaje: string;
  exito: boolean;
}

// Nombres legibles de cada parámetro de la tabla Configuracion
const ETIQUETAS: Record<string, string> = {
  FichasBienvenida: 'Fichas de bienvenida',
  MaxSalasActivas: 'Máximo de salas activas',
  MaxJugadoresPorSala: 'Jugadores por sala',
  ApuestaMinimaBlackjack: 'Apuesta mínima (Blackjack)',
  ApuestaMaximaBlackjack: 'Apuesta máxima (Blackjack)',
  SegundosPorTurno: 'Segundos por turno'
};

// Panel de administración (REQ8): cambia los parámetros sin tocar el código
@Component({
  selector: 'app-admin',
  templateUrl: './admin.page.html',
  styleUrls: ['./admin.page.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, IonContent, IonButton, IonInput]
})
export class AdminPage implements OnInit {

  private adminService = inject(AdminService);

  filas = signal<FilaConfiguracion[]>([]);
  cargando = signal(true);
  mensaje = signal('');

  ngOnInit(): void {
    this.cargar();
  }

  etiqueta(clave: string): string {
    return ETIQUETAS[clave] ?? clave;
  }

  cargar(): void {
    this.cargando.set(true);
    this.mensaje.set('');

    this.adminService.listarConfiguracion().subscribe({
      next: (respuesta) => {
        this.cargando.set(false);

        if (!respuesta.success || !respuesta.data) {
          this.mensaje.set(respuesta.error || 'No se pudo cargar la configuración.');
          return;
        }

        this.filas.set(respuesta.data.map(config => ({ ...config, nuevoValor: config.valor, guardando: false, mensaje: '', exito: false })));
      },
      error: (error) => {
        this.cargando.set(false);
        this.mensaje.set(error.status === 403 ? 'Tu cuenta no tiene permisos de administrador.' : error.error?.error || 'No se pudo cargar la configuración.');
      }
    });
  }

  guardar(fila: FilaConfiguracion): void {
    const valor = String(fila.nuevoValor ?? '').trim();

    if (valor === fila.valor) {
      return;
    }

    this.actualizarFila(fila.clave, { guardando: true, mensaje: '' });

    this.adminService.modificarConfiguracion(fila.clave, valor).subscribe({
      next: (respuesta) => {
        if (!respuesta.success || !respuesta.data) {
          this.actualizarFila(fila.clave, {
            guardando: false,
            exito: false,
            mensaje: respuesta.error || 'No se pudo guardar.'
          });
          return;
        }

        this.actualizarFila(fila.clave, {
          valor: respuesta.data.valor,
          nuevoValor: respuesta.data.valor,
          guardando: false,
          exito: true,
          mensaje: 'Guardado'
        });
      },
      error: (error) => {
        this.actualizarFila(fila.clave, {
          guardando: false,
          exito: false,
          mensaje: error.error?.error || 'No se pudo guardar.'
        });
      }
    });
  }

  // Reemplaza la fila para que el signal avise a la vista
  private actualizarFila(clave: string, cambios: Partial<FilaConfiguracion>): void {
    this.filas.update(filas =>
      filas.map(f => f.clave === clave ? { ...f, ...cambios } : f));
  }
}
