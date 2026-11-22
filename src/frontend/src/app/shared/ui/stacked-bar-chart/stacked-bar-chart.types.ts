// one stacked layer: the same series (name, color) in every column; the first is at the bottom of the bars
export interface StackedBarSeries {
  name: string;
  // any css color
  color: string;
}

export interface StackedBarColumn {
  // the title of the popover, e.g. the day
  label: string;
  // one value per series (same order as the series of the control); 0 or missing = nothing in that layer
  values: number[];
}

export interface StackedBarClick {
  columnIndex: number;
  column: StackedBarColumn;
  total: number;
}

export interface StackedBarLayer {
  series: StackedBarSeries;
  value: number;
  // the svg path of the layer (the top one has its top corners rounded)
  path: string;
}

export interface StackedBarBar {
  index: number;
  column: StackedBarColumn;
  total: number;
  // px, in the svg: the slot of the column (the hit area, and the track when highlighted)
  x: number;
  width: number;
  // only the layers with a value, in the order of the series
  layers: StackedBarLayer[];
  highlighted: boolean;
  // the bar with the data it was made of: a bar with new data is a new element, so its growth runs again
  key: string;
  // ms after which the bar starts to grow (animate); null: not animated
  growDelay: number | null;
}

// the context of the popover of a column
export interface StackedBarTipContext {
  label: string;
  total: number;
  // only the layers with a value
  layers: StackedBarLayer[];
  columnIndex: number;
}
