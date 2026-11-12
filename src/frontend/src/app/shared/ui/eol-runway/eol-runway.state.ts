import { EolRunwayState } from './eol-runway.types';

export const EOL_RISK_DAYS = 30;

const DAY_MS = 86_400_000;

// "yyyy-MM-dd" (or a longer ISO string) as UTC midnight, in ms; undefined when it is not a date
export function parseEolDay(day: string | null | undefined): number | undefined {
  if (!day) return undefined;
  const ms = Date.parse(`${day.slice(0, 10)}T00:00:00Z`);
  return Number.isNaN(ms) ? undefined : ms;
}

// "in 1y 7m", "10 days ago" (ms)
export function eolRelative(ms: number, today: number): string {
  const days = Math.round((ms - today) / DAY_MS);
  if (days === 0) return 'today';

  const abs = Math.abs(days);
  let text: string;
  if (abs < 60) {
    text = `${abs} day${abs === 1 ? '' : 's'}`;
  } else {
    const months = Math.round(abs / 30.4375);
    text = months < 24 ? `${months} months` : `${Math.floor(months / 12)}y ${months % 12}m`;
  }
  return days > 0 ? `in ${text}` : `${text} ago`;
}

// the state of an OS cycle on a day (all in ms): end of life passed; risky (the end of life is in less than riskDays
// days, whatever the support phase); only security fixes (end of active support passed); supported
export function eolStateOf(eol: number, eoas: number | undefined, today: number, riskDays = EOL_RISK_DAYS): EolRunwayState {
  if (eol <= today) return 'eol';
  if ((eol - today) / DAY_MS < riskDays) return 'risky';
  return eoas !== undefined && eoas <= today ? 'eoas' : 'supported';
}
