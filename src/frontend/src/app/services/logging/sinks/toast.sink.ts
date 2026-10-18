import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { LogSink } from '../log-sink';
import { LogEntry, LogLevel } from '../log.models';

const TOAST_SEVERITY: Record<LogLevel, string> = {
  debug: 'secondary',
  info: 'info',
  warn: 'warn',
  error: 'error',
};

@Injectable({ providedIn: 'root' })
export class ToastSink implements LogSink {
  private readonly toastDuration = 5000;
  private readonly messageService = inject(MessageService);

  write(entry: LogEntry): void {
    if (!(entry.toast ?? this.isToastedByDefault(entry.level))) return;

    // errors always stay until the user closes them
    const sticky = entry.level === 'error';

    this.messageService.add({
      severity: TOAST_SEVERITY[entry.level],
      summary: entry.source,
      detail: entry.message,
      sticky,
      closable: true,
      life: sticky ? undefined : this.toastDuration,
    });
  }

  private isToastedByDefault(level: LogLevel): boolean {
    return level === 'warn' || level === 'error';
  }
}
