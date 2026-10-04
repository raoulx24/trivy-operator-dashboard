import { LocalStorageUtils } from '../utils/local-storage.utils';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AccessTrackingService {
  private readonly maxDays = 3;

  trackAccess(componentBaseKey: string): AccessDay[] {
    const key = LocalStorageUtils.toCamelCase(`${componentBaseKey}.access days`);

    const stored = localStorage.getItem(key);
    let days: AccessDay[] = stored ? JSON.parse(stored) : [];

    const now = new Date();
    const todayUtc = now.toISOString().substring(0, 10);
    const nowUtc = now.toISOString();

    days = days.filter((d) => d.day !== todayUtc);

    days.unshift({
      day: todayUtc,
      utc: nowUtc,
    });

    days = days.slice(0, this.maxDays);

    localStorage.setItem(key, JSON.stringify(days));

    return days;
  }
}

export interface AccessDay {
  day: string;
  utc: string;
}
