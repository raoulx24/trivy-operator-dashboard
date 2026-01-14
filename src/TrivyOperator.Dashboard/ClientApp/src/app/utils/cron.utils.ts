export class CronUtils {
  static daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  static monthsOfYear = [
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

  static parseCron(cron: string) {
    const [minute, hour, dayOfMonth, month, dayOfWeek] = cron.split(' ');

    const readable = [];

    // Minute
    if (minute === '*') {
      readable.push('every minute');
    } else {
      readable.push(`at minute ${minute}`);
    }

    // Hour
    if (hour === '*') {
      readable.push('every hour');
    } else {
      readable.push(`at hour ${hour}`);
    }

    // Day of Month
    if (dayOfMonth === '*') {
      readable.push('every day');
    } else {
      readable.push(`on day ${dayOfMonth}`);
    }

    // Month
    if (month === '*') {
      readable.push('every month');
    } else {
      const monthNames = month.split(',').map((m) => this.monthsOfYear[parseInt(m, 10) - 1]);
      readable.push(`in ${monthNames.join(', ')}`);
    }

    // Day of Week
    if (dayOfWeek === '*') {
      readable.push('on every day of the week');
    } else {
      const weekDays = dayOfWeek.split(',').map((d) => this.daysOfWeek[parseInt(d, 10)]);
      readable.push(`on ${weekDays.join(', ')}`);
    }

    return readable.join(', ');
  }

  // Example usage:
  //const cronExpression = '0 12 * * 1';
  //const humanReadable = parseCron(cronExpression);
  //console.log(humanReadable);  // Output: "at minute 0, at hour 12, every day, every month, on Monday"
}
