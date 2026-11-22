export type ScatterShape = 'circle' | 'diamond' | 'square' | 'triangle' | 'cross';

// one point; T is whatever the owner wants back about it (the selection returns it as it is)
export interface ScatterPoint<T = unknown> {
  // the identity of the point across new data (e.g. an id): a point whose id was there stays (and moves if its position
  // changed), a new id fades in and a missing one fades out; none = the index in the input
  id?: string;
  // data coordinates
  x: number;
  y: number;
  // any css color; none = the primary color
  color?: string;
  // none = circle
  shape?: ScatterShape;
  // a small dot in the middle of the point, with this color (any css color); none = no dot
  centerColor?: string;
  // the title of the popover
  label?: string;
  data?: T;
}

export interface ScatterAxis {
  // written at the end of the axis (x: bottom right, y: top left)
  label?: string;
  // none = linear. Log: values <= 0 are drawn at the start of the axis
  scale?: 'linear' | 'log';
  // none = from the data, with some room
  min?: number;
  max?: number;
  // none = compact number
  format?: (value: number) => string;
}

export interface ScatterLegendItem {
  label: string;
  // the same color / shape as the points it explains; none = the primary color / circle
  color?: string;
  shape?: ScatterShape;
  // the popover of the item (on hover)
  hint?: string;
}

export interface ScatterVertex {
  x: number;
  y: number;
}

// a drawn region, in data coordinates
export interface ScatterRegion {
  polygon: ScatterVertex[];
}

// what the owner gets: the union of the regions
export interface ScatterSelection<T = unknown> {
  // the points inside, in the order of the input
  points: ScatterPoint<T>[];
  // their indexes in the input
  indexes: number[];
  regions: ScatterRegion[];
}

// a point as it is drawn
export interface ScatterPointView<T = unknown> {
  index: number;
  point: ScatterPoint<T>;
  // px, in the svg
  px: number;
  py: number;
  // the shape, around (0, 0): the element is moved to px, py (so it can glide when its position changes)
  path: string;
  color: string | null;
  centerColor: string | null;
  selected: boolean;
  // there is a selection and the point is not in it
  dimmed: boolean;
  // the identity of the element: the id of the point, else its index
  key: string;
  // ms after which a new point fades in (animate); null: it was there before (or no animation)
  litDelay: number | null;
}

export interface ScatterTick {
  value: number;
  // px
  at: number;
  text: string;
}

// the context of the popover of a point
export interface ScatterTipContext<T = unknown> {
  view: ScatterPointView<T>;
  xText: string;
  yText: string;
  xLabel: string;
  yLabel: string;
}
