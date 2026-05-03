import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'friendlyTime',
  standalone: true,
})
export class FriendlyTimePipe implements PipeTransform {
  private datePipe = new DatePipe('en-US');

  transform(data: string | undefined, local: boolean = false): string {
    if (!data) {
      return '';
    }
    return this.formatUtcToFriendly(data, local);
  }

  private formatUtcToFriendly(utcDateString: string, local: boolean): string {
    const timezone = local ? undefined : 'UTC';
    const timeFormat = local ? 'yyyy-MM-dd HH:mm' : 'yyyy-MM-dd HH:mm:ss';

    const formatted = this.datePipe.transform(utcDateString, timeFormat, timezone);

    if (!formatted) {
      return 'Invalid date';
    }

    // const suffix = local ? Intl.DateTimeFormat().resolvedOptions().timeZone : 'UTC';
    const suffix = local ? '(local)' : 'UTC';

    return `${formatted} ${suffix}`;
  }
}
