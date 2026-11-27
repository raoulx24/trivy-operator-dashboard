import { InjectionToken, OutputEmitterRef } from '@angular/core';

/** What a chart emits when an item (cell, bar, node, ...) is hovered: what the item is, and the mouse event. */
export interface ChartTip<T> {
  context: T;
  event: MouseEvent;
}

/**
 * The 3 outputs a chart exposes for its data popovers. The chart draws no popover: the owner decides what to show
 * (see ChartTipDirective).
 */
export interface ChartTipSource<T = unknown> {
  /** The mouse entered an item. */
  readonly tipShow: OutputEmitterRef<ChartTip<T>>;
  /** The mouse moved over the item (for the popovers that follow it). */
  readonly tipMove: OutputEmitterRef<MouseEvent>;
  /** The mouse left the item. */
  readonly tipHide: OutputEmitterRef<void>;
}

/** Every chart with data popovers provides itself under this token: `{ provide: CHART_TIP_SOURCE, useExisting: X }`. */
export const CHART_TIP_SOURCE = new InjectionToken<ChartTipSource>('CHART_TIP_SOURCE');
