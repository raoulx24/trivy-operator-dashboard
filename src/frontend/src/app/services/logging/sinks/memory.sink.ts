import { Injectable, signal } from '@angular/core';
import { LogSink } from '../log-sink';
import { LogEntry, StoredLogEntry } from '../log.models';

// bounded in-memory history, source for a future message-center UI
@Injectable({ providedIn: 'root' })
export class MemorySink implements LogSink {
  static readonly maxEntries = 500;

  private nextId = 1; // never reused, not even after clear(), so a row key stays unique for the session
  private readonly _entries = signal<StoredLogEntry[]>([]);
  private readonly _lastEntry = signal<StoredLogEntry | null>(null);

  readonly entries = this._entries.asReadonly();
  readonly lastEntry = this._lastEntry.asReadonly();

  write(entry: LogEntry): void {
    // stored as a copy, the other sinks keep receiving the entry without an id
    const stored: StoredLogEntry = { ...entry, id: this.nextId++ };
    this._lastEntry.set(stored);
    this._entries.update((list) => {
      const next = [...list, stored];
      return next.length > MemorySink.maxEntries ? next.slice(next.length - MemorySink.maxEntries) : next;
    });
  }

  clear() {
    this._lastEntry.set(null);
    this._entries.set([]);
  }
}
