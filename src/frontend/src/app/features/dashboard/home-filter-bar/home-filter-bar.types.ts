// the "since" window of the widgets about changes; stored as a choice (not as a date), so it means the same tomorrow
export type HomeWindowMode = 'lastVisit' | '24h' | '7d' | 'history';

export interface HomeOption<T> {
  label: string;
  value: T;
}

export const homeWindowOptions: HomeOption<HomeWindowMode>[] = [
  { label: 'Since last visit', value: 'lastVisit' },
  { label: 'Last 24 hours', value: '24h' },
  { label: 'Last 7 days', value: '7d' },
  { label: 'History window', value: 'history' },
];

// severity ids as the backend uses them (0 critical ... 4 unknown)
export const homeSeverityOptions: HomeOption<number>[] = [
  { label: 'Critical', value: 0 },
  { label: 'High', value: 1 },
  { label: 'Medium', value: 2 },
  { label: 'Low', value: 3 },
  { label: 'Unknown', value: 4 },
];

export const homeDefaultSeverityIds: number[] = [0, 1];
