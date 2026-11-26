import { inject, Injectable, signal } from '@angular/core';

import { ApiConfiguration } from '@api/api-configuration';
import { AlertDto } from '@api/models/alert-dto';
import { RetryPolicy } from '@shared/utils/retry-policy.utils';
import { injectLogger } from '../logging/inject-logger';
import { ResilientHubConnection } from '../signalr/resilient-hub-connection';

@Injectable({
  providedIn: 'root',
})
export class AlertsService {
  private readonly _alerts = signal<AlertDto[]>([]);
  readonly alerts = this._alerts.asReadonly();

  private readonly _refreshCounter = signal(0); // simple counter trigger
  readonly refreshEvents = this._refreshCounter.asReadonly();

  private readonly hubPath = 'alerts-hub';
  private readonly hubConnection: ResilientHubConnection;

  constructor() {
    this.hubConnection = new ResilientHubConnection({
      url: `${inject(ApiConfiguration).rootUrl}${this.hubPath}`,
      log: injectLogger('Alerts'),
      retryPolicy: new RetryPolicy({ initialDelayMs: 1_000, maxDelayMs: 30_000 }),
      // the hub sends every current alert again on (re)connect (AlertsHub.OnConnectedAsync)
      onReconnecting: () => this._alerts.set([]),
    });
    this.addEventListeners();
    this.hubConnection.start();
  }

  getAlerts(): AlertDto[] {
    return this._alerts();
  }

  triggerRefresh(): void {
    this._refreshCounter.update((v) => v + 1);
  }

  private addEventListeners() {
    this.hubConnection.on('ReceiveAddedAlert', (alert: AlertDto) => {
      this.addAlert(alert);
    });

    this.hubConnection.on('ReceiveRemovedAlert', (alert: AlertDto) => {
      this.removeAlert(alert);
    });
  }

  private addAlert(alert: AlertDto) {
    this._alerts.update((list) => [...list, alert]);
  }

  private removeAlert(alert: AlertDto) {
    this._alerts.update((list) => list.filter((a) => a.emitter !== alert.emitter || a.emitterKey !== alert.emitterKey));
  }
}
