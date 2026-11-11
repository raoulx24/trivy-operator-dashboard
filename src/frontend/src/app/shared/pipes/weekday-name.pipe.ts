import { Pipe, PipeTransform } from '@angular/core';

// the full English weekday name of a "yyyy-MM-dd" day (read as UTC): "2026-11-11" -> "Wednesday"
@Pipe({
  name: 'weekdayName',
  standalone: true,
})
export class WeekdayNamePipe implements PipeTransform {
  private static readonly format = new Intl.DateTimeFormat('en', { weekday: 'long', timeZone: 'UTC' });

  transform(day: string | undefined | null): string {
    if (!day) {
      return '';
    }
    const ms = Date.parse(`${day.slice(0, 10)}T00:00:00Z`);
    return Number.isNaN(ms) ? '' : WeekdayNamePipe.format.format(ms);
  }
}
