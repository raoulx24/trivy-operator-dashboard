export interface RingGaugeData {
  // returned by the control when it is selected (the model), so the parent knows which ring it is
  id: string;
  // 0 - 100 (a percent); out of range is clamped
  value: number;
  // any css color, of the arc
  color: string;
  // the legend, under the ring
  label: string;
  // the text in the middle; by default the value as a percent, e.g. "88%"
  centerText?: string;
}
