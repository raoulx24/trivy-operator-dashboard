// one OS cycle with its end dates; dates are "yyyy-MM-dd"
export interface EolRunwayItem {
  label: string;
  eolFrom: string;
  // end of active support; absent when the OS has no such phase
  eoasFrom?: string | null;
  images: number;
}

export type EolRunwayState = 'supported' | 'eoas' | 'eol';

export interface EolRunwayBar {
  label: string;
  state: EolRunwayState;
  y: number;
  width: number;
  // the end is beyond the right edge of the axis
  clipped: boolean;
  imagesLabel: string;
  tooltip: string;
}

export interface EolRunwayTick {
  label: string;
  x: number;
}
