export type SpanTimelineScale = 'linear' | 'log';

// where the value 0 (now) is: at the right end (ages, the default) or at the left end (bars that grow to the right,
// e.g. a share)
export type SpanTimelineOrigin = 'right' | 'left';

export type SpanTimelineMarkerKind = 'dot' | 'diamond' | 'tick';

// a mark on a row (at most 3 are drawn per row); `at` is an age, in the same unit as the start and end of the rows
export interface SpanTimelineMarker {
  at: number;
  // default: dot
  kind?: SpanTimelineMarkerKind;
  // css color; default: the one of the kind (theme)
  color?: string;
  // the text of its popover, e.g. "new vuln added"; the kind name when missing
  label?: string;
}

// one bar: the time between `start` and `end`, both ages (distance back from now: 0 = now, 90 = 90 units ago), so
// start >= end; the unit (days...) is the caller's, the control only draws numbers
export interface SpanTimelineRow {
  // the identity used by the selection; unique
  id: string;
  label: string;
  start: number;
  // default 0 (now)
  end?: number;
  // css color of the bar; default: the control colors, taken in turn
  color?: string;
  // the text on the right, e.g. "212d"
  valueLabel?: string;
  // extra lines of the popover of the row
  details?: string[];
  // a value to take with care (e.g. computed on few items): the bar is faint and its track has a dashed outline
  faint?: boolean;
  markers?: SpanTimelineMarker[];
}

export interface SpanTimelineTick {
  value: number;
  // default: tickFormat(value)
  label?: string;
}

// an entry of the legend: a bar (swatch) or a kind of marker
export interface SpanTimelineLegendItem {
  kind: 'bar' | SpanTimelineMarkerKind;
  label: string;
  // css color; only used by a bar
  color?: string;
  // the text of its popover
  description?: string;
}

export interface SpanTimelineTipContext {
  row: SpanTimelineRow;
  // "30d → today"
  range: string;
}

// what the control draws, computed
export interface SpanTimelineBar {
  row: SpanTimelineRow;
  y: number;
  x: number;
  width: number;
  color: string;
  range: string;
  markers: SpanTimelineMarkerView[];
}

export interface SpanTimelineMarkerView {
  kind: SpanTimelineMarkerKind;
  x: number;
  color: string | null;
  tip: string;
}

export interface SpanTimelineTickView {
  x: number;
  label: string;
}
