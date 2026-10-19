import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'friendlyTime',
  standalone: true,
})
export class FriendlyTimePipe implements PipeTransform {
  private datePipe = new DatePipe('en-US');

  transform(data: string | undefined, local: boolean = false, justDate: boolean = false): string {
    if (!data) {
      return 'N/A';
    }
    return this.formatUtcToFriendly(data, local, justDate);
  }

  private formatUtcToFriendly(utcDateString: string, local: boolean, justDate: boolean): string {
    const timezone = local ? undefined : 'UTC';
    let timeFormat = 'yyyy-MM-dd';
    if (!justDate) {
      timeFormat = local ? 'yyyy-MM-dd HH:mm' : 'yyyy-MM-dd HH:mm:ss';
    }

    let formatted: string | null = null;

    try {
      formatted = this.datePipe.transform(utcDateString, timeFormat, timezone);
    } catch {
      // swallow the error
      return 'N/A';
    }

    if (!formatted) {
      return 'N/A';
    }

    // const suffix = local ? Intl.DateTimeFormat().resolvedOptions().timeZone : 'UTC';
    const suffix = local ? '(local)' : '';

    return `${formatted} ${suffix}`;
  }
}
