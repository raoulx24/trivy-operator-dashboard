import { Injectable, signal } from '@angular/core';
import { LogSink } from '../log-sink';
import { LogEntry } from '../log.models';

// bounded in-memory history, source for a future message-center UI
@Injectable({ providedIn: 'root' })
export class MemorySink implements LogSink {
  static readonly maxEntries = 500;

  private readonly _entries = signal<LogEntry[]>([]);
  private readonly _lastEntry = signal<LogEntry | null>(null);

  readonly entries = this._entries.asReadonly();
  readonly lastEntry = this._lastEntry.asReadonly();

  write(entry: LogEntry): void {
    this._lastEntry.set(entry);
    this._entries.update((list) => {
      const next = [...list, entry];
      return next.length > MemorySink.maxEntries ? next.slice(next.length - MemorySink.maxEntries) : next;
    });
  }

  clear() {
    this._lastEntry.set(null);
    this._entries.set([]);
  }
}
