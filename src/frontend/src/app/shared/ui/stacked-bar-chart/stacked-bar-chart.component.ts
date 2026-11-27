import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';

import { CHART_TIP_SOURCE, ChartTip } from '../popover/chart-tip';

import {
  StackedBarBar,
  StackedBarClick,
  StackedBarColumn,
  StackedBarSeries,
  StackedBarTipContext,
} from './stacked-bar-chart.types';

// the radius of the top corners of a bar, and of the track
const BAR_RADIUS = 3;
const TRACK_RADIUS = 4;
// the dash of a column without a value
const DASH_HEIGHT = 2;
// the longest a bar takes to grow (ms)
const MAX_GROW = 500;

// a column chart drawn in svg: one bar per column, each bar stacked by series (the first series at the bottom)
// - everything comes from the inputs: columns (label + one value per series), series (name + color)
// - the bars share one scale: the largest total is the full height; a column without a value is a dash on the baseline
// - highlightFrom (optional) puts a track behind the columns from that index to the end
// - hovering a column emits tipShow / tipMove / tipHide (a StackedBarTipContext: the label, the total and the layers)
// - animate: when new data arrives the bars grow up from the baseline, one after the other in a random order of columns, all
//   within animationDuration (ms) in total; the same data does not play again (a resize does not); the dashes of the
//   columns without a value and the highlight are always there
// - the control does not know what the columns or the series mean
@Component({
  selector: 'app-stacked-bar-chart',
  standalone: true,
  templateUrl: './stacked-bar-chart.component.html',
  styleUrl: './stacked-bar-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: CHART_TIP_SOURCE, useExisting: StackedBarChartComponent }],
})
export class StackedBarChartComponent {
  columns = input<StackedBarColumn[]>([]);
  series = input<StackedBarSeries[]>([]);
  // the first column of the highlighted range (to the end); null = no highlight
  highlightFrom = input<number | null>(null);

  // the bars grow when the data is received; the whole animation takes animationDuration ms
  animate = input<boolean>(false);
  animationDuration = input<number>(900);
  // anything that changes when the owner receives data again: the animation plays again for the same data too (e.g. a
  // refresh); without it only changed data plays
  replay = input<unknown>(undefined);

  // px
  height = input<number>(56);
  gap = input<number>(4);

  // the columns with a value can be clicked (columnClicked)
  clickable = input<boolean>(false);

  columnClicked = output<StackedBarClick>();

  // the popover of a column is up to the owner (see ChartTipDirective)
  readonly tipShow = output<ChartTip<StackedBarTipContext>>();
  readonly tipMove = output<MouseEvent>();
  readonly tipHide = output<void>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly width = signal(0);
  protected readonly dashHeight = DASH_HEIGHT;
  protected readonly trackRadius = TRACK_RADIUS;

  constructor() {
    const destroyRef = inject(DestroyRef);
    // the svg is drawn in px: it follows the width of the host
    afterNextRender(() => {
      const element = this.host.nativeElement;
      this.width.set(element.clientWidth);
      const observer = new ResizeObserver(() => this.width.set(element.clientWidth));
      observer.observe(element);
      destroyRef.onDestroy(() => observer.disconnect());
    });
  }

  // how long one bar takes to grow (the last one starts at animationDuration minus this, so everything is done within
  // animationDuration)
  protected readonly grow = computed(() => Math.min(MAX_GROW, Math.max(0, this.animationDuration())));

  // what the growth depends on: the values and the colors (not the width); the delays are kept while it is unchanged, as a
  // changed delay would restart the growth of a bar that has already grown
  private delayCache?: { key: string; id: number; delays: Map<number, number> };
  private delayCacheId = 0;

  // per column, in ms: a random order of the columns with a value, evenly spread over the time left after the growth of the
  // last one
  private delaysOf(columns: StackedBarColumn[], series: StackedBarSeries[], key: string) {
    if (this.delayCache?.key === key) return this.delayCache;

    const order = columns
      .map((column, i) => ({ i, total: series.reduce((sum, _, s) => sum + Math.max(0, column.values[s] ?? 0), 0) }))
      .filter((x) => x.total > 0)
      .map((x) => x.i);
    // Fisher-Yates
    for (let i = order.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [order[i], order[j]] = [order[j], order[i]];
    }

    const spread = Math.max(0, this.animationDuration() - this.grow());
    const delays = new Map(order.map((column, i) => [column, order.length > 1 ? (i / (order.length - 1)) * spread : 0]));
    this.delayCache = { key, id: ++this.delayCacheId, delays };
    return this.delayCache;
  }

  protected readonly bars = computed<StackedBarBar[]>(() => {
    const series = this.series();
    const columns = this.columns();
    const from = this.highlightFrom();
    const height = this.height();
    const count = columns.length;
    const slot = count > 0 ? Math.max(0, (this.width() - this.gap() * (count - 1)) / count) : 0;

    const totals = columns.map((column) => series.reduce((sum, _, i) => sum + Math.max(0, column.values[i] ?? 0), 0));
    const max = Math.max(0, ...totals);
    const dataKey = JSON.stringify([
      this.animationDuration(),
      this.replay(),
      series.map((s) => s.color),
      columns.map((c) => [c.label, c.values]),
    ]);
    const lit = this.animate() ? this.delaysOf(columns, series, dataKey) : undefined;

    return columns.map((column, index) => {
      const values = series
        .map((s, i) => ({ series: s, value: Math.max(0, column.values[i] ?? 0) }))
        .filter((layer) => layer.value > 0);
      const x = index * (slot + this.gap());

      // from the baseline up
      let top = height;
      const layers = values.map((layer, i) => {
        const h = max > 0 ? (layer.value / max) * height : 0;
        top -= h;
        return { ...layer, path: this.layerPath(x, top, slot, h, i === values.length - 1 ? BAR_RADIUS : 0) };
      });

      return {
        index,
        column,
        total: totals[index],
        x,
        width: slot,
        layers,
        highlighted: from !== null && index >= from,
        key: `${lit?.id ?? 0}|${index}`,
        growDelay: lit?.delays.get(index) ?? null,
      };
    });
  });

  protected onBarClick(bar: StackedBarBar): void {
    if (!this.clickable() || bar.total <= 0) return;
    this.columnClicked.emit({ columnIndex: bar.index, column: bar.column, total: bar.total });
  }

  protected tipContext(bar: StackedBarBar): StackedBarTipContext {
    return { label: bar.column.label, total: bar.total, layers: bar.layers, columnIndex: bar.index };
  }

  // a rectangle with the two top corners rounded by `radius` (0 = square)
  private layerPath(x: number, y: number, width: number, height: number, radius: number): string {
    const r = Math.min(radius, width / 2, height);
    return (
      `M${x},${y + height} L${x},${y + r} Q${x},${y} ${x + r},${y} L${x + width - r},${y} ` +
      `Q${x + width},${y} ${x + width},${y + r} L${x + width},${y + height} Z`
    );
  }
}
