import { Signal, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

import { BoundLogger } from '../logging/log.service';
import { RetryPolicy } from '@shared/utils/retry-policy.utils';

export interface ResilientHubConnectionOptions {
  // the full url of the hub (the api root + the hub path, so it also works under an ingress path)
  url: string;
  log: BoundLogger;
  retryPolicy?: RetryPolicy;
  // true: every connection message goes to debug, without a toast (e.g. when another hub already reports that the
  // server is unreachable: the alerts hub)
  isQuiet?: boolean;
  // the connection dropped and automatic reconnect started (e.g. clear what the hub keeps up to date)
  onReconnecting?: () => void;
  // after the first start and after every reconnect: events sent meanwhile are lost, so reload what the hub tracks
  onConnected?: () => void;
}

/**
 * A SignalR connection that keeps itself up:
 * - the first start() is retried (withAutomaticReconnect does not cover a failed first start);
 * - a dropped connection reconnects automatically, with the same retry policy;
 * - when automatic reconnect gives up (close with an error), it starts over.
 * The owner registers its handlers with on() before start().
 */
export class ResilientHubConnection {
  private readonly connection: HubConnection;
  private readonly retryPolicy: RetryPolicy;
  private readonly isQuiet: boolean;
  private readonly _isConnected = signal<boolean>(false);
  private isStopped = false;

  readonly isConnected: Signal<boolean> = this._isConnected.asReadonly();

  constructor(private readonly options: ResilientHubConnectionOptions) {
    this.retryPolicy = options.retryPolicy ?? new RetryPolicy({ initialDelayMs: 1_000, maxDelayMs: 30_000 });
    this.isQuiet = options.isQuiet ?? false;

    this.connection = new HubConnectionBuilder()
      .withUrl(options.url)
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (ctx) => this.retryPolicy.delayFor(ctx.previousRetryCount),
      })
      .build();

    this.connection.onreconnecting((error) => {
      this._isConnected.set(false);
      this.report('warn', 'Connection lost. Reconnecting.', { error });
      options.onReconnecting?.();
    });

    this.connection.onreconnected(() => {
      this._isConnected.set(true);
      options.onConnected?.();
    });

    this.connection.onclose((error) => {
      this._isConnected.set(false);
      if (error) this.report('error', 'Connection closed.', { error });
      else options.log.info('Connection closed.');
      // closed with an error: automatic reconnect gave up; a plain stop() passes no error
      if (error && !this.isStopped) this.startWithRetry();
    });
  }

  on(methodName: string, handler: (...args: any[]) => void): void {
    this.connection.on(methodName, handler);
  }

  start(): void {
    this.isStopped = false;
    void this.startWithRetry();
  }

  async stop(): Promise<void> {
    this.isStopped = true;
    await this.connection.stop();
  }

  private async startWithRetry(attempt = 0): Promise<void> {
    if (this.isStopped) {
      return;
    }

    try {
      await this.connection.start();
      this._isConnected.set(true);
      this.options.onConnected?.();
    } catch (err) {
      const delay = this.retryPolicy.delayFor(attempt);
      if (delay === null) {
        this.report('error', 'Giving up connecting.', { error: err });
        return;
      }
      this.report('error', 'Connection error, retrying.', { error: err, data: { delayMs: Math.round(delay) } });
      setTimeout(() => this.startWithRetry(attempt + 1), delay);
    }
  }

  // a connection problem: at its level with the default toast, or quietly at debug
  private report(level: 'warn' | 'error', message: string, options: { error?: unknown; data?: Record<string, unknown> }) {
    if (this.isQuiet) {
      this.options.log.debug(message, { ...options, toast: false });
    } else {
      this.options.log[level](message, { ...options, toast: true });
    }
  }
}
