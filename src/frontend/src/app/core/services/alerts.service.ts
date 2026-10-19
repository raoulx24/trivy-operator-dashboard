import { inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

import { ApiConfiguration } from '@api/api-configuration';
import { AlertDto } from '@api/models/alert-dto';
import { RetryPolicy } from '@shared/utils/retry-policy.utils';
import { injectLogger } from '../logging/inject-logger';

@Injectable({
  providedIn: 'root',
})
export class AlertsService {
  private hubConnection!: HubConnection;

  private readonly _alerts = signal<AlertDto[]>([]);
  readonly alerts = this._alerts.asReadonly();

  private readonly _refreshCounter = signal(0); // simple counter trigger
  readonly refreshEvents = this._refreshCounter.asReadonly();

  private readonly retryPolicy = new RetryPolicy({ initialDelayMs: 1_000, maxDelayMs: 30_000 });
  private readonly hubPath = 'alerts-hub';
  private hubUrl = '';

  private readonly log = injectLogger('Alerts');
  private readonly apiConfiguration = inject(ApiConfiguration);

  constructor() {
    this.hubUrl = `${this.apiConfiguration.rootUrl}${this.hubPath}`;
    this.startConnection();
    this.addEventListeners();
  }

  getAlerts(): AlertDto[] {
    return this._alerts();
  }

  triggerRefresh(): void {
    this._refreshCounter.update((v) => v + 1);
  }

  private startConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl)
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (ctx) => this.retryPolicy.delayFor(ctx.previousRetryCount),
      })
      .build();

    this.hubConnection.onreconnecting((error) => {
      this.log.warn('Connection lost. Reconnecting.', { error, toast: true });
      this._alerts.set([]);
    });

    this.hubConnection.onclose((error) => {
      if (error) this.log.error('Connection closed.', { error, toast: true });
      else this.log.info('Connection closed.');
      // Closed with an error means automatic reconnect gave up; a plain stop() passes no error
      if (error) this.startWithRetry();
    });

    this.startWithRetry();
  }

// withAutomaticReconnect only covers dropped connections, not a failed first start()
  private async startWithRetry(attempt = 0): Promise<void> {
    try {
      await this.hubConnection.start();
    } catch (err) {
      const delay = this.retryPolicy.delayFor(attempt);
      if (delay === null) {
        this.log.error('Giving up connecting to alerts hub.', { error: err, toast: true });
        return;
      }
      this.log.error('Connection error, retrying.', { error: err, data: { delayMs: Math.round(delay) }, toast: true });
      setTimeout(() => this.startWithRetry(attempt + 1), delay);
    }
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
