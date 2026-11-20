export interface MeterGroupSegment {
  label: string;
  value: number;
  // any css color
  color: string;
  // the color of the value written in the segment (showValues); by default the theme's meter text color
  textColor?: string;
}

export interface MeterGroupBar {
  segment: MeterGroupSegment;
  // px, in the svg
  x: number;
  width: number;
  // the share of the segment: of max when there is one, of the sum of the segments otherwise
  sharePercent: number;
  // the value fits in the segment
  showValue: boolean;
}

// the context of the popover of a segment
export interface MeterGroupTipContext {
  label: string;
  value: number;
  // the max input (undefined for a full meter)
  max: number | undefined;
  sharePercent: number;
  color: string;
  segmentIndex: number;
}
