import { DestroyRef, Injectable, inject } from '@angular/core';
import { LOG_SINKS } from './log-sink';
import { BoundLogOptions, LogEntry, LogLevel, LogOptions } from './log.models';
import { normalizeError } from './normalize-error';

export interface BoundLogger {
  debug(message: string, options?: BoundLogOptions): void;
  info(message: string, options?: BoundLogOptions): void;
  warn(message: string, options?: BoundLogOptions): void;
  error(message: string, options?: BoundLogOptions): void;
}

interface SeenEntry {
  lastEmitAt: number;
  suppressed: number; // identical entries dropped since lastEmitAt
  lastEntry: LogEntry;
}

@Injectable({ providedIn: 'root' })
export class LogService {
  // storm protection: identical warn/error entries (same level, source and message) inside the window are
  // dropped and counted; the count is reported on the next emitted entry or by the sweep once the key goes quiet.
  // Variable text belongs in `data`, not in the message, or the entries never match.
  private static readonly dedupeWindowMs = 5_000;
  private static readonly dedupedLevels: readonly LogLevel[] = ['warn', 'error'];

  private readonly sinks = inject(LOG_SINKS);
  private readonly seen = new Map<string, SeenEntry>();
  private sweepTimer: ReturnType<typeof setInterval> | null = null;

  constructor() {
    inject(DestroyRef).onDestroy(() => this.stopSweep());
  }

  debug(message: string, options: LogOptions) {
    this.log('debug', message, options);
  }

  info(message: string, options: LogOptions) {
    this.log('info', message, options);
  }

  warn(message: string, options: LogOptions) {
    this.log('warn', message, options);
  }

  error(message: string, options: LogOptions) {
    this.log('error', message, options);
  }

  // logger with a default source, so call sites do not repeat it; a function is resolved on every call
  // (for sources that change at runtime, e.g. the page title). Holds no state on the service.
  for(source: string | (() => string)): BoundLogger {
    const resolve = (options?: BoundLogOptions): LogOptions => ({
      ...options,
      source: options?.source ?? (typeof source === 'function' ? source() : source),
    });

    return {
      debug: (message, options) => this.debug(message, resolve(options)),
      info: (message, options) => this.info(message, resolve(options)),
      warn: (message, options) => this.warn(message, resolve(options)),
      error: (message, options) => this.error(message, resolve(options)),
    };
  }

  private log(level: LogLevel, message: string, options: LogOptions) {
    const entry: LogEntry = {
      level,
      message,
      source: options.source,
      timestamp: Date.now(),
      code: options.code,
      error: normalizeError(options.error),
      data: options.data,
      toast: options.toast,
    };

    if (LogService.dedupedLevels.includes(level) && this.isSuppressed(entry)) return;

    this.dispatch(entry);
  }

  // true when the entry is a repeat inside the window; otherwise registers it and carries any pending count
  private isSuppressed(entry: LogEntry): boolean {
    const key = `${entry.level}|${entry.source}|${entry.message}`;
    const seen = this.seen.get(key);

    if (seen && entry.timestamp - seen.lastEmitAt < LogService.dedupeWindowMs) {
      seen.suppressed++;
      seen.lastEntry = entry;
      return true;
    }

    if (seen?.suppressed) entry.suppressed = seen.suppressed;
    this.seen.set(key, { lastEmitAt: entry.timestamp, suppressed: 0, lastEntry: entry });
    this.startSweep();
    return false;
  }

  private startSweep() {
    this.sweepTimer ??= setInterval(() => this.sweep(), LogService.dedupeWindowMs);
  }

  private stopSweep() {
    if (this.sweepTimer === null) return;
    clearInterval(this.sweepTimer);
    this.sweepTimer = null;
  }

  // reports the count of keys that went quiet and forgets them; stops itself once nothing is tracked
  private sweep() {
    const now = Date.now();

    for (const [key, seen] of this.seen) {
      if (now - seen.lastEmitAt < LogService.dedupeWindowMs) continue;

      if (seen.suppressed > 0) {
        this.dispatch({ ...seen.lastEntry, timestamp: now, suppressed: seen.suppressed, toast: false });
      }
      this.seen.delete(key);
    }

    if (this.seen.size === 0) this.stopSweep();
  }

  private dispatch(entry: LogEntry) {
    for (const sink of this.sinks) {
      // a failing sink must never break the app or the other sinks, and must not log itself
      try {
        sink.write(entry);
      } catch (e) {
        console.error('Log sink failed', e);
      }
    }
  }
}
