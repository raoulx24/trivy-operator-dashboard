import { inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

import { ApiConfiguration } from '../../api/api-configuration';
import { AlertDto } from '../../api/models/alert-dto';
import { RetryPolicy } from '../utils/retry-policy.utils';

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
      console.warn(`Connection lost due to ${error}. Reconnecting...`);
      this._alerts.set([]);
    });

    this.hubConnection.onclose((error) => {
      console.error('Connection closed.', error);
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
        console.error('Giving up connecting to alerts hub', err);
        return;
      }
      console.error(`Connection error, retrying in ${Math.round(delay)}ms`, err);
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
