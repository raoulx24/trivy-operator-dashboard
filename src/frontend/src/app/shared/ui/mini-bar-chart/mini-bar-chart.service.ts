import { Injectable, signal } from '@angular/core';
import { injectLogger } from '@core/logging/inject-logger';

@Injectable({
  providedIn: 'root',
})
export class MiniBarChartService {
  private readonly log = injectLogger('MiniBarChart');

  private _minDays = signal<number | undefined>(undefined);
  private _highlightedDays = signal<number| undefined>(undefined);

  minDays = this._minDays.asReadonly();
  highlightedDays = this._highlightedDays.asReadonly();

  setMinDays(minDays?: number): void {
    if (minDays && minDays <= 0) {
      this.log.warn('minDays must be greater than 0.', { toast: false });
      return;
    }

    this._minDays.set(minDays);
  }

  setHighlightedDays(highlightedDays?: number): void {
    if (highlightedDays && (highlightedDays <0 || highlightedDays > (this._minDays() ?? 0))) {
      this.log.warn('Highlighted days must be greater than 0 or lower than minDays.', { toast: false });
      return;
    }
    this._highlightedDays.set(highlightedDays);
  }
}
