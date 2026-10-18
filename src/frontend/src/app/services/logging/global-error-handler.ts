import { ErrorHandler, Injectable, inject } from '@angular/core';
import { LogService } from './log.service';
import { normalizeError } from './normalize-error';

// catches everything nobody handled (also window errors and unhandled rejections,
// via provideBrowserGlobalErrorListeners); shown to the user as a sticky error toast
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private static readonly maxMessageLength = 200; // full text stays in console and memory sinks

  private readonly log = inject(LogService).for('Application');

  handleError(error: unknown): void {
    const detail = normalizeError(error)?.message?.trim();
    const message = detail ? `Unexpected error: ${this.truncate(detail)}` : 'Unexpected error.';

    this.log.error(message, { error });
  }

  private truncate(text: string): string {
    const max = GlobalErrorHandler.maxMessageLength;
    return text.length > max ? `${text.slice(0, max)}…` : text;
  }
}
