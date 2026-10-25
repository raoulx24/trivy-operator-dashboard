import { LocalStorageUtils } from '@shared/utils/local-storage.utils';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AccessTrackingService {
  private readonly maxDays = 3;

  trackAccess(componentBaseKey: string): AccessDay[] {
    const key = LocalStorageUtils.toCamelCase(`${componentBaseKey}.access days`);

    const stored = LocalStorageUtils.getItem(key);
    let days: AccessDay[] = [];
    try {
      const parsed = stored ? JSON.parse(stored) : [];
      days = Array.isArray(parsed) ? parsed : [];
    } catch {
      LocalStorageUtils.removeItem(key);
    }

    const now = new Date();
    const todayUtc = now.toISOString().substring(0, 10);
    const nowUtc = now.toISOString();

    days = days.filter((d) => d.day !== todayUtc);

    days.unshift({
      day: todayUtc,
      utc: nowUtc,
    });

    days = days.slice(0, this.maxDays);

    LocalStorageUtils.setItem(key, JSON.stringify(days));

    return days;
  }
}

export interface AccessDay {
  day: string;
  utc: string;
}
