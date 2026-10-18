import { Injectable, isDevMode } from '@angular/core';
import { LogSink } from '../log-sink';
import { LogEntry } from '../log.models';

@Injectable({ providedIn: 'root' })
export class ConsoleSink implements LogSink {
  write(entry: LogEntry): void {
    if (entry.level === 'debug' && !isDevMode()) return;

    const extras = [entry.code, entry.error, entry.data].filter((x) => x !== undefined);
    const repeated = entry.suppressed ? ` (+${entry.suppressed} repeated)` : '';
    console[entry.level](`[${entry.source}] ${entry.message}${repeated}`, ...extras);
  }
}
