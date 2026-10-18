import { Injectable, inject } from '@angular/core';
import { LOG_SINKS } from './log-sink';
import { BoundLogOptions, LogEntry, LogLevel, LogOptions } from './log.models';
import { normalizeError } from './normalize-error';

export interface BoundLogger {
  debug(message: string, options?: BoundLogOptions): void;
  info(message: string, options?: BoundLogOptions): void;
  warn(message: string, options?: BoundLogOptions): void;
  error(message: string, options?: BoundLogOptions): void;
}

@Injectable({ providedIn: 'root' })
export class LogService {
  private readonly sinks = inject(LOG_SINKS);

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

  // logger with a fixed source, so call sites do not repeat it
  for(source: string): BoundLogger {
    return {
      debug: (message, options) => this.debug(message, { ...options, source }),
      info: (message, options) => this.info(message, { ...options, source }),
      warn: (message, options) => this.warn(message, { ...options, source }),
      error: (message, options) => this.error(message, { ...options, source }),
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
