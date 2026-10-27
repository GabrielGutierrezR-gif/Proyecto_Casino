import { Component, inject, signal } from '@angular/core';
import { AudioService } from '../../services/audio.service';

// Botón flotante con dos barras: volumen de música y de efectos
@Component({
  selector: 'app-control-volumen',
  templateUrl: './control-volumen.component.html',
  styleUrls: ['./control-volumen.component.scss'],
  standalone: true
})
export class ControlVolumenComponent {
  audio = inject(AudioService);
  abierto = signal(false);

  valor(evento: Event): number {
    return Number((evento.target as HTMLInputElement).value);
  }

  porcentaje(valor: number): number {
    return Math.round(valor * 100);
  }
}
