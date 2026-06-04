import { inject } from '@angular/core';
import { RouterEventEmitterService } from '../services/router-event-emitter.service';
import { TrivyMessageService } from '../services/trivy-message.service';

export abstract class DataPageBase {
  private readonly trivyMessageService = inject(TrivyMessageService);
  private readonly routerEventEmitterService = inject(RouterEventEmitterService);

  protected onError(err: any) {
    this.trivyMessageService.pushSimple('Error on getting data.', this.routerEventEmitterService.title(), 'error', err);
  }

  protected showErrorToast(message: string, title: string) {
    this.trivyMessageService.pushSimple(message, title, 'error');
  }
}
