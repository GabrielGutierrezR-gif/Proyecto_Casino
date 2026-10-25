import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { IonContent, IonButton, IonInput, AlertController } from '@ionic/angular';

import { AuthService } from '../../services/auth.service';
import {
  ComprasService,
  MetodoPago,
  PaqueteFichas
} from '../../services/compras.service';

@Component({
  selector: 'app-comprar-fichas',
  templateUrl: './comprar-fichas.page.html',
  styleUrls: ['./comprar-fichas.page.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, IonContent, IonButton, IonInput]
})
export class ComprarFichasPage implements OnInit {

  private comprasService = inject(ComprasService);
  private authService = inject(AuthService);
  private alertController = inject(AlertController);

  usuario = this.authService.usuario;

  // La app no usa zone.js: el estado va en signals para que la vista se actualice sola
  paquetes = signal<PaqueteFichas[]>([]);
  metodos = signal<MetodoPago[]>([]);

  paqueteSeleccionado = signal<number | null>(null);
  metodoSeleccionado = signal<number | null>(null);

  paqueteActual = computed(() =>
    this.paquetes().find(p => p.idPaquete === this.paqueteSeleccionado()));

  cargando = signal(true);
  comprando = signal(false);
  mensaje = signal('');

  // Formulario de tarjeta ficticia
  mostrarFormulario = signal(false);
  guardandoTarjeta = signal(false);
  tipoTarjeta = signal<'Credito' | 'Debito'>('Credito');
  numeroTarjeta = signal('');
  nombreTitular = signal('');
  mensajeTarjeta = signal('');

  ngOnInit(): void {
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.cargando.set(true);
    this.mensaje.set('');

    forkJoin({
      paquetes: this.comprasService.listarPaquetes(),
      metodos: this.comprasService.listarMetodos()
    }).subscribe({
      next: ({ paquetes, metodos }) => {
        this.paquetes.set(paquetes.data ?? []);
        this.metodos.set(metodos.data ?? []);

        // Si solo tiene una tarjeta, se selecciona sola
        if (this.metodos().length === 1) {
          this.metodoSeleccionado.set(this.metodos()[0].idMetodoPago);
        }

        this.mostrarFormulario.set(this.metodos().length === 0);
        this.cargando.set(false);
      },
      error: (error) => {
        this.mensaje.set(error.error?.error || 'No se pudieron cargar los paquetes.');
        this.cargando.set(false);
      }
    });
  }

  // Muestra el número con espacios cada 4 dígitos mientras se escribe
  formatearNumero(valor: string | null | undefined): void {
    const digitos = (valor ?? '').replace(/\D/g, '').slice(0, 19);
    this.numeroTarjeta.set(digitos.replace(/(\d{4})(?=\d)/g, '$1 '));
  }

  guardarTarjeta(): void {
    const numero = this.numeroTarjeta().replace(/\D/g, '');
    const titular = this.nombreTitular().trim();

    if (numero.length < 13 || numero.length > 19) {
      this.mensajeTarjeta.set('El número de tarjeta debe tener entre 13 y 19 dígitos.');
      return;
    }

    if (!titular) {
      this.mensajeTarjeta.set('Escribe el nombre del titular.');
      return;
    }

    this.guardandoTarjeta.set(true);
    this.mensajeTarjeta.set('');

    this.comprasService.agregarMetodo({
      tipoTarjeta: this.tipoTarjeta(),
      numeroTarjeta: numero,
      nombreTitular: titular
    }).subscribe({
      next: (respuesta) => {
        this.guardandoTarjeta.set(false);

        if (!respuesta.success || !respuesta.data) {
          this.mensajeTarjeta.set(respuesta.error || 'No se pudo guardar la tarjeta.');
          return;
        }

        this.metodos.update(lista => [...lista, respuesta.data!]);
        this.metodoSeleccionado.set(respuesta.data.idMetodoPago);

        // Por seguridad se borra el número completo del formulario
        this.numeroTarjeta.set('');
        this.nombreTitular.set('');
        this.mostrarFormulario.set(false);
      },
      error: (error) => {
        this.guardandoTarjeta.set(false);
        this.mensajeTarjeta.set(error.error?.error || 'No se pudo guardar la tarjeta.');
      }
    });
  }

  async comprar(): Promise<void> {
    const paquete = this.paqueteActual();

    if (!paquete) {
      this.mensaje.set('Selecciona un paquete de fichas.');
      return;
    }

    const idMetodoPago = this.metodoSeleccionado();

    if (idMetodoPago === null) {
      this.mensaje.set('Selecciona o agrega un método de pago.');
      return;
    }

    const confirmar = await this.alertController.create({
      header: 'Confirmar compra',
      message: `${paquete.nombre}: ${paquete.cantidadFichas} fichas por $${paquete.precio.toFixed(2)}.`,
      buttons: [
        { text: 'Cancelar', role: 'cancel' },
        { text: 'Comprar', role: 'confirm' }
      ]
    });

    await confirmar.present();
    const { role } = await confirmar.onDidDismiss();

    if (role !== 'confirm') {
      return;
    }

    this.comprando.set(true);
    this.mensaje.set('');

    this.comprasService.comprar(paquete.idPaquete, idMetodoPago).subscribe({
      next: async (respuesta) => {
        this.comprando.set(false);

        if (!respuesta.success || !respuesta.data) {
          this.mensaje.set(respuesta.error || 'No se pudo completar la compra.');
          return;
        }

        this.authService.actualizarSaldo(respuesta.data.nuevoSaldo);
        this.paqueteSeleccionado.set(null);

        const alerta = await this.alertController.create({
          header: '¡Compra exitosa!',
          message: `Se agregaron ${respuesta.data.fichasAgregadas} fichas. Nuevo saldo: ${respuesta.data.nuevoSaldo}.`,
          buttons: ['Aceptar']
        });

        await alerta.present();
      },
      error: (error) => {
        this.comprando.set(false);
        this.mensaje.set(error.error?.error || 'No se pudo completar la compra.');
      }
    });
  }
}
