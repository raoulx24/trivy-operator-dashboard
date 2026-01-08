import { Injectable, signal, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

import { ApiConfiguration } from '../../api/api-configuration';
import { AlertDto } from '../../api/models/alert-dto';
import { RetryPolicyUtils } from '../utils/retry-policy.utils';

@Injectable({
  providedIn: 'root',
})
export class AlertsService {
  private hubConnection!: HubConnection;

  private readonly _alerts = signal<AlertDto[]>([]);
  readonly alerts = this._alerts.asReadonly();

  private readonly _refreshCounter = signal(0); // simple counter trigger
  readonly refreshEvents = this._refreshCounter.asReadonly();

  private retryPolicy = new RetryPolicyUtils();
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
        nextRetryDelayInMilliseconds: () => this.retryPolicy.nextDelayMs(),
      })
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.retryPolicy.resetCounter();
      })
      .catch((err) => {
        console.error('Connection error ', err);
        this.retryConnection();
      });

    this.hubConnection.onreconnecting((error) => {
      console.warn(`Connection lost due to ${error}. Reconnecting...`);
      this._alerts.set([]); // clear alerts
    });

    this.hubConnection.onreconnected(() => {
      this.retryPolicy.resetCounter();
    });

    this.hubConnection.onclose(() => console.error('Connection closed.'));
  }

  private retryConnection() {
    setTimeout(() => {
      this.hubConnection.start().catch((err) => {
        console.error('Retry connection error', err);
        this.retryConnection();
      });
    }, this.retryPolicy.nextDelayMs());
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
