import { InjectionToken } from '@angular/core';
import { LogEntry } from './log.models';

export interface LogSink {
  write(entry: LogEntry): void;
}

// multi-token: every provider registered under it receives every entry
export const LOG_SINKS = new InjectionToken<LogSink[]>('LOG_SINKS');
