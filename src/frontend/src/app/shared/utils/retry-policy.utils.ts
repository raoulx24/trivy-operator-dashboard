import { HttpErrorResponse } from '@angular/common/http';
import { MonoTypeOperatorFunction, retry, throwError, timer } from 'rxjs';

export type JitterStrategy = 'none' | 'full' | 'equal';

export interface RetryPolicyOptions {
  initialDelayMs: number;
  maxDelayMs: number;
  multiplier: number;
  jitter: JitterStrategy;
  /** Number of retries before giving up. Use Infinity to retry forever. */
  maxRetries: number;
}

export const DEFAULT_RETRY_POLICY: RetryPolicyOptions = {
  initialDelayMs: 1_000,
  maxDelayMs: 30_000,
  multiplier: 2,
  jitter: 'equal',
  maxRetries: Infinity,
};

export class RetryPolicy {
  readonly options: RetryPolicyOptions;

  constructor(options: Partial<RetryPolicyOptions> = {}) {
    this.options = { ...DEFAULT_RETRY_POLICY, ...options };
  }

  /**
   * Delay before retry number `attempt` (0-based), or null when retries are exhausted.
   * Stateless, so one instance can be shared by any number of callers.
   */
  delayFor(attempt: number): number | null {
    const { initialDelayMs, maxDelayMs, multiplier, jitter, maxRetries } = this.options;
    if (attempt >= maxRetries) return null;

    const capped = Math.min(maxDelayMs, initialDelayMs * multiplier ** attempt);

    switch (jitter) {
      case 'full':
        return Math.random() * capped; // [0, capped)
      case 'equal':
        return capped / 2 + Math.random() * (capped / 2); // [capped/2, capped)
      default:
        return capped;
    }
  }
}

/** Network errors, timeouts, 408, 429 and 5xx are worth retrying. Other 4xx responses are not. */
export function isTransientError(err: unknown): boolean {
  if (err instanceof HttpErrorResponse) {
    return err.status === 0 || err.status === 408 || err.status === 429 || err.status >= 500;
  }
  return true; // TimeoutError, etc.
}

export function retryWithBackoff<T>(
  policy: RetryPolicy,
  shouldRetry: (err: unknown) => boolean = isTransientError,
): MonoTypeOperatorFunction<T> {
  return retry({
    delay: (error, retryCount) => {
      // rxjs retryCount is 1-based
      const delay = shouldRetry(error) ? policy.delayFor(retryCount - 1) : null;
      if (delay === null) return throwError(() => error);

      console.warn(`Retry #${retryCount} in ${Math.round(delay)}ms`, error);
      return timer(delay);
    },
  });
}
