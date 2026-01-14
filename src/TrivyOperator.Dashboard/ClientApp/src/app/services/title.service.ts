import { effect, inject, Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterEventEmitterService } from './router-event-emitter.service';

@Injectable({
  providedIn: 'root',
})
export class TitleService {
  private defaultTitle: string = 'Trivy Operator Dashboard';

  private readonly titleService = inject(Title);
  private readonly routerEventEmitterService = inject(RouterEventEmitterService);

  constructor() {
    effect(() => {
      const title = this.routerEventEmitterService.title();
      this.updateTitle(title);
    });
  }

  private updateTitle(routeTitle: string) {
    const fullTitle = [routeTitle.replace('Reports', 'Reps'), this.defaultTitle].filter(Boolean).join(' - ');
    this.titleService.setTitle(fullTitle);
  }
}
