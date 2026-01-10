import { inject } from '@angular/core';
import { TrivyMessageService } from '../services/trivy-message.service';
import { RouterEventEmitterService } from '../services/router-event-emitter.service';

export abstract class DataPageBase {
  private readonly trivyMessageService = inject(TrivyMessageService);
  private readonly routerEventEmitterService = inject(RouterEventEmitterService);

  protected onError(err: any) {
    this.trivyMessageService.pushSimple(
      'Error on getting data.',
      this.routerEventEmitterService.title(),
      'error', err);
  }
}
