import { inject } from '@angular/core';
import { BoundLogger, LogService } from './log.service';

// must be called in an injection context (field initializer / constructor)
export function injectLogger(source: string | (() => string)): BoundLogger {
  return inject(LogService).for(source);
}
