import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

import { ApiConfiguration } from '../../api/api-configuration';

import { AlertDto } from '../../api/models/alert-dto';
import { RetryPolicyUtils } from '../utils/retry-policy.utils';

@Injectable({
  providedIn: 'root',
})
export class AlertsService {
  private hubConnection!: HubConnection;
  private alertsSubject = new BehaviorSubject<AlertDto[]>([]);
  public alerts$: Observable<AlertDto[]> = this.alertsSubject.asObservable();

  private retryPolicy = new RetryPolicyUtils();
  private readonly hubPath: string = 'alerts-hub';
  private hubUrl: string = '';

  private readonly refreshTrigger = new Subject<void>();

  constructor(private apiConfiguration: ApiConfiguration) {
    this.hubUrl = apiConfiguration.rootUrl ? new URL(this.hubPath, apiConfiguration.rootUrl).toString() : this.hubPath;
    this.startConnection();
    this.addEventListeners();
  }

  getAlerts(): AlertDto[] {
    return this.alertsSubject.value;
  }

  onRefresh(): Observable<void> {
    return this.refreshTrigger.asObservable();
  }

  triggerRefresh(): void {
    this.refreshTrigger.next();
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
      this.alertsSubject.next([]);
    });

    this.hubConnection.onreconnected((connectionId) => {
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
    const currentAlerts = this.alertsSubject.value;
    this.alertsSubject.next([...currentAlerts, alert]);
  }

  private removeAlert(alert: AlertDto) {
    const currentAlerts = this.alertsSubject.value.filter(
      (a) => a.emitter !== alert.emitter || a.emitterKey !== alert.emitterKey,
    );
    this.alertsSubject.next(currentAlerts);
  }
}
