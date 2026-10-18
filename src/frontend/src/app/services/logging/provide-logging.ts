import { EnvironmentProviders, ErrorHandler, makeEnvironmentProviders } from '@angular/core';
import { GlobalErrorHandler } from './global-error-handler';
import { LOG_SINKS } from './log-sink';
import { ConsoleSink } from './sinks/console.sink';
import { MemorySink } from './sinks/memory.sink';
import { ToastSink } from './sinks/toast.sink';

// to add a sink (e.g. backend), add one more { provide: LOG_SINKS, useExisting: ..., multi: true }
export function provideLogging(): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    { provide: LOG_SINKS, useExisting: ConsoleSink, multi: true },
    { provide: LOG_SINKS, useExisting: ToastSink, multi: true },
    { provide: LOG_SINKS, useExisting: MemorySink, multi: true },
  ]);
}
