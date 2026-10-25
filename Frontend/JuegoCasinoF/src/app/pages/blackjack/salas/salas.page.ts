import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, of, catchError } from 'rxjs';

import { IonContent, IonButton, IonInput } from '@ionic/angular';

import { RespuestaAPI } from '../../../services/auth.service';
import { Sala, SalasService } from '../../../services/salas.service';

@Component({
  selector: 'app-salas',
  templateUrl: './salas.page.html',
  styleUrls: ['./salas.page.scss'],
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
export class SalasPage {

  private salasService = inject(SalasService);
  private router = inject(Router);

  // La app no usa zone.js: el estado va en signals para que la vista se actualice sola
  salas = signal<Sala[]>([]);
  miSala = signal<Sala | null>(null);

  cargando = signal(true);
  mensaje = signal('');

  mostrarCodigo = signal(false);
  codigo = signal('');
  uniendose = signal(false);

  // Ionic reutiliza la página: se refresca la lista cada vez que se entra
  ionViewWillEnter(): void {
    this.cargarSalas();
  }

  cargarSalas(): void {
    this.cargando.set(true);
    this.mensaje.set('');

    // "mia" responde 404 si no estoy en ninguna sala: no es un error
    const sinSala: RespuestaAPI<Sala> = { success: false, data: null, error: '' };

    forkJoin({
      publicas: this.salasService.listarPublicas(),
      mia: this.salasService.miSala().pipe(catchError(() => of(sinSala)))
    }).subscribe({
      next: ({ publicas, mia }) => {
        this.salas.set(publicas.data ?? []);
        this.miSala.set(mia.data);
        this.cargando.set(false);
      },
      error: (error) => {
        this.mensaje.set(error.error?.error || 'No se pudieron cargar las salas.');
        this.cargando.set(false);
      }
    });
  }

  unirseConCodigo(): void {
    const codigo = this.codigo().trim().toUpperCase();

    if (codigo.length !== 6) {
      this.mensaje.set('El código de sala tiene 6 caracteres.');
      return;
    }

    this.unirse(codigo);
  }

  unirse(codigo: string): void {
    this.uniendose.set(true);
    this.mensaje.set('');

    this.salasService.unirse(codigo).subscribe({
      next: (respuesta) => {
        this.uniendose.set(false);

        if (!respuesta.success || !respuesta.data) {
          this.mensaje.set(respuesta.error || 'No se pudo entrar a la sala.');
          return;
        }

        this.codigo.set('');
        this.router.navigate(['/blackjack/sala', respuesta.data.idSala]);
      },
      error: (error) => {
        this.uniendose.set(false);
        this.mensaje.set(error.error?.error || 'No se pudo entrar a la sala.');
      }
    });
  }

  volverAMiSala(): void {
    const sala = this.miSala();

    if (sala) {
      this.router.navigate(['/blackjack/sala', sala.idSala]);
    }
  }
}
