// one OS cycle with its end dates; dates are "yyyy-MM-dd"
export interface EolRunwayItem {
  label: string;
  eolFrom: string;
  // end of active support; absent when the OS has no such phase
  eoasFrom?: string | null;
  images: number;
}

export type EolRunwayState = 'supported' | 'eoas' | 'risky' | 'eol';

export interface EolRunwayBar {
  label: string;
  state: EolRunwayState;
  y: number;
  width: number;
  // the end is beyond the right edge of the axis
  clipped: boolean;
  // end of life passed: the length from the end of the bar to today (the time it still runs after its end of life,
  // as far as the axis shows), 0 otherwise
  overdueWidth: number;
  imagesLabel: string;
  tip: EolRunwayTip;
}

// what the popover of a bar shows
export interface EolRunwayTip {
  label: string;
  // "2027-04-30 (in 5 months)"
  eol: string;
  eoas?: string;
  securityFixesOnly: boolean;
  // the end of life is near (see EolRunwayComponent.riskDays)
  endsSoon: boolean;
}

export interface EolRunwayTick {
  label: string;
  x: number;
}
