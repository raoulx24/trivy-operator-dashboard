import { Injectable, inject, signal } from '@angular/core';
import { MessageService } from 'primeng/api';

export type TrivyMessageSeverity = 'success' | 'info' | 'warn' | 'error';

export interface TrivyMessage {
  message: string;
  source: string; // page/component/service
  timestamp: number; // UTC epoch ms
  severity: TrivyMessageSeverity;
  code?: string;
  messageDetails?: string;
}

@Injectable({ providedIn: 'root' })
export class TrivyMessageService {
  private readonly _lastTrivyMessage = signal<TrivyMessage | null>(null);
  private readonly _trivyMessages = signal<TrivyMessage[]>([]);

  readonly lastTrivyMessage = this._lastTrivyMessage.asReadonly();
  readonly trivyMessages = this._trivyMessages.asReadonly();

  private readonly toastDuration: number = 5000;

  private readonly messageService = inject(MessageService);

  push(message: TrivyMessage) {
    this._lastTrivyMessage.set(message);
    this._trivyMessages.update((list) => [...list, message]);

    this.messageService.add({
      severity: message.severity,
      summary: message.source,
      detail: message.message,
      life: this.toastDuration,
    });
    console.log("mama");
  }

  pushSimple(message: string, source: string, severity: TrivyMessageSeverity, details?: any) {
    this.push({
      message,
      source,
      severity,
      timestamp: Date.now(),
      messageDetails: this.safeStringifyObject(details),
    });
  }

  clear() {
    this._lastTrivyMessage.set(null);
    this._trivyMessages.set([]);
  }

  private safeStringifyObject(value?: any): string | undefined {
    if (!value) return undefined;

    if (value instanceof Error) {
      return value.message;
    }

    if (typeof value === 'string') {
      return value;
    }

    try {
      return JSON.stringify(value);
    } catch {
      return String(value);
    }
  }

}
