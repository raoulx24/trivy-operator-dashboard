import { HttpErrorResponse } from '@angular/common/http';
import { LogError } from './log.models';

export function normalizeError(value: unknown): LogError | undefined {
  if (value === undefined || value === null) return undefined;

  if (value instanceof HttpErrorResponse) {
    return { name: value.name, message: value.message, status: value.status };
  }

  if (value instanceof Error) {
    return { name: value.name, message: value.message, stack: value.stack };
  }

  if (typeof value === 'string') {
    return { name: 'Error', message: value };
  }

  let message: string;
  try {
    message = JSON.stringify(value);
  } catch {
    message = String(value);
  }
  return { name: 'Error', message };
}
