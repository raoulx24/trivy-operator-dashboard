export type LogLevel = 'debug' | 'info' | 'warn' | 'error';

export interface LogError {
  name: string;
  message: string;
  stack?: string;
  status?: number; // HTTP status, when the error comes from an HTTP call
}

export interface LogEntry {
  level: LogLevel;
  message: string;
  source: string; // page/component/service
  timestamp: number; // UTC epoch ms
  code?: string;
  error?: LogError;
  data?: unknown; // structured payload, never pre-stringified
  toast?: boolean; // presentation hint for the toast sink; undefined = level default
  suppressed?: number; // identical earlier entries dropped by storm protection and summarized by this one
}

// an entry as kept in memory: the id is assigned on write, so tables can key rows on it
export type StoredLogEntry = LogEntry & { id: number };

export interface LogOptions {
  source: string;
  code?: string;
  error?: unknown; // raw error, normalized into LogEntry.error
  data?: unknown;
  toast?: boolean;
}

// source is optional on a bound logger: it defaults to the bound one but a call can override it
export type BoundLogOptions = Omit<LogOptions, 'source'> & { source?: string };
