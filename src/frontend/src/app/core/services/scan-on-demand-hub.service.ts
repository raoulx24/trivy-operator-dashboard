import { inject, Injectable, Signal, signal } from '@angular/core';

import { ApiConfiguration } from '@api/api-configuration';
import { injectLogger } from '../logging/inject-logger';
import { ResilientHubConnection } from '../signalr/resilient-hub-connection';
import { MainAppInitService } from './main-app-init.service';

// the scans list changed on the server (ScanOnDemandHub "ScansChanged"): the page reads it again
// connects only when scan on demand is enabled; created by the first one that injects it, then lives with the app
@Injectable({
  providedIn: 'root',
})
export class ScanOnDemandHubService {
  private readonly hubPath = 'scan-on-demand-hub';
  private readonly hubConnection?: ResilientHubConnection;

  private readonly _changes = signal(0);
  // bumped on every "ScansChanged" and after every (re)connect (changes sent while disconnected are lost)
  readonly changes = this._changes.asReadonly();
  // false also while scan on demand is disabled; the page polls while it is false
  readonly isConnected: Signal<boolean>;

  constructor() {
    if (!inject(MainAppInitService).backendSettingsDto().isScanOnDemandEnabled) {
      this.isConnected = signal(false).asReadonly();
      return;
    }

    this.hubConnection = new ResilientHubConnection({
      url: `${inject(ApiConfiguration).rootUrl}${this.hubPath}`,
      log: injectLogger('Scan on demand hub'),
      // the alerts hub already reports when the server is unreachable; here only debug
      isQuiet: true,
      onConnected: () => this.bump(),
    });
    this.isConnected = this.hubConnection.isConnected;
    this.hubConnection.on('ScansChanged', () => this.bump());
    this.hubConnection.start();
  }

  private bump() {
    this._changes.update((x) => x + 1);
  }
}
