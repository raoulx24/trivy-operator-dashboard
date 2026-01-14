import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'cron',
  standalone: true,
})
export class CronPipe implements PipeTransform {
  transform(data: string | undefined): string {
    if (!data) {
      return '';
    }
    return this.parseCron(data);
  }

  private daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  private monthsOfYear = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
    'August',
    'September',
    'October',
    'November',
    'December',
  ];

  private parseCron(cron: string): string {
    if (cron.split(' ').length != 5) {
      return 'Unknown format';
    }

    const [minute, hour, dayOfMonth, month, dayOfWeek] = cron.split(' ');

    const readable = [];

    // Minute
    if (minute === '*') {
      readable.push('every minute');
    } else {
      if (minute.startsWith('*/')) {
        readable.push(`every ${minute.replace('*/', '')} minute(s)`);
      } else {
        readable.push(`at minute ${minute}`);
      }
    }

    // Hour
    if (hour === '*') {
      readable.push('every hour');
    } else {
      if (hour.startsWith('*/')) {
        readable.push(`every ${hour.replace('*/', '')} hour(s)`);
      } else {
        readable.push(`at minute ${hour}`);
      }
    }

    // Day of Month
    if (dayOfMonth === '*') {
      readable.push('every day');
    } else {
      if (dayOfMonth.startsWith('*/')) {
        readable.push(`every ${dayOfMonth.replace('*/', '')} day of month`);
      } else {
        readable.push(`on day ${dayOfMonth}`);
      }
    }

    // Month
    if (month === '*') {
      readable.push('every month');
    } else {
      if (month.startsWith('*/')) {
        readable.push(`every ${month.replace('*/', '')} month(s)`);
      } else {
        const monthNames = month.split(',').map((m) => this.monthsOfYear[parseInt(m, 10) - 1]);
        readable.push(`in ${monthNames.join(', ')}`);
      }
    }

    // Day of Week
    if (dayOfWeek === '*') {
      readable.push('on every day of the week');
    } else {
      if (dayOfWeek.startsWith('*/')) {
        readable.push(`every ${dayOfWeek.replace('*/', '')} day of week`);
      } else {
        const weekDays = dayOfWeek.split(',').map((d) => this.daysOfWeek[parseInt(d, 10)]);
        readable.push(`on ${weekDays.join(', ')}`);
      }
    }

    return readable.join(', ');
  }

  // Example usage:
  //const cronExpression = '0 12 * * 1';
  //const humanReadable = parseCron(cronExpression);
  //console.log(humanReadable);  // Output: "at minute 0, at hour 12, every day, every month, on Monday"
}
