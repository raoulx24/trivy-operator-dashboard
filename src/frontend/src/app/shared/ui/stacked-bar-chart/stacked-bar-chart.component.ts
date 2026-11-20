import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  TemplateRef,
  afterNextRender,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';

import { PopoverService } from '../popover/popover.service';

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

// a column chart drawn in svg: one bar per column, each bar stacked by series (the first series at the bottom)
// - everything comes from the inputs: columns (label + one value per series), series (name + color)
// - the bars share one scale: the largest total is the full height; a column without a value is a dash on the baseline
// - highlightFrom (optional) puts a track behind the columns from that index to the end
// - the popover shows the label, the total and the layers of the column (heatmap-style: it follows the mouse)
// - the control does not know what the columns or the series mean
@Component({
  selector: 'app-stacked-bar-chart',
  standalone: true,
  templateUrl: './stacked-bar-chart.component.html',
  styleUrl: './stacked-bar-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StackedBarChartComponent {
  columns = input<StackedBarColumn[]>([]);
  series = input<StackedBarSeries[]>([]);
  // the first column of the highlighted range (to the end); null = no highlight
  highlightFrom = input<number | null>(null);

  // px
  height = input<number>(56);
  gap = input<number>(4);

  // the columns with a value can be clicked (columnClicked)
  clickable = input<boolean>(false);
  // the text of the popover after the total, e.g. "ids"
  valueLabel = input<string>('');
  // replaces the default popover; its context is a StackedBarTipContext
  tooltipTemplate = input<TemplateRef<{ $implicit: StackedBarTipContext }> | undefined>(undefined);

  columnClicked = output<StackedBarClick>();

  protected readonly popover = inject(PopoverService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly width = signal(0);
  protected readonly dashHeight = DASH_HEIGHT;
  protected readonly trackRadius = TRACK_RADIUS;

  constructor() {
    const destroyRef = inject(DestroyRef);
    // never leave the popover behind when the chart goes away while hovered
    destroyRef.onDestroy(() => this.popover.hide());

    // the svg is drawn in px: it follows the width of the host
    afterNextRender(() => {
      const element = this.host.nativeElement;
      this.width.set(element.clientWidth);
      const observer = new ResizeObserver(() => this.width.set(element.clientWidth));
      observer.observe(element);
      destroyRef.onDestroy(() => observer.disconnect());
    });
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
