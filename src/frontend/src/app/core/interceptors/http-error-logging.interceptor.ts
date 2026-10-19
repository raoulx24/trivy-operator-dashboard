import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { LogService } from '../logging/log.service';

// records failed HTTP calls (console + memory); no toast, the calling page decides what the user sees.
// The error is rethrown untouched.
export const httpErrorLoggingInterceptor: HttpInterceptorFn = (req, next) => {
  const log = inject(LogService).for('HTTP');

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        log.error(`${req.method} ${req.url} failed (${error.status})`, { error, toast: false });
      }
      return throwError(() => error);
    }),
  );
};
