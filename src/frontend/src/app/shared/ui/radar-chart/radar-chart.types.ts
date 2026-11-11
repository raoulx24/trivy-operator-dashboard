// one spoke of the radar
export interface RadarChartAxis {
  // written next to the end of the spoke
  label: string;
  // 0 (the centre) .. 1 (the outer ring); out of range values are clamped
  value: number;
}

// an axis placed on the chart (viewBox units, the centre is 0,0)
export interface RadarChartPoint {
  index: number;
  axis: RadarChartAxis;
  // the vertex
  x: number;
  y: number;
  // the end of the spoke
  endX: number;
  endY: number;
  labelX: number;
  labelY: number;
  labelAnchor: 'start' | 'middle' | 'end';
  labelBaseline: 'auto' | 'central' | 'hanging';
}
