import { SeverityUtils } from '@shared/utils/severity.utils';

// what the collapsed widgets of the changes (new vulnerabilities, new fixes, known exploited) share

const DAY = 86_400_000;

// every UTC day ("yyyy-MM-dd") from `from` to `today`, both included; none without a start
export function dayRange(from: string | undefined, today: string): string[] {
  if (!from) {
    return [];
  }

  const start = Date.parse(from.substring(0, 10));
  const count = Math.round((Date.parse(today) - start) / DAY) + 1;

  return Array.from({ length: Math.max(0, count) }, (_, i) => new Date(start + i * DAY).toISOString().substring(0, 10));
}

// the index of the first day that is the window start's day or later (the days of "since last visit" are highlighted from
// there); null when there is none
export function firstDayIndex(days: string[], since: string): number | null {
  const day = since.substring(0, 10);
  const index = days.findIndex((x) => x >= day);

  return index < 0 ? null : index;
}

// the whole days of the window (at least 1): "2 days"
export function windowDaysLabel(since: string): string {
  const days = Math.max(1, Math.round((Date.now() - Date.parse(since)) / DAY));

  return `${days} ${days === 1 ? 'day' : 'days'}`;
}

// the colors of the severities of the charts: the palette ones, as the strip of the page uses
export function severityColor(severityId: number): string {
  return `var(--p-${SeverityUtils.getColor(severityId)}-500)`;
}

export function severityName(severityId: number): string {
  const name = SeverityUtils.severityDtos.find((x) => x.id === severityId)?.name ?? String(severityId);

  return name.charAt(0) + name.substring(1).toLowerCase();
}

// the text on a segment of the severity colors: dark on the light ones (yellow), white on the others
export function severityTextColor(severityId: number): string {
  return SeverityUtils.getColor(severityId) === 'yellow' ? '#222' : '#fff';
}
