// a value per UTC day; day is "yyyy-MM-dd"
export interface ActivityHeatmapDayDto {
  day: string;
  value: number;
}

export type ActivityHeatmapRows = 3 | 4;

export interface ActivityHeatmapCell {
  day: string;
  value: number;
  // 0 = empty, 1..n = quantile step
  level: number;
  emphasis1: boolean;
  emphasis2: boolean;
  // with selectableDays: the day can be clicked; a day with a value that cannot is inactive (dimmed)
  selectable: boolean;
  inactive: boolean;
  selected: boolean;
  x: number;
  y: number;
}

export interface ActivityHeatmapWeek {
  // "MM-dd" of the first shown day of the week
  label: string;
  x: number;
  cells: ActivityHeatmapCell[];
}
