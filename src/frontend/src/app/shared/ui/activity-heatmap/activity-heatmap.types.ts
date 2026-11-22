// everything the heatmap shows about a UTC day; day is "yyyy-MM-dd"
export interface ActivityHeatmapDayDto {
  day: string;
  value: number;
  // the corner markers (emphasis1: bottom-left, emphasis2: top-right), independent of the value
  emphasis1?: boolean;
  emphasis2?: boolean;
  // true: the day can be clicked; false: it cannot (a day with a value is then dimmed); not set on any day: the heatmap
  // only shows (nothing is clickable, nothing is dimmed)
  selectable?: boolean;
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
  // ms after which the cell lights on (animate); null: not animated (an empty cell without a marker)
  litDelay: number | null;
  x: number;
  y: number;
}

export interface ActivityHeatmapWeek {
  // the week, and the data it was made of: a week with new data is a new element, so its animation runs again
  key: string;
  // "MM-dd" of the first shown day of the week
  label: string;
  x: number;
  cells: ActivityHeatmapCell[];
}
