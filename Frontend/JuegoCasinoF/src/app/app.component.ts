import { Component, inject } from '@angular/core';
import { IonApp, IonRouterOutlet } from '@ionic/angular';
import { AudioService } from './services/audio.service';
import { ControlVolumenComponent } from './components/control-volumen/control-volumen.component';

@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  imports: [IonApp, IonRouterOutlet, ControlVolumenComponent],
})
export class AppComponent {
  private audio = inject(AudioService);
  constructor() {
    // La música arranca con el primer toque en cualquier parte de la app
    document.addEventListener('pointerdown', () => this.audio.iniciarMusica(), { once: true });
  
  }
}
